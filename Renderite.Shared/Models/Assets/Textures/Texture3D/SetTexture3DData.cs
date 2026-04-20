using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    public class SetTexture3DData : AssetCommand
    {
        /// <summary>
        /// Buffer containing the full texture data. This needs to match the currently set texture format
        /// </summary>
        
        public SharedMemoryBufferDescriptor<byte> data;

        /// <summary>
        /// Hint for uploading this texture data. This allow only a region of the texture data to be updated when
        /// only a portion of the texture has changed, leading to better efficiency.
        /// </summary>
        
        public Texture3DUploadHint hint;

        /// <summary>
        /// If it should use high priority integration
        /// </summary>
        
        public bool highPriority;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(data);
            packer.Write(hint);
            packer.Write(highPriority);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref data);
            packer.Read(ref hint);
            packer.Read(ref highPriority);
        }
    }
}
