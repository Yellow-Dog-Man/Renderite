using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Renderite.Shared
{
    public class SetDesktopTextureProperties : AssetCommand
    {
        /// <summary>
        /// Index of the display
        /// </summary>
        public int displayIndex;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(displayIndex);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref displayIndex);
        }
    }
}
