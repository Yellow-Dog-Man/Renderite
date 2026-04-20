using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SetTexture2DFormat : AssetCommand
    {
        /// <summary>
        /// Width of the texture
        /// </summary>
        
        public int width;

        /// <summary>
        /// Height of the texture
        /// </summary>
        
        public int height;

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
                throw new InvalidOperationException($"2D Texture {assetId} uses HDR format {format}, but non-Linear color profile {profile}");
        }

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(width);
            packer.Write(height);
            packer.Write(mipmapCount);
            packer.Write(format);
            packer.Write(profile);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref width);
            packer.Read(ref height);
            packer.Read(ref mipmapCount);
            packer.Read(ref format);
            packer.Read(ref profile);
        }
    }
}
