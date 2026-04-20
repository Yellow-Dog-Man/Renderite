using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// This is a Span-like struct which assumes that the data passed to it is allocated on unmanaged memory.
    /// WARNING: Memory corruption will occur if it's used with managed memory!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly unsafe struct UnmanagedSpan<T>
        where T : unmanaged
    {
        public int Length => size;

        public readonly T* data;
        public readonly int size;

        public UnmanagedSpan(T* data, int size)
        {
            this.data = data;
            this.size = size;
        }

        public T this[int index]
        {
            get
            {
                CheckIndex(index);
                return *(data + index);
            }

            set
            {
                CheckIndex(index);
                *(data + index) = value;
            }
        }

        void CheckIndex(int index)
        {
            if (index < 0 || index >= size)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        public UnmanagedSpan<T> Slice(int index)
        {
            if (index == size)
                return new UnmanagedSpan<T>(default, 0);

            CheckIndex(index);
            return new UnmanagedSpan<T>(data + index, size - index);
        }

        public UnmanagedSpan<T> Slice(int index, int count)
        {
            if(index == size)
            {
                if (count > 0)
                    throw new ArgumentOutOfRangeException(nameof(count));

                return new UnmanagedSpan<T>(default, 0);
            }

            CheckIndex(index);

            if (index + count > size)
                throw new ArgumentOutOfRangeException(nameof(count));

            return new UnmanagedSpan<T>(data + index, count);
        }

        public UnmanagedSpan<O> As<O>()
            where O : unmanaged
        {
            var oldElementSize = sizeof(T);
            var newElementSize = sizeof(O);

            var bytes = size * oldElementSize;
            var newSize = bytes / newElementSize;

            return new UnmanagedSpan<O>((O*)data, newSize);
        }
    }
}
