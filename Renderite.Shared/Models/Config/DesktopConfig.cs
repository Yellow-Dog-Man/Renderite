using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class DesktopConfig : RendererCommand
    {
        public int? maximumBackgroundFramerate;
        public int? maximumForegroundFramerate;
        public bool vSync;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(maximumBackgroundFramerate);
            packer.Write(maximumForegroundFramerate);
            packer.Write(vSync);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref maximumBackgroundFramerate);
            packer.Read(ref maximumForegroundFramerate);
            packer.Read(ref vSync);
        }
    }
}
