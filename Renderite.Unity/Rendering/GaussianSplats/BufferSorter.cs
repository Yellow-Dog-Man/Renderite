using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;

namespace Renderite.Unity
{
    public class BufferSorter : System.IDisposable
    {
        private class Kernels
        {
            public int Sort { get; private set; }
            public int PadBuffer { get; private set; }
            public int OverwriteAndTruncate { get; private set; }
            public int CopyBuffer { get; private set; }

            public Kernels(ComputeShader cs)
            {
                Sort = cs.FindKernel("BitonicSort");
                PadBuffer = cs.FindKernel("PadBuffer");
                OverwriteAndTruncate = cs.FindKernel("OverwriteAndTruncate");
                CopyBuffer = cs.FindKernel("CopyBuffer");
            }
        }

        private static class Properties
        {
            public static int Block { get; private set; } = Shader.PropertyToID("_Block");
            public static int Dimension { get; private set; } = Shader.PropertyToID("_Dimension");
            public static int Count { get; private set; } = Shader.PropertyToID("_Count");
            public static int Reverse { get; private set; } = Shader.PropertyToID("_Reverse");
            public static int NextPowerOfTwo { get; private set; } = Shader.PropertyToID("_NextPowerOfTwo");

            public static int KeysBuffer { get; private set; } = Shader.PropertyToID("_Keys");
            public static int ValuesBuffer { get; private set; } = Shader.PropertyToID("_Values");

            public static int ExternalValuesBuffer { get; private set; } = Shader.PropertyToID("_ExternalValues");
            public static int ExternalKeysBuffer { get; private set; } = Shader.PropertyToID("_ExternalKeys");

            public static int FromBuffer { get; private set; } = Shader.PropertyToID("_From");
            public static int ToBuffer { get; private set; } = Shader.PropertyToID("_To");
        }

        public int OriginalCount { get; private set; }
        public int PaddedCount { get; private set; }

        public bool IsSortRunning => _currentDim >= 0;

        int _currentDim = -1;
        int _currentBlock = -1;

        private readonly Kernels m_kernels;
        private readonly ComputeShader m_computeShader;

        private ComputeBuffer m_keysBuffer;
        private ComputeBuffer m_valuesBuffer;
        private ComputeBuffer m_paddingBuffer;

        private readonly int[] m_paddingInput = new int[] { 0, 0 };

        public BufferSorter(ComputeShader computeShader, int length)
        {
            m_computeShader = computeShader;
            m_kernels = new Kernels(m_computeShader);

            OriginalCount = length;
            PaddedCount = Mathf.NextPowerOfTwo(OriginalCount);

            m_paddingBuffer = new ComputeBuffer(2, sizeof(int));
            m_keysBuffer = new ComputeBuffer(PaddedCount, sizeof(uint));
            m_valuesBuffer = new ComputeBuffer(PaddedCount, sizeof(int));

            m_valuesBuffer.SetCounterValue(0);
        }

        ~BufferSorter() => Dispose();

        public void Dispose()
        {
            m_keysBuffer?.Dispose();
            m_valuesBuffer?.Dispose();
            m_paddingBuffer?.Dispose();
        }

        public bool RunSortChunk(CommandBuffer cmd, ComputeBuffer values, ComputeBuffer keys, ref long? availableSortOps, bool reverse = false)
        {
            if(!IsSortRunning)
            {
                // We're at the start of a new sort. Init everything!
                InitSort(cmd, values, keys, reverse);

                // Consider this taking away from the available sort ops
                if(availableSortOps != null)
                    availableSortOps -= PaddedCount;
            }

            // We ran out of ops! Just bail out now
            if (availableSortOps != null && availableSortOps <= 0)
                return false;

            // This is always set for all the shaders
            cmd.SetComputeIntParam(m_computeShader, Properties.Count, PaddedCount);

            // initialize the keys buffer for use with the sort algorithm proper
            Util.CalculateWorkSize(PaddedCount, out var x, out var y, out var z);

            // Schedule as many sort operations as there are available sort ops
            while (availableSortOps == null || availableSortOps > 0)
            {
                // Take away sort ops
                if(availableSortOps != null)
                    availableSortOps -= PaddedCount;

                if (PerformSortStep(cmd, x, y, z))
                {
                    // We are complete! Copy the result to the output buffers.
                    CopyResults(cmd, keys);

                    // Reset the sort for another cycle
                    _currentDim = -1;
                    _currentBlock = -1;

                    // Inform that we have finished the sort in this cycle!
                    return true;
                }
            }

            // The loop finished without returning, which means we ran out of sort ops. That means we're not done
            return false;
        }

