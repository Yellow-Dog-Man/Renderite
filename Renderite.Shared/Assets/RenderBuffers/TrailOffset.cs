using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct TrailOffset
    {
        // These control the size of the actual buffer for the particle data
        [FieldOffset(0)]
        public int offset;

        [FieldOffset(4)]
        public int capacity;

        // These indicate the start position within the buffer and number of points
        // This is because each trail itself is a circular buffer, which allows
        // adding and removing points during the trail's lifetime
        [FieldOffset(8)]
        public int start;

        [FieldOffset(12)]
        public int count;

        public void RemovePointFromBeginning()
        {
            start++;

            // Check if we need to wrap around
            if (start == capacity)
                start = 0;

            count--;
        }

        public int GetSubIndex(int pos) => (start + pos) % capacity;
        public int GetIndex(int pos) => offset + GetSubIndex(pos);

        public int LastPointSubIndex => GetSubIndex(count - 1);
        public int LastPointIndex => GetIndex(count - 1);
        public int SecondToLastPointSubIndex => GetSubIndex(count - 2);
        public int SecondToLastPointIndex => GetIndex(count - 2);

        public bool HasFreeCapacity => count < capacity;
        public bool WrapsAround => start + count > capacity;
        public int EndIndex => offset + capacity;

        public override string ToString() => $"Trail (Offset: {offset}, Capacity: {capacity}, Start: {start}, Count: {count})";
    }
}
