using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SetTexture2DProperties : AssetCommand
    {
        /// <summary>
        /// The filter mode for this texture
        /// </summary>
        
        public TextureFilterMode filterMode;

        /// <summary>
        /// When set to Anisotropic filtering mode, this controls the aniso level
        /// </summary>
        
        public int anisoLevel;

        /// <summary>
        /// The wrapping behavior across horizontal axis
        /// </summary>
        
        public TextureWrapMode wrapU;

        /// <summary>
        /// The wrapping behavior across vertical axis
        /// </summary>
        
        public TextureWrapMode wrapV;

        /// <summary>
        /// Mipmap Bias
        /// </summary>
        
        public float mipmapBias;

        /// <summary>
        /// Indicates if these properties should be applied immediately. When false, the properties will be staged, but
        /// not applied until texture data is submitted.
        /// </summary>
        
        public bool applyImmediatelly;

        /// <summary>
        /// If it should use high priority integration
        /// </summary>
        
        public bool highPriority;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(filterMode);
            packer.Write(anisoLevel);
            packer.Write(wrapU);
            packer.Write(wrapV);
            packer.Write(mipmapBias);
            packer.Write(applyImmediatelly);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref filterMode);
            packer.Read(ref anisoLevel);
            packer.Read(ref wrapU);
            packer.Read(ref wrapV);
            packer.Read(ref mipmapBias);
            packer.Read(ref applyImmediatelly);
        }
    }
}