        bool PerformSortStep(CommandBuffer cmd, int x, int y, int z)
        {
            cmd.SetComputeIntParam(m_computeShader, Properties.Dimension, _currentDim);
            cmd.SetComputeIntParam(m_computeShader, Properties.Block, _currentBlock);
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.Sort, Properties.KeysBuffer, m_keysBuffer);
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.Sort, Properties.ValuesBuffer, m_valuesBuffer);

            cmd.DispatchCompute(m_computeShader, m_kernels.Sort, x, y, z);

            // Advance the block
            _currentBlock >>= 1;

            // Check if we still got more blocks to process
            if (_currentBlock > 0)
                return false;

            // We finished the current set of blocks. Advance the dimension
            _currentDim <<= 1;

            // We are done now!
            if (_currentDim > PaddedCount)
                return true;

            // Initialize next block value
            InitBlock();

            // There's more to sort!
            return false;
        }

        void InitSort(CommandBuffer cmd, ComputeBuffer values, ComputeBuffer keys, bool reverse = false)
        {
            Debug.Assert(values.count == keys.count, "Value and key buffers must be of the same size.");

            // initializing local buffers
            cmd.SetComputeIntParam(m_computeShader, Properties.Count, OriginalCount);
            cmd.SetComputeIntParam(m_computeShader, Properties.NextPowerOfTwo, PaddedCount);

            cmd.SetComputeIntParam(m_computeShader, Properties.Reverse, reverse ? 1 : 0);

            // setting up the second kernel, the padding kernel. because the sort only works on power of two sized buffers,
            // this will pad the buffer with duplicates of the greatest (or least, if reverse sort) integer to be truncated later
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.PadBuffer, Properties.ExternalValuesBuffer, values);
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.PadBuffer, Properties.ValuesBuffer, m_valuesBuffer);
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.PadBuffer, Properties.KeysBuffer, m_keysBuffer);

            cmd.DispatchCompute(m_computeShader, m_kernels.PadBuffer, Mathf.CeilToInt((float)PaddedCount / Util.GROUP_SIZE), 1, 1);

            // Init the sort parameters
            _currentDim = 2;
            InitBlock();
        }

        void CopyResults(CommandBuffer cmd, ComputeBuffer keys)
        {
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.OverwriteAndTruncate, Properties.KeysBuffer, m_keysBuffer);
            cmd.SetComputeBufferParam(m_computeShader, m_kernels.OverwriteAndTruncate, Properties.ExternalKeysBuffer, keys);

            cmd.DispatchCompute(m_computeShader, m_kernels.OverwriteAndTruncate, Mathf.CeilToInt((float)OriginalCount / Util.GROUP_SIZE), 1, 1);
        }

        void InitBlock() => _currentBlock = _currentDim >> 1;

        private static class Util
        {
            public const int GROUP_SIZE = 256;
            public const int MAX_DIM_GROUPS = 1024;
            public const int MAX_DIM_THREADS = (GROUP_SIZE * MAX_DIM_GROUPS);

            public static void CalculateWorkSize(int length, out int x, out int y, out int z)
            {
                if (length <= MAX_DIM_THREADS)
                {
                    x = (length - 1) / GROUP_SIZE + 1;
                    y = z = 1;
                }
                else
                {
                    x = MAX_DIM_GROUPS;
                    y = (length - 1) / MAX_DIM_THREADS + 1;
                    z = 1;
                }
            }
        }
    }
}
