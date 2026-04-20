using System;
using System.Threading;
using System.Threading.Tasks;
using Cloudtoid.Interprocess;

namespace Renderite.Shared
{
    public delegate void RenderCommandHandler(RendererCommand command, int messageSize);

    public class MessagingManager : IDisposable
    {
        // 8 MB, eyeballed
        public const int DEFAULT_CAPACITY = 1024 * 1024 * 8;

        public int ReceivedMessages { get; private set; }
        public int SentMessages { get; private set; }

        public RenderCommandHandler CommandHandler;
        public Action<Exception> FailureHandler;
        public Action<string> WarningHandler;

        IPublisher _publisher;
        ISubscriber _subscriber;

        object _lock = new object();

        IMemoryPackerEntityPool _pool;

        CancellationTokenSource cancellation;

        Memory<byte> writerBuffer;
        Memory<byte> receiverBuffer;

        Thread receiverThread;

        public MessagingManager(IMemoryPackerEntityPool pool)
        {
            this._pool = pool;
        }

        public void Connect(string queueName, bool isAuthority, long capacity = DEFAULT_CAPACITY)
        {
            // TODO!!! Do we need the whole capacity? Are we expected to have messages that big?
            writerBuffer = new Memory<byte>(new byte[capacity]);
            receiverBuffer = new Memory<byte>(new byte[capacity]);

            var factory = new QueueFactory();

            QueueOptions publisherOptions;

            QueueOptions subscriberOptions;

            publisherOptions = new QueueOptions(
                queueName: queueName + (isAuthority ? "A" : "S"),
                capacity: capacity,
                deleteOnDispose: isAuthority);

            subscriberOptions = new QueueOptions(
            queueName: queueName + (isAuthority ? "S" : "A"),
            capacity: capacity,
            deleteOnDispose: isAuthority);

            _publisher = factory.CreatePublisher(publisherOptions);
            _subscriber = factory.CreateSubscriber(subscriberOptions);

            StartProcessing();
        }

        void StartProcessing()
        {
            cancellation = new CancellationTokenSource();

            // Run receiver logic
            receiverThread = new Thread(ReceiverLogic);

            receiverThread.Priority = ThreadPriority.Highest;
            receiverThread.IsBackground = true;

            receiverThread.Start();
        }

        public void SendCommand(RendererCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            try
            {
                if (_publisher == null)
                    throw new InvalidOperationException("Cannot send commands when publisher is gone!");

                lock (_lock)
                {
                    // Serialize the command into the buffer

                    // Reset the buffer to be empty
                    var writer = new MemoryPacker(writerBuffer.Span);

                    command.Encode(ref writer);

                    var length = writer.ComputeLength(writerBuffer.Span);

                    if (length == 0)
                        throw new Exception($"Serializing command resulted in zero length. Command: {command}");

                    // Get the buffer
                    var buffer = writerBuffer.Span.Slice(0, length);

                    while (!_publisher.TryEnqueue(buffer))
                    {
                        WarningHandler("Queue doesn't have enough free capacity, stalling message writing!");

                        // If we fail to enqueue, sleep a bit to wait
                        // TODO!!! Exponential back off?
                        // Add breaking logic if it stalls for way too long
                        System.Threading.Thread.Sleep(10);
                    }

                    SentMessages++;
                }
            }
            catch (Exception ex)
            {
                FailureHandler?.Invoke(ex);
                throw;
            }
        }

        public void StartKeepAlive(int intervalMilliseconds = 50, Action onSend = null)
            => Task.Run(async () => await KeepAliveHandler(intervalMilliseconds, onSend));

        async Task KeepAliveHandler(int intervalMilliseconds, Action onSend = null)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(intervalMilliseconds)).ConfigureAwait(false);

                SendCommand(new KeepAlive());

                onSend?.Invoke();
            }
        }

        void ReceiverLogic()
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var message = _subscriber.Dequeue(receiverBuffer, cancellation.Token);

                    ReceivedMessages++;

                    if (cancellation.IsCancellationRequested)
                        break;

                    // Ignore empty messages
                    if (message.IsEmpty)
                        continue;

                    var size = message.Length;

                    var packer = new MemoryUnpacker(message.Span, _pool);

                    RendererCommand command = null;

                    try
                    {
                        command = RendererCommand.Decode(ref packer);

                        // Process the command
                        CommandHandler?.Invoke(command, size);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failure processing message. Message length: {message.Span.Length}. Remaining data: {packer.RemainingData}, Command: {command}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                FailureHandler?.Invoke(ex);
            }
            finally
            {
                _publisher.Dispose();
                _subscriber.Dispose();

                _publisher = null;
                _subscriber = null;
            }
        }

        public void Dispose()
        {
            cancellation?.Cancel();
        }
    }
}
