using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class DragAndDropEvent : IMemoryPackable
    {
        
        public List<string> paths;

        
        public RenderVector2i dropPoint;

        public void Pack(ref MemoryPacker packer)
        {
            packer.WriteStringList(paths);
            packer.Write(dropPoint);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.ReadStringList(ref paths);
            unpacker.Read(ref dropPoint);
        }
    }
}
