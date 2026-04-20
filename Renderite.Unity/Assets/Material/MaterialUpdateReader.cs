using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public ref struct MaterialUpdateReader
    {
        MaterialsUpdateBatch batch;

        int instanceChangedIndex;
        BitSpan instanceChangedBuffer;

        Span<MaterialPropertyUpdate> updateBuffer;

        Span<int> intBuffer;
        Span<float> floatBuffer;
        Span<Vector4> vectorBuffer;
        Span<Matrix4x4> matrixBuffer;

        int updateBufferIndex;
        int intBufferIndex;
        int floatBufferIndex;
        int vectorBufferIndex;
        int matrixBufferIndex;

        int updateIndex;

        int intIndex;
        int floatIndex;
        int vectorIndex;
        int matrixIndex;

        public MaterialUpdateReader(MaterialsUpdateBatch batch, BitSpan instanceChangedBuffer) : this()
        {
            // This is all that's needed, it will initialize the rest on the first read of each type
            this.batch = batch;
            this.instanceChangedBuffer = instanceChangedBuffer;
        }

        public bool HasNextUpdate
        {
            get
            {
                // If we reached the end of the buffer and reached the last update buffer, then we no longer have any
                // more updates.
                if (updateIndex == updateBuffer.Length)
                {
                    // We reached the last buffer too, so there's no more updates after this
                    if (updateBufferIndex == batch.materialUpdates.Count)
                        return false;
                    else
                        return true; // there's another buffer after this, so more updates
                }

                // Otherwise just check if the next update marks the end of the updates
                return updateBuffer[updateIndex].updateType != MaterialPropertyUpdateType.UpdateBatchEnd;
            }
        }

        public void WriteInstanceChanged(bool instanceChanged)
        {
            instanceChangedBuffer[instanceChangedIndex++] = instanceChanged;
        }

        public MaterialPropertyUpdate ReadUpdate() => ReadValue(ref updateBufferIndex, ref updateIndex, ref updateBuffer, batch.materialUpdates);

        public int PeekInt() => ReadValue(ref intBufferIndex, ref intIndex, ref intBuffer, batch.intBuffers, false);
        public int ReadInt() => ReadValue(ref intBufferIndex, ref intIndex, ref intBuffer, batch.intBuffers);
        public float ReadFloat() => ReadValue(ref floatBufferIndex, ref floatIndex, ref floatBuffer, batch.floatBuffers);
        public Vector4 ReadVector() => ReadValue(ref vectorBufferIndex, ref vectorIndex, ref vectorBuffer, batch.float4Buffers);
        public Matrix4x4 ReadMatrix() => ReadValue(ref matrixBufferIndex, ref matrixIndex, ref matrixBuffer, batch.matrixBuffers);

        // These are marked unsafe, because Span<T> can potentially point to locally allocated memory, like one on stack, which cannot
        // be returned from the method. However in this case we're always accessing shared memory, which is safe to return, because
        // it stays allocated outside of the scope of the method and will not move.
        public unsafe Span<float> AccessFloatArray() => AccessArray(ref floatBufferIndex, ref floatIndex, ref floatBuffer, batch.floatBuffers);
        public unsafe Span<Vector4> AccessVectorArray() => AccessArray(ref vectorBufferIndex, ref vectorIndex, ref vectorBuffer, batch.float4Buffers);

        unsafe T ReadValue<T, S>(ref int bufferIndex, ref int valueIndex, ref Span<T> buffer, List<SharedMemoryBufferDescriptor<S>> list, bool advance = true)
            where T : unmanaged
            where S : unmanaged
        {
            if (valueIndex == buffer.Length)
                buffer = FetchNextBuffer<T, S>(ref bufferIndex, ref valueIndex, list);

            var value = buffer[valueIndex];

            if (advance)
                valueIndex++;

            return value;
        }

        unsafe Span<T> AccessArray<T, S>(ref int bufferIndex, ref int valueIndex, ref Span<T> buffer, List<SharedMemoryBufferDescriptor<S>> list)
            where T : unmanaged
            where S : unmanaged
        {
            // Read the length first
            var length = ReadInt();

            // Check if the current buffer has enough space to accommodate
            if (length + valueIndex > buffer.Length)
                buffer = FetchNextBuffer<T, S>(ref bufferIndex, ref valueIndex, list);

            var slice = buffer.Slice(valueIndex, length);
            valueIndex += length;

            return slice;
        }

        Span<T> FetchNextBuffer<T,S>(ref int bufferIndex, ref int valueIndex, List<SharedMemoryBufferDescriptor<S>> list)
            where T : unmanaged
            where S : unmanaged
        {
            // Since the struct cannot be initialized to -1, we consider the index to be the "next buffer" to fetch
            // So we access it first and only then increment

            if (bufferIndex >= list.Count)
                throw new InvalidOperationException($"Next buffer of type {typeof(T)} does not exist!");

            var buffer = list[bufferIndex++];

            // Reset back to start
            valueIndex = 0;

            return RenderingManager.Instance.SharedMemory.AccessData<T>(buffer.As<T>());
        }

        public override string ToString() =>
            $"InstanceChangedIndex: {instanceChangedIndex}\n" +
            $"UpdateBufferIndex: {updateBufferIndex}\n" +
            $"IntBufferIndex: {intBufferIndex}\n" +
            $"FloatBufferIndex: {floatBufferIndex}\n" +
            $"VectorBufferIndex: {vectorBufferIndex}\n" +
            $"MatrixBufferIndex: {matrixBufferIndex}\n" +
            $"\n" +
            $"UpdateIndex: {updateIndex}\n" +
            $"IntIndex: {intIndex}\n" +
            $"FloatIndex: {floatIndex}\n" +
            $"VectorIndex: {vectorIndex}\n" +
            $"MatrixIndex: {matrixIndex}\n" +
            $"\n" +
            $"Current UpdateBuffer.Length: {updateBuffer.Length}\n" +
            $"Current IntBuffer.Length {intBuffer.Length}\n" +
            $"Current FloatBuffer.Length {floatBuffer.Length}\n" +
            $"Current VectorBuffer.Length {vectorBuffer.Length}\n" +
            $"Current MatrixBuffer.Length {matrixBuffer.Length}\n" +
            $"\n" +
            $"UpdateBatch:\n{batch}";
    }
}
