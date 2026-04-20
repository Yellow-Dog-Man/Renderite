using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MeshUploadData : AssetCommand
    {
        
        public bool highPriority;

        
        public SharedMemoryBufferDescriptor<byte> buffer;

        
        public int vertexCount;
        
        public int boneWeightCount;
        
        public int boneCount;

        
        public IndexBufferFormat indexBufferFormat;

        
        public List<VertexAttributeDescriptor> vertexAttributes;
        
        public List<SubmeshBufferDescriptor> submeshes;
        
        public List<BlendshapeBufferDescriptor> blendshapeBuffers;

        
        public MeshUploadHint uploadHint;
        
        public RenderBoundingBox bounds;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(highPriority);

            packer.Write(buffer);

            packer.Write(vertexCount);
            packer.Write(boneWeightCount);
            packer.Write(boneCount);

            packer.Write(indexBufferFormat);

            packer.WriteValueList(vertexAttributes);
            packer.WriteValueList(submeshes);
            packer.WriteValueList(blendshapeBuffers);

            packer.Write(uploadHint);
            packer.Write(bounds);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref highPriority);

            packer.Read(ref buffer);

            packer.Read(ref vertexCount);
            packer.Read(ref boneWeightCount);
            packer.Read(ref boneCount);

            packer.Read(ref indexBufferFormat);

            packer.ReadValueList(ref vertexAttributes);
            packer.ReadValueList(ref submeshes);
            packer.ReadValueList(ref blendshapeBuffers);

            packer.Read(ref uploadHint);
            packer.Read(ref bounds);
        }
    }
}
