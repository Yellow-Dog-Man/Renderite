using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Unity
{
    public class SharedMemoryViewSlice<T> : BackingMemoryBuffer
        where T : unmanaged
    {
        public SharedMemoryView SharedView { get; private set; }
        public int OffsetBytes { get; private set; }
        public override int SizeBytes => _sizeBytes;

        public override Span<byte> RawData => SharedView.RawData.Slice(OffsetBytes, SizeBytes);
        public Span<T> Data => MemoryMarshal.Cast<byte, T>(RawData);
        public override Memory<byte> Memory => SharedView.Memory.Slice(OffsetBytes, SizeBytes);

        int _sizeBytes;

        public SharedMemoryViewSlice(SharedMemoryView view,  int offset, int size)
        {
            this.SharedView = view;
            this.OffsetBytes = offset;
            _sizeBytes = size;
        }

        protected override void ActuallyDispose()
        {
            // IMPORTANT!!! We don't actually dispose the view, because it's shared across multiple
            // view slices. We let the memory management system dispose of these.
            SharedView = null;
        }
    }
}
