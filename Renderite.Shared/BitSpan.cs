using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public ref struct BitSpan
    {
        /// <summary>
        /// Computes the minimum necessary length of a raw underlying buffer needed to accommodate the total number of bits.
        /// </summary>
        /// <param name="totalBits">How many bits must the buffer be able to accommodate.</param>
        /// <returns>Length of the underlying buffer</returns>
        public static int ComputeMinimumBufferLength(int totalBits) => (totalBits + BITS_IN_ELEMENT - 1) / BITS_IN_ELEMENT;

        const int BITS_IN_ELEMENT = sizeof(uint) * 8;

        public int Length => data.Length * 8;

        Span<uint> data;

        public BitSpan(Span<uint> data)
        {
            this.data = data;
        }

        public bool this[int bitIndex]
        {
            get
            {
                var elementIndex = bitIndex / BITS_IN_ELEMENT;
                bitIndex %= BITS_IN_ELEMENT;

                var e = data[elementIndex];

                return (e & (1U << bitIndex)) != 0;
            }

            set
            {
                var elementIndex = bitIndex / BITS_IN_ELEMENT;
                bitIndex %= BITS_IN_ELEMENT;

                ref var e = ref data[elementIndex];

                var mask = 1U << bitIndex;

                if (value)
                    e |= mask;
                else
                    e &= ~mask;
            }
        }
    }
}
