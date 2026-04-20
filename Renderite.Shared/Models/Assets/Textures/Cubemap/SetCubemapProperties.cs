using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SetCubemapProperties : AssetCommand
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
            packer.Write(mipmapBias);
            packer.Write(applyImmediatelly);
            packer.Write(highPriority);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref filterMode);
            packer.Read(ref anisoLevel);
            packer.Read(ref mipmapBias);
            packer.Read(ref applyImmediatelly);
            packer.Read(ref highPriority);
        }
    }
}
