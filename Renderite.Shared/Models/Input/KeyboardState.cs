using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class KeyboardState : IMemoryPackable
    {
        public string? typeDelta;
        public HashSet<Key> heldKeys = new HashSet<Key>();
        public bool compositionActive;
        public string? compositionText;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(typeDelta);
            packer.WriteValueList(heldKeys);
            packer.Write(compositionActive);
            packer.Write(compositionText);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref typeDelta);
            packer.ReadValueList(ref heldKeys);
            packer.Read(ref compositionActive);
            packer.Read(ref compositionText);
        }
    }
}
