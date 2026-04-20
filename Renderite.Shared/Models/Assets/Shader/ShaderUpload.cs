using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class ShaderUpload : AssetCommand
    {
        /// <summary>
        /// File containing this shader
        /// </summary>
        
        public string file;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(file);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref file);
        }
    }
}
