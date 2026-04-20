using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SetCubemapFormat : AssetCommand
    {
        /// <summary>
        /// The size of each side of the face of the cubemap. The faces must be square, so the size is shared.
        /// </summary>
        
        public int size;

        /// <summary>
        /// How many mipmaps this texture should have
        /// </summary>
        
        public int mipmapCount;

        /// <summary>
        /// Format of the texture data
        /// </summary>
        
        public TextureFormat format;

        /// <summary>
        /// The color profile of this texture
        /// </summary>
        
        public ColorProfile profile;

        public void Validate()
        {
            if (format.IsHDR() && profile != ColorProfile.Linear)
                throw new InvalidOperationException($"Cubemap {assetId} uses HDR format {format}, but non-Linear color profile {profile}");
        }

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(size);
            packer.Write(mipmapCount);
            packer.Write(format);
            packer.Write(profile);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref size);
            packer.Read(ref mipmapCount);
            packer.Read(ref format);
            packer.Read(ref profile);
        }
    }
}
