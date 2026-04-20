using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public interface IMemoryPackerEntityPool
    {
        T Borrow<T>() where T : class, IMemoryPackable, new();
        void Return<T>(T value) where T : class, IMemoryPackable, new();
    }
}
