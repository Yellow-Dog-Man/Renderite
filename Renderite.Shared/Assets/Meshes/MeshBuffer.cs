using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Renderite.Shared
{
    [Flags]
    public enum BlendshapeDataFlags
    {
        NONE = 0,

        Positions = 1,
        Normals = 2,
        Tangets = 4,
    }

    public enum IndexBufferFormat
    {
        UInt16,
        UInt32,
    }

    public class MeshBuffer
    {
        public const long MAX_BUFFER_SIZE = 1024L * 1024L * 1024L * 2; // 2 GB

        public const int MAX_UV_CHANNEL_COUNT = 8;

        public int VertexAttributeCount => VertexAttributes?.Count ?? 0;
        public int SubmeshCount => Submeshes?.Count ?? 0;

        public List<VertexAttributeDescriptor> VertexAttributes { get; private set; }
        public List<SubmeshBufferDescriptor> Submeshes { get; private set; }

        public int VertexCount;
        public int BoneWeightCount;
        public int BoneCount;

        public IndexBufferFormat IndexBufferFormat;

        public int BlendshapeBufferCount => BlendshapeBuffers?.Count ?? 0;
        public List<BlendshapeBufferDescriptor> BlendshapeBuffers { get; private set; }

        public IBackingMemoryBuffer Data { get; set; }
        public Span<byte> RawBuffer => Data == null ? Span<byte>.Empty : Data.RawData;

        public int VertexStride { get; private set; }
        public int IndexBufferStart { get; private set; }
        public int IndexCount { get; private set; }
        public int IndexBufferLength { get; private set; }

        public int BoneCountsBufferStart { get; private set; }
        public int BoneCountsBufferLength { get; private set; }
        public int BoneWeightsBufferStart { get; private set; }
        public int BoneWeightsBufferLength { get; private set; }

        public int BindPosesBufferStart { get; private set; }
        public int BindPosesBufferLength { get; private set; }
        public int BlendshapeDataStart { get; private set; }

        public int TotalBufferLength { get; private set; }

        List<int> vertexAttributeOffsets = new List<int>();

        public MeshBuffer()
        {
            VertexAttributes = new List<VertexAttributeDescriptor>();
            Submeshes = new List<SubmeshBufferDescriptor>();
            BlendshapeBuffers = new List<BlendshapeBufferDescriptor>();
        }

        public MeshBuffer(MeshUploadData data)
        {
            VertexAttributes = data.vertexAttributes;
            Submeshes = data.submeshes;
            BlendshapeBuffers = data.blendshapeBuffers;

            VertexCount = data.vertexCount;
            BoneWeightCount = data.boneWeightCount;
            BoneCount = data.boneCount;

            IndexBufferFormat = data.indexBufferFormat;

            ComputeBufferLayout();
        }

        public void FillMeshBufferDefinition(MeshUploadData data)
        {
            data.vertexAttributes = VertexAttributes;
            data.submeshes = Submeshes;
            data.blendshapeBuffers = BlendshapeBuffers;

            data.vertexCount = VertexCount;
            data.boneWeightCount = BoneWeightCount;
            data.boneCount = BoneCount;

            data.indexBufferFormat = IndexBufferFormat;
        }

        public Span<byte> GetRawVertexBufferData()
        {
            // Vertex data ends, where index data starts
            return RawBuffer.Slice(0, IndexBufferStart);
        }

        public Span<byte> GetRawIndexBufferData()
        {
            return RawBuffer.Slice(IndexBufferStart, IndexBufferLength);
        }

        public Span<uint> GetIndexBufferUInt32()
        {
            if (IndexBufferFormat != IndexBufferFormat.UInt32)
                throw new InvalidOperationException("Index buffer format is not UInt32.");

            return MemoryMarshal.Cast<byte, uint>(RawBuffer.Slice(IndexBufferStart, IndexBufferLength));
        }

        public Span<ushort> GetIndexBufferUInt16()
        {
            if (IndexBufferFormat != IndexBufferFormat.UInt16)
                throw new InvalidOperationException("Index buffer format is not UInt16.");

            return MemoryMarshal.Cast<byte, ushort>(RawBuffer.Slice(IndexBufferStart, IndexBufferLength));
        }

        public Span<byte> GetBoneCountsBuffer()
        {
            return RawBuffer.Slice(BoneCountsBufferStart, BoneCountsBufferLength);
        }

        public Span<BoneWeight> GetBoneWeightsBuffer()
        {
            return MemoryMarshal.Cast<byte, BoneWeight>(RawBuffer.Slice(BoneWeightsBufferStart, BoneWeightsBufferLength));
        }

        public Span<T> GetBindPosesBuffer<T>()
            where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(RawBuffer.Slice(BindPosesBufferStart, BindPosesBufferLength));
        }

        public Span<T> GetBlendshapeBuffer<T>()
            where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(RawBuffer.Slice(BlendshapeDataStart));
        }

        public int VertexAttributeOffset(int attributeIndex) => vertexAttributeOffsets[attributeIndex];

        public void SetVertexAttribute<T>(int vertexIndex, int attributeIndex, T value)
            where T : unmanaged
        {
            SetVertexAttribute(RawBuffer, vertexIndex, attributeIndex, value);
        }

        public void SetVertexAttribute<T>(Span<byte> rawData, int vertexIndex, int attributeIndex, T value)
            where T : unmanaged
        {
            SetVertexAttributeAtOffset(rawData, vertexIndex, VertexAttributeOffset(attributeIndex), value);
        }

        public void SetVertexAttributeAtOffset<T>(Span<byte> rawData, int vertexIndex, int attributeOffset, T value)
            where T : unmanaged
        {
            rawData = rawData.Slice(vertexIndex * VertexStride + attributeOffset);
            MemoryMarshal.Write(rawData, ref value);
        }

        public void WriteVertexAttributeAtOffset(Span<byte> rawData, int vertexIndex, int attributeOffset, Span<byte> value)
        {
            rawData = rawData.Slice(vertexIndex * VertexStride + attributeOffset);
            value.CopyTo(rawData);
        }

        public unsafe void FillVertexAttributesRaw<T>(Span<byte> rawData, Span<T> values, int attributeOffset)
            where T : unmanaged
        {
            var stride = VertexStride;

            var rawValues = MemoryMarshal.Cast<T, byte>(values);
            var size = sizeof(T);

            // Originally this used spans, but this is significantly faster (more than twice), at least with
            // Unity's Mono from my testing.
            fixed (byte* sourcePtr = rawValues)
            fixed (byte* targetPtrBase = rawData)
            {
                var targetPtr = targetPtrBase + attributeOffset;

                for (int v = 0; v < rawValues.Length; v += size)
                {
                    Buffer.MemoryCopy(sourcePtr + v, targetPtr, size, size);

                    targetPtr += stride;
                }
            }
        }

        public unsafe void ComputeBufferLayout()
        {
            long offset = 0;

            VertexStride = ComputeVertexStride();

            // Indexes follow the vertex buffer
            offset += VertexStride * VertexCount;
            IndexBufferStart = (int)offset;

            IndexCount = ComputeIndexCount();

            IndexBufferLength = IndexCount;

            switch (IndexBufferFormat)
            {
                case IndexBufferFormat.UInt16:
                    IndexBufferLength *= sizeof(ushort);
                    break;

                case IndexBufferFormat.UInt32:
                    IndexBufferLength *= sizeof(uint);
                    break;

                default:
                    throw new NotSupportedException("Unsupported index buffer format: " + IndexBufferFormat);
            }

            offset += IndexBufferLength;

            BoneCountsBufferStart = (int)offset;
            BoneCountsBufferLength = VertexCount * sizeof(byte); // 255 bones per vertex max

            offset += BoneCountsBufferLength;

            BoneWeightsBufferStart = (int)offset;
            BoneWeightsBufferLength = BoneWeightCount * sizeof(BoneWeight);

            offset += BoneWeightsBufferLength;

            BindPosesBufferStart = (int)offset;
            BindPosesBufferLength = BoneCount * sizeof(RenderMatrix4x4);

            offset += BindPosesBufferLength;

            // Blendshape follow next
            BlendshapeDataStart = (int)offset;

            if (BlendshapeBuffers != null)
            {
                foreach (var blendshapeBuffer in BlendshapeBuffers)
                {
                    if (blendshapeBuffer.dataFlags.HasFlag(BlendshapeDataFlags.Positions))
                        offset += sizeof(float) * 3 * VertexCount;

                    if (blendshapeBuffer.dataFlags.HasFlag(BlendshapeDataFlags.Normals))
                        offset += sizeof(float) * 3 * VertexCount;

                    if (blendshapeBuffer.dataFlags.HasFlag(BlendshapeDataFlags.Tangets))
                        offset += sizeof(float) * 3 * VertexCount;
                }
            }

            if (offset >= MAX_BUFFER_SIZE)
                throw new Exception("Mesh buffer size exceeds maximum allowed size of 2 GB.");

            TotalBufferLength = (int)offset;
        }

        int ComputeVertexStride()
        {
            vertexAttributeOffsets.Clear();

            int offset = 0;

            if (VertexAttributes != null)
            {
                foreach (var attribute in VertexAttributes)
                {
                    vertexAttributeOffsets.Add(offset);
                    offset += attribute.Size;
                }
            }

            return offset;
        }

        int ComputeIndexCount()
        {
            int maxCount = 0;

            if (Submeshes != null)
            {
                foreach (var submesh in Submeshes)
                    maxCount = Math.Max(maxCount, submesh.EndIndex);
            }

            return maxCount;
        }

        public override string ToString() => $"MeshBuffer. Verts: {VertexCount}, IndicieCount: {IndexCount} (Format: {IndexBufferFormat}), SubmeshCount: {SubmeshCount}, " +
            $"Bones: {BoneCount}\nVertexLayout: {string.Join("\n", VertexAttributes?.Select(a => "- " + a) ?? Array.Empty<string>() )}\n" +
            $"SubmeshLayout: {string.Join("\n", Submeshes?.Select(s => "- " + s) ?? Array.Empty<string>())}";
    }
}
