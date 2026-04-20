using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    public delegate void ListWriter<T>(ref MemoryPacker packer, List<T> list);

    public ref struct MemoryPacker
    {
        Span<byte> buffer;

        public int ComputeLength(Span<byte> originalBuffer) => originalBuffer.Length - buffer.Length;

        public MemoryPacker(Span<byte> buffer)
        {
            this.buffer = buffer;
        }

        public unsafe Span<T> Access<T>(int count)
            where T : unmanaged
        {
            var size = count * sizeof(T);

            var chunk = MemoryMarshal.Cast<byte, T>(buffer).Slice(0, count);

            buffer = buffer.Slice(size);

            return chunk;
        }

        public void Write(string str)
        {
            if(str == null)
            {
                Write(-1);
                return;
            }

            var chars = str.AsSpan();
            Write(chars.Length);

            var data = Access<char>(chars.Length);
            chars.CopyTo(data);
        }

        public unsafe void Write<T>(T? value)
            where T : unmanaged
        {
            if (value == null)
                Write(false);
            else
            {
                Write(true);
                Write(value.Value);
            }
        }

        public unsafe void Write<T>(T value)
            where T : unmanaged
        {
            Unsafe.WriteUnaligned<T>(ref buffer[0], value);
            buffer = buffer.Slice(sizeof(T));
        }

        public void Write(bool bit0, bool bit1, bool bit2 = false, bool bit3 = false, bool bit4 = false, bool bit5 = false, bool bit6 = false, bool bit7 = false)
        {
            Write((byte)(
                (bit0 ? 1 : 0) << 0 |
                (bit1 ? 1 : 0) << 1 |
                (bit2 ? 1 : 0) << 2 |
                (bit3 ? 1 : 0) << 3 |
                (bit4 ? 1 : 0) << 4 |
                (bit5 ? 1 : 0) << 5 |
                (bit6 ? 1 : 0) << 6 |
                (bit7 ? 1 : 0) << 7
                ));
        }

        public void WriteObject<T>(T? obj)
            where T : class, IMemoryPackable
        {
            if (obj == null)
                Write(false);
            else
            {
                Write(true);
                obj.Pack(ref this);
            }
        }

        public void WriteNestedValueList<T>(List<List<T>> list) where T : unmanaged =>
            WriteNestedList(list, delegate (ref MemoryPacker packer, List<T> sublist) { packer.WriteValueList(sublist); });

        public void WriteNestedList<T>(List<List<T>> list, ListWriter<T> sublistWriter)
        {
            var count = list?.Count ?? 0;

            Write(count);

            for (int i = 0; i < count; i++)
                sublistWriter(ref this, list[i]);
        }

        public void WriteObjectList<T>(List<T> list)
            where T : IMemoryPackable
        {
            var count = list?.Count ?? 0;

            Write(count);

            for (int i = 0; i < count; i++)
                list[i].Pack(ref this);
        }

        public void WritePolymorphicList<T>(List<T> list)
            where T : PolymorphicMemoryPackableEntity<T>
        {
            var count = list?.Count ?? 0;

            Write(count);

            for (int i = 0; i < count; i++)
                list[i].Encode(ref this);
        }

        public void WriteValueList<T>(List<T> list) where T : unmanaged => WriteValueList<List<T>, T>(list);
        public void WriteValueList<T>(HashSet<T> list) where T : unmanaged => WriteValueList<HashSet<T>, T>(list);

        public void WriteValueList<C, T>(C list)
            where C : ICollection<T>
            where T : unmanaged
        {
            var count = list?.Count ?? 0;

            Write(count);

            if(list != null)
                foreach (var v in list)
                    Write(v);
        }

        public void WriteStringList(List<string> list)
        {
            var count = list?.Count ?? 0;

            Write(count);

            for (int i = 0; i < count; i++)
                Write(list[i]);
        }
    }
}
