using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class PackerMemoryPool : IMemoryPackerEntityPool
    {
        public static readonly PackerMemoryPool Instance = new PackerMemoryPool();

        public T Borrow<T>() where T : class, IMemoryPackable, new() => PackerMemoryPool<T>.Borrow();
        public void Return<T>(T value) where T : class, IMemoryPackable, new() => PackerMemoryPool<T>.Return(value);
    }

    public static class PackerMemoryPool<T>
        where T : class, IMemoryPackable, new()
    {
        static Stack<T> _instances = new Stack<T>();

        public static T Borrow()
        {
            lock (_instances)
            {
                if (_instances.Count == 0)
                    return new T();
                else
                    return _instances.Pop();
            }
        }

        public static void Return(T instance)
        {
            lock (_instances)
                _instances.Push(instance);

            instance = default;
        }
    }
}
