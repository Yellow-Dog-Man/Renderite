using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class ArrayBackingMemoryBuffer : BackingMemoryBuffer
    {
        public byte[] Array { get; private set; }

        public override int SizeBytes => Array.Length;

        public override Span<byte> RawData => Array.AsSpan();

        public override Memory<byte> Memory => Array.AsMemory();

        public ArrayBackingMemoryBuffer(byte[] array)
        {
            this.Array = array;
        }

        protected override void ActuallyDispose()
        {
            Array = null;
        }
    }
}
