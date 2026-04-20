using Cloudtoid.Interprocess;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    public class VideoTextureAudioWriter : IDisposable
    {
        IPublisher _publisher;

        public VideoTextureAudioWriter(string queueName, int capacity)
        {
            var factory = new QueueFactory();

            // IMPORTANT!!! See the comment on the opposite side on why are we disposing this on this end
            var options = new QueueOptions(queueName, capacity, true);

            _publisher = factory.CreatePublisher(options);
        }

        public bool Write(Span<float> samples)
        {
            var rawData = MemoryMarshal.Cast<float, byte>(samples);
            return _publisher.TryEnqueue(rawData);
        }

        public void Dispose()
        {
            _publisher.Dispose();
            _publisher = null;
        }
    }
}
