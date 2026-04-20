using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class ReflectionProbeRenderTask : IMemoryPackable
    {
        /// <summary>
        /// Index of the reflection probe renderable
        /// </summary>
        
        public int renderableIndex;

        /// <summary>
        /// Unique ID for the render task. The rendering process can take certain amount of time to complete and by the time
        /// it finishes the renderableIndex of the probe might have changed, so the ID is used to uniquely identify the task.
        /// It's potentially possible to also have multiple tasks for the same renderableIndex, so the ID also helps distinguish them.
        /// </summary>
        
        public int renderTaskId;

        /// <summary>
        /// Resolution of the cubemap side
        /// </summary>
        
        public int size;

        /// <summary>
        /// Whether to render the cubemap as HDR or LDR
        /// </summary>
        public bool hdr;

        /// <summary>
        /// Byte origins of each mip level of each face for the result data.
        /// </summary>
        
        public List<List<int>> mipOrigins;

        /// <summary>
        /// Buffer to fill the rendered data. This represents BitmapCube data
        /// </summary>
        
        public SharedMemoryBufferDescriptor<byte> resultData;

        /// <summary>
        /// List of transform IDs to exclude from the render
        /// </summary>
        
        public List<int> excludeTransformIds;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(renderableIndex);
            packer.Write(renderTaskId);
            packer.Write(size);
            packer.Write(hdr);

            packer.WriteNestedValueList(mipOrigins);

            packer.Write(resultData);

            packer.WriteValueList(excludeTransformIds);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref renderableIndex);
            packer.Read(ref renderTaskId);
            packer.Read(ref size);
            packer.Read(ref hdr);

            packer.ReadNestedValueList(ref mipOrigins);

            packer.Read(ref resultData);

            packer.ReadValueList(ref excludeTransformIds);
        }
    }
}
