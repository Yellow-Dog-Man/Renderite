using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    public class TransformsUpdate : IMemoryPackable
    {
        /// <summary>
        /// This is the target number of transforms that should be present and allocated. This includes any newly allocated
        /// transforms in this frame, which will be assigned sequential ID's
        /// </summary>
        
        public int targetTransformCount;

        /// <summary>
        /// This is buffer describing any removed transforms. These need to be done in sequence, because the transform ID's
        /// will be remapped appropriately form the end to the new slots
        /// </summary>
        
        public SharedMemoryBufferDescriptor<int> removals;

        /// <summary>
        /// Buffer describing any parent changes. These are applied before any pose updates are done.
        /// </summary>
        
        public SharedMemoryBufferDescriptor<TransformParentUpdate> parentUpdates;

        /// <summary>
        /// Buffer describing new poses for the transforms in their local space.
        /// </summary>
        
        public SharedMemoryBufferDescriptor<TransformPoseUpdate> poseUpdates;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(targetTransformCount);
            packer.Write(removals);
            packer.Write(parentUpdates);
            packer.Write(poseUpdates);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref targetTransformCount);
            packer.Read(ref removals);
            packer.Read(ref parentUpdates);
            packer.Read(ref poseUpdates);
        }
    }
}
