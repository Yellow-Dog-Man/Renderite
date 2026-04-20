using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MaterialsUpdateBatch : RendererCommand
    {
        /// <summary>
        /// ID uniquely identifying this update batch. This will allow the system to match up the update response.
        /// And send events to appropriate systems.
        /// </summary>
        
        public int updateBatchId = -1;

        /// <summary>
        /// This is a set of buffers describing all the material & property block updates that need to happen as part of this batch.
        /// </summary>
        
        public List<SharedMemoryBufferDescriptor<MaterialPropertyUpdate>> materialUpdates = new List<SharedMemoryBufferDescriptor<MaterialPropertyUpdate>>();

        /// <summary>
        /// This indicates how many materials there are to update.
        /// between regular materials and property blocks for better code reuse and buffer reuse, so it needs to be indicated
        /// at which point do the updates switch over to materials
        /// </summary>
        
        public int materialUpdateCount;

        /// <summary>
        /// Buffer of int values that will be assigned to materials.
        /// </summary>
        
        public List<SharedMemoryBufferDescriptor<int>> intBuffers = new List<SharedMemoryBufferDescriptor<int>>();

        /// <summary>
        /// Buffer of float values that will be assigned to materials.
        /// </summary>
        
        public List<SharedMemoryBufferDescriptor<float>> floatBuffers = new List<SharedMemoryBufferDescriptor<float>>();

        /// <summary>
        /// Buffer of float4 values to be assigned to materials
        /// </summary>
        
        public List<SharedMemoryBufferDescriptor<RenderVector4>> float4Buffers = new List<SharedMemoryBufferDescriptor<RenderVector4>>();

        /// <summary>
        /// Buffer of float4x4 values to be assigned to materials
        /// </summary>
        
        public List<SharedMemoryBufferDescriptor<RenderMatrix4x4>> matrixBuffers = new List<SharedMemoryBufferDescriptor<RenderMatrix4x4>>();

        /// <summary>
        /// This is buffer that the renderer will write bits to, to indicate which materials have updated instance
        /// </summary>
        
        public SharedMemoryBufferDescriptor<uint> instanceChangedBuffer;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(updateBatchId);

            packer.WriteValueList(materialUpdates);
            packer.Write(materialUpdateCount);
            packer.WriteValueList(intBuffers);
            packer.WriteValueList(floatBuffers);
            packer.WriteValueList(float4Buffers);
            packer.WriteValueList(matrixBuffers);
            packer.Write(instanceChangedBuffer);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref updateBatchId);

            packer.ReadValueList(ref materialUpdates);
            packer.Read(ref materialUpdateCount);
            packer.ReadValueList(ref intBuffers);
            packer.ReadValueList(ref floatBuffers);
            packer.ReadValueList(ref float4Buffers);
            packer.ReadValueList(ref matrixBuffers);
            packer.Read(ref instanceChangedBuffer);
        }

        public override string ToString() =>
            $"BatchId: {updateBatchId}\n" +
            $"MaterialUpdateCount: {materialUpdateCount}\n" +

            $"MaterialUpdates:\n\t{string.Join("\n\t", materialUpdates)}\n" +
            $"IntBuffers:\n\t{string.Join("\n\t", intBuffers)}\n" +
            $"FloatBuffers:\n\t{string.Join("\n\t", floatBuffers)}\n" +
            $"Float4Buffers:\n\t{string.Join("\n\t", float4Buffers)}\n" +
            $"MatrixBuffers:\n\t{string.Join("\n\t", matrixBuffers)}"
            ;
    }
}
