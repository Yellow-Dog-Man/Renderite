using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class CameraRenderTask : IMemoryPackable
    {
        /// <summary>
        /// The ID of the render space this render is for
        /// </summary>
        
        public int renderSpaceId;

        /// <summary>
        /// Position of the render camera view
        /// </summary>
        
        public RenderVector3 position;

        /// <summary>
        /// Rotatino of the render camera
        /// </summary>
        
        public RenderQuaternion rotation;

        /// <summary>
        /// Parameters for the camera render
        /// </summary>
        
        public CameraRenderParameters parameters;

        /// <summary>
        /// Buffer to fill the rendered data
        /// </summary>
        
        public SharedMemoryBufferDescriptor<byte> resultData;

        /// <summary>
        /// List of transform ID's that should be the only ones rendered
        /// </summary>
        
        public List<int> onlyRenderList;

        /// <summary>
        /// List of transform ID's that should be excluded from the render
        /// </summary>
        
        public List<int> excludeRenderList;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(renderSpaceId);
            packer.Write(position);
            packer.Write(rotation);
            packer.WriteObject(parameters);
            packer.Write(resultData);
            packer.WriteValueList(onlyRenderList);
            packer.WriteValueList(excludeRenderList);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref renderSpaceId);
            packer.Read(ref position);
            packer.Read(ref rotation);
            packer.ReadObject(ref parameters);
            packer.Read(ref resultData);
            packer.ReadValueList(ref onlyRenderList);
            packer.ReadValueList(ref excludeRenderList);
        }
    }
}
