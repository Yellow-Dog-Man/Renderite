using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    public class SetTexture3DResult : AssetCommand
    {
        /// <summary>
        /// Indicates what is this result for.
        /// </summary>
        
        public TextureUpdateResultType type;

        /// <summary>
        /// Indicates if the engine instance has changed
        /// </summary>
        
        public bool instanceChanged;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(type);
            packer.Write(instanceChanged);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref type);
            packer.Read(ref instanceChanged);
        }
    }
}
