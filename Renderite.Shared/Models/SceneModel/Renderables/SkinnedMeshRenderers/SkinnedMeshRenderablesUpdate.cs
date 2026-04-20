using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class SkinnedMeshRenderablesUpdate : MeshRenderablesUpdate
    {
        /// <summary>
        /// Updates to bounding boxes of skinned mesh renderers
        /// </summary>
        
        public SharedMemoryBufferDescriptor<SkinnedMeshBoundsUpdate> boundsUpdates;

        /// <summary>
        /// Updates that need to be computed by the renderer and assigned back
        /// </summary>
        
        public SharedMemoryBufferDescriptor<SkinnedMeshRealtimeBoundsUpdate> realtimeBoundsUpdates;

        /// <summary>
        /// Descriptors of bone assignments that need to happen in this update
        /// </summary>
        
        public SharedMemoryBufferDescriptor<BoneAssignment> boneAssignments;

        /// <summary>
        /// This buffer contains the transform ID's for the bone assignments buffer
        /// </summary>
        
        public SharedMemoryBufferDescriptor<int> boneTransformIndexes;

        /// <summary>
        /// Batches of blendshape updates. The actual updates are in separate buffer
        /// </summary>
        
        public SharedMemoryBufferDescriptor<BlendshapeUpdateBatch> blendshapeUpdateBatches;

        /// <summary>
        /// Individual blendshape updates
        /// </summary>
        
        public SharedMemoryBufferDescriptor<BlendshapeUpdate> blendshapeUpdates;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(boundsUpdates);
            packer.Write(realtimeBoundsUpdates);
            packer.Write(boneAssignments);
            packer.Write(boneTransformIndexes);
            packer.Write(blendshapeUpdateBatches);
            packer.Write(blendshapeUpdates);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref boundsUpdates);
            packer.Read(ref realtimeBoundsUpdates);
            packer.Read(ref boneAssignments);
            packer.Read(ref boneTransformIndexes);
            packer.Read(ref blendshapeUpdateBatches);
            packer.Read(ref blendshapeUpdates);
        }

    }
}
