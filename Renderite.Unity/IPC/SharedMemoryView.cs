using System;
using Cloudtoid.Interprocess;
using Microsoft.Extensions.Logging.Abstractions;
using Renderite.Shared;

namespace Renderite.Unity
{
    public class SharedMemoryView : IDisposable
    {
        public SharedMemoryAccessor Accessor { get; private set; }
        public int BufferId { get; private set; }

        public Span<byte> RawData => view.Data;
        public Memory<byte> Memory => memory.Memory;
        public unsafe UnmanagedSpan<byte> UnmanagedRawData => new UnmanagedSpan<byte>(view.Pointer, (int)capacity);

        MemoryView view;
        UnmanagedMemoryManager<byte> memory;
        long capacity;

        public SharedMemoryView(SharedMemoryAccessor accessor, int bufferId, long capacity)
        {
            this.Accessor = accessor;
            this.BufferId = bufferId;

            this.capacity = capacity;

            var name = Renderite.Shared.Helper.ComposeMemoryViewName(accessor.Prefix, bufferId);

            view = new MemoryView(new MemoryViewOptions(name, capacity, false), NullLoggerFactory.Instance);

            unsafe
            {
                memory = new UnmanagedMemoryManager<byte>(view.Pointer, (int)capacity);
            }
        }

        public void Dispose()
        {
            view.Dispose();
            view = null;
        }
    }
}
