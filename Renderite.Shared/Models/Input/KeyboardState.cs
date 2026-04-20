using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class KeyboardState : IMemoryPackable
    {
        
        public string typeDelta;

        
        public HashSet<Key> heldKeys = new HashSet<Key>();

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(typeDelta);
            packer.WriteValueList(heldKeys);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref typeDelta);
            packer.ReadValueList(ref heldKeys);
        }
    }
}
