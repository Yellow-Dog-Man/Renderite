using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SetRenderTextureFormat : AssetCommand
    {
        /// <summary>
        /// The size of the render texture in pixels
        /// </summary>
        
        public RenderVector2i size;

        /// <summary>
        /// Number of bits of depth
        /// </summary>
        
        public int depth;

        /// <summary>
        /// Filter mode of this render texture
        /// </summary>
        
        public TextureFilterMode filterMode;

        /// <summary>
        /// Anisotropic level when filtering is set to this mode
        /// </summary>
        
        public int anisoLevel;

        /// <summary>
        /// Wrap mode along the U coordinate
        /// </summary>
        
        public TextureWrapMode wrapU;

        /// <summary>
        /// Wrap mode along the V coordinate
        /// </summary>
        
        public TextureWrapMode wrapV;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(size);
            packer.Write(depth);
            packer.Write(filterMode);
            packer.Write(anisoLevel);
            packer.Write(wrapU);
            packer.Write(wrapV);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref size);
            packer.Read(ref depth);
            packer.Read(ref filterMode);
            packer.Read(ref anisoLevel);
            packer.Read(ref wrapU);
            packer.Read(ref wrapV);
        }
    }
}
