using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    public delegate void ListReader<T>(ref MemoryUnpacker unpacker, ref List<T> list);

    public ref struct MemoryUnpacker
    {
        public IMemoryPackerEntityPool Pool { get; private set; }
        public int RemainingData => buffer.Length;

        ReadOnlySpan<byte> buffer;

        public MemoryUnpacker(ReadOnlySpan<byte> buffer, IMemoryPackerEntityPool pool)
        {
            this.buffer = buffer;
            this.Pool = pool;
        }

        public unsafe ReadOnlySpan<T> Access<T>(int count)
            where T : unmanaged
        {
            var size = count * sizeof(T);

            var chunk = MemoryMarshal.Cast<byte, T>(buffer).Slice(0, count);

            buffer = buffer.Slice(size);

            return chunk;
        }

        public void Read(ref string str) => str = ReadString();

        public unsafe string ReadString()
        {
            var length = Read<int>();

            if (length < 0)
                return null;

            if (length == 0)
                return "";

            var chars = Access<char>(length);

            fixed (char* charsPtr = chars)
                return new string(charsPtr, 0, length);
        }
        public unsafe T Read<T>()
            where T : unmanaged
        {
            return Access<T>(1)[0];
        }

        public unsafe void Read<T>(ref T? target)
            where T : unmanaged
        {
            var hasValue = Read<bool>();

            if (hasValue)
                target = Read<T>();
            else
                target = null;
        }

        public unsafe void Read<T>(ref T target)
            where T : unmanaged
        {
            target = Access<T>(1)[0];
        }

        public void Read(out bool bit0, out bool bit1) =>
            Read(out bit0, out bit1, out _, out _, out _, out _, out _, out _);

        public void Read(out bool bit0, out bool bit1, out bool bit2) =>
            Read(out bit0, out bit1, out bit2, out _, out _, out _, out _, out _);

        public void Read(out bool bit0, out bool bit1, out bool bit2, out bool bit3) =>
            Read(out bit0, out bit1, out bit2, out bit3, out _, out _, out _, out _);

        public void Read(out bool bit0, out bool bit1, out bool bit2, out bool bit3, out bool bit4) =>
            Read(out bit0, out bit1, out bit2, out bit3, out bit4, out _, out _, out _);

        public void Read(out bool bit0, out bool bit1, out bool bit2, out bool bit3, out bool bit4, out bool bit5) =>
            Read(out bit0, out bit1, out bit2, out bit3, out bit4, out bit5, out _, out _);

        public void Read(out bool bit0, out bool bit1, out bool bit2, out bool bit3, out bool bit4, out bool bit5, out bool bit6) =>
            Read(out bit0, out bit1, out bit2, out bit3, out bit4, out bit5, out bit6, out _);

        public void Read(out bool bit0, out bool bit1, out bool bit2, out bool bit3, out bool bit4, out bool bit5, out bool bit6, out bool bit7)
        {
            var value = Read<byte>();

            bit0 = (value & 0b00000001) != 0;
            bit1 = (value & 0b00000010) != 0;
            bit2 = (value & 0b00000100) != 0;
            bit3 = (value & 0b00001000) != 0;
            bit4 = (value & 0b00010000) != 0;
            bit5 = (value & 0b00100000) != 0;
            bit6 = (value & 0b01000000) != 0;
            bit7 = (value & 0b10000000) != 0;
        }

        public void ReadObject<T>(ref T? obj)
            where T : class, IMemoryPackable, new()
        {
            var hasObject = Read<bool>();

            if (hasObject)
            {
                if (obj == null)
                    obj = Pool.Borrow<T>();

                obj.Unpack(ref this);
            }
            else
                obj = null;
        }

        public void ReadObjectList<T>(ref List<T> list)
            where T : class, IMemoryPackable, new()
        {
            var count = Read<int>();

            if (count > 0)
            {
                if (list == null)
                    list = new List<T>();

                for (int i = 0; i < count; i++)
                {
                    T item;

                    if (list.Count == i)
                    {
                        item = Pool.Borrow<T>();
                        list.Add(item);
                    }
                    else
                        item = list[i];

                    item.Unpack(ref this);
                }
            }

            // Remove any excess items
            if (list != null)
                while (list.Count > count)
                {
                    var lastIndex = list.Count - 1;

                    Pool.Return(list[lastIndex]);
                    list.RemoveAt(list.Count - 1);
                }
        }

        public void ReadPolymorphicList<T>(ref List<T> list)
            where T : PolymorphicMemoryPackableEntity<T>
        {
            var count = Read<int>();

            if (count > 0)
            {
                if (list == null)
                    list = new List<T>();

                for (int i = 0; i < count; i++)
                {
                    if (list.Count == i)
                        list.Add(PolymorphicMemoryPackableEntity<T>.Decode(ref this));
                    else
                        list[i] = PolymorphicMemoryPackableEntity<T>.Decode(ref this, list[i]);
                }
            }

            // Remove any excess items
            if (list != null)
                while (list.Count > count)
                {
                    var lastIndex = list.Count - 1;

                    PolymorphicMemoryPackableEntity<T>.ReturnAuto(Pool, list[lastIndex]);
                    list.RemoveAt(list.Count - 1);
                }
        }

        public void ReadValueList<T>(ref List<T> list) where T : unmanaged => ReadValueList<List<T>, T>(ref list);
        public void ReadValueList<T>(ref HashSet<T> list) where T : unmanaged => ReadValueList<HashSet<T>, T>(ref list);

        public void ReadValueList<C, T>(ref C list)
            where C : ICollection<T>, new()
            where T : unmanaged
        {
            var count = Read<int>();

            if (count > 0)
            {
                if (list == null)
                    list = new C();

                list.Clear();

                var values = Access<T>(count);

                for (int i = 0; i < values.Length; i++)
                    list.Add(values[i]);
            }
            else
                list?.Clear();

        }

        public void ReadStringList(ref List<string> list)
        {
            var count = Read<int>();

            if (count > 0)
            {
                if (list == null)
                    list = new List<string>();

                list.Clear();

                for (int i = 0; i < count; i++)
                    list.Add(ReadString());
            }
            else
                list?.Clear();
        }

        public void ReadNestedValueList<T>(ref List<List<T>> list) where T : unmanaged =>
            ReadNestedList(ref list, delegate (ref MemoryUnpacker unpacker, ref List<T> sublist) { unpacker.ReadValueList(ref sublist); });

        public void ReadNestedList<T>(ref List<List<T>> list, ListReader<T> reader)
        {
            var count = Read<int>();

            if (count > 0)
            {
                if (list == null)
                    list = new List<List<T>>();

                for (int i = 0; i < count; i++)
                {
                    List<T> subList;

                    if (list.Count == i)
                    {
                        subList = null;
                        list.Add(subList);
                    }
                    else
                        subList = list[i];

                    reader(ref this, ref subList);

                    list[i] = subList;
                }
            }

            // Remove any excess items
            if (list != null)
                while (list.Count > count)
                    list.RemoveAt(list.Count - 1);
        }
    }
}
