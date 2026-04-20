using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public interface IMemoryPackable
    {
        void Pack(ref MemoryPacker packer);
        void Unpack(ref MemoryUnpacker unpacker);
    }
}
