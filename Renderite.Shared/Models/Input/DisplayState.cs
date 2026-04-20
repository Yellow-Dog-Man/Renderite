using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class DisplayState : IMemoryPackable
    {
        public int displayIndex;

        public RenderVector2i resolution;
        public RenderVector2i offset;
        public double refreshRate;
        public RectOrientation orientation;
        public RenderVector2 dpi;

        public bool isPrimary;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(displayIndex);

            packer.Write(resolution);
            packer.Write(offset);
            packer.Write(refreshRate);
            packer.Write(orientation);
            packer.Write(dpi);

            packer.Write(isPrimary);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref displayIndex);

            unpacker.Read(ref resolution);
            unpacker.Read(ref offset);
            unpacker.Read(ref refreshRate);
            unpacker.Read(ref orientation);
            unpacker.Read(ref dpi);

            unpacker.Read(ref isPrimary);
        }
    }
}
