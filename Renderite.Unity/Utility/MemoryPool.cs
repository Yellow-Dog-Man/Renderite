using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public interface IPoolable
    {
        void Clean();
    }

    public static class MemoryPool
    {
        public static T Borrow<T>() where T : IPoolable, new() => MemoryPool<T>.Borrow();
        public static void Return<T>(ref T instance) where T : IPoolable, new() => MemoryPool<T>.Return(ref instance);
    }

    public static class MemoryPool<T>
        where T : IPoolable, new()
    {
        static Stack<T> _instances = new Stack<T>();

        public static T Borrow()
        {
            lock(_instances)
            {
                if (_instances.Count == 0)
                    return new T();
                else
                    return _instances.Pop();
            }
        }

        public static void Return(ref T instance)
        {
            instance.Clean();

            lock(_instances)
                _instances.Push(instance);

            instance = default;
        }
    }
}
