using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SetCubemapData : AssetCommand
    {
        /// <summary>
        /// Buffer containing the full texture data. This needs to match the currently set texture format
        /// </summary>
        
        public SharedMemoryBufferDescriptor<byte> data;

        /// <summary>
        /// Which mip level should the data be uploaded from. This is often done for textures that are loaded
        /// progressively from smaller mip levels to the higher ones.
        /// </summary>
        
        public int startMipLevel;

        /// <summary>
        /// The aligned sizes of each mip level. These are shared across all the faces
        /// </summary>
        
        public List<RenderVector2i> mipMapSizes;

        /// <summary>
        /// The starting positions (in pixels) of each mip map level data for each face in the currently uploaded data.
        /// The top level list contains lists containing the starting positions of each of the faces.
        /// </summary>
        
        public List<List<int>> mipStarts;

        /// <summary>
        /// Indicates if the texture data is flipped on the Y axis
        /// </summary>
        
        public bool flipY;

        /// <summary>
        /// If it should use high priority integration
        /// </summary>
        
        public bool highPriority;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(data);

            packer.Write(startMipLevel);
            packer.WriteValueList(mipMapSizes);
            packer.WriteNestedValueList(mipStarts);

            packer.Write(flipY);
            packer.Write(highPriority);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref data);

            packer.Read(ref startMipLevel);
            packer.ReadValueList(ref mipMapSizes);
            packer.ReadNestedValueList(ref mipStarts);

            packer.Read(ref flipY);
            packer.Read(ref highPriority);
        }
    }
}
