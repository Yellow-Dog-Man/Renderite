using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Unity
{
    public class SharedMemoryAccessor
    {
        public string Prefix { get; private set; }

        Dictionary<int, SharedMemoryView> _views = new Dictionary<int, SharedMemoryView>();

        public SharedMemoryAccessor(string prefix)
        {
            this.Prefix = prefix;
        }

        public Span<T> AccessData<T>(SharedMemoryBufferDescriptor<T> descriptor)
            where T : unmanaged
        {
            try
            {
                var view = GetMemoryView(descriptor);

                var buffer = view.RawData.Slice(descriptor.offset, descriptor.length);

                return MemoryMarshal.Cast<byte, T>(buffer);
            }
            catch(ArgumentOutOfRangeException ex)
            {
                UnityEngine.Debug.LogError($"Out of range exception. " +
                    $"Offset: {descriptor.offset}, Length: {descriptor.length}, BufferCapacity: {descriptor.bufferCapacity}, BufferId: {descriptor.bufferId}");
                throw;
            }
        }

        public UnmanagedSpan<T> AccessDataUnmanaged<T>(SharedMemoryBufferDescriptor<T> descriptor)
            where T : unmanaged
        {
            var view = GetMemoryView(descriptor);

            var buffer = view.UnmanagedRawData.Slice(descriptor.offset, descriptor.length);

            return buffer.As<T>();
        }

        public SharedMemoryViewSlice<T> AccessSlice<T>(SharedMemoryBufferDescriptor<T> descriptor)
            where T : unmanaged
        {
            var view = GetMemoryView(descriptor);

            return new SharedMemoryViewSlice<T>(view, descriptor.offset, descriptor.length);
        }

        SharedMemoryView GetMemoryView<T>(SharedMemoryBufferDescriptor<T> descriptor)
            where T : unmanaged
        {
            lock (_views)
            {
                if (!_views.TryGetValue(descriptor.bufferId, out var view))
                {
                    view = new SharedMemoryView(this, descriptor.bufferId, descriptor.bufferCapacity);
                    _views.Add(descriptor.bufferId, view);
                }

                return view;
            }
        }

        public void ReleaseView(int bufferId)
        {
            lock (_views)
            {
                if (_views.TryGetValue(bufferId, out var view))
                {
                    view.Dispose();
                    _views.Remove(bufferId);
                }
            }
        }
    }
}
