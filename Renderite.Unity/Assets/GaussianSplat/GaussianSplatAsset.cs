using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renderite.Unity
{
    public class GaussianSplatAsset : Asset
    {
        const int SH_COEFFICIENT_COUNT = 16;

        static UnityEngine.Texture2D _emptyTexture;

        public int SplatCount { get; private set; }
        public Bounds Bounds { get; private set; }
        public bool IsLoaded => encodedPositions != null;

        GaussianVectorFormat positionsFormat;
        GaussianRotationFormat rotationsFormat;
        GaussianVectorFormat scalesFormat;
        GaussianColorFormat colorsFormat;
        GaussianSHFormat shFormat;

        ComputeBuffer chunks;
        int splatChunkCount;

        int shIndexesOffset;

        ComputeBuffer encodedPositions;
        ComputeBuffer encodedRotations;
        ComputeBuffer encodedScales;
        ComputeBuffer encodedColors;
        ComputeBuffer encodedSH;

        ComputeBuffer rawRotations;
        ComputeBuffer rawOpacities;
        ComputeBuffer rawColorData;

        Texture2D colorsTexture;

        public void AssignDataBuffers(CommandBuffer cmd, ComputeShader compute, int kernelID)
        {
            cmd.SetComputeIntParam(compute, "_positionFormat", (int)positionsFormat);
            cmd.SetComputeBufferParam(compute, kernelID, "_encodedPositions", encodedPositions);

            cmd.SetComputeIntParam(compute, "_rotationFormat", (int)rotationsFormat);
            cmd.SetComputeBufferParam(compute, kernelID, "_rawRotations", rawRotations);
            cmd.SetComputeBufferParam(compute, kernelID, "_encodedRotations", encodedRotations);

            cmd.SetComputeIntParam(compute, "_scaleFormat", (int)scalesFormat);
            cmd.SetComputeBufferParam(compute, kernelID, "_encodedScales", encodedScales);

            cmd.SetComputeIntParam(compute, "_colorFormat", (int)colorsFormat);
            cmd.SetComputeIntParam(compute, "_shFormat", (int)shFormat);
            cmd.SetComputeBufferParam(compute, kernelID, "_rawOpacities", rawOpacities);
            cmd.SetComputeBufferParam(compute, kernelID, "_rawColorData", rawColorData);
            cmd.SetComputeBufferParam(compute, kernelID, "_encodedColors", encodedColors);
            cmd.SetComputeBufferParam(compute, kernelID, "_encodedSH", encodedSH);

            cmd.SetComputeIntParam(compute, "_SplatChunkCount", splatChunkCount);
            cmd.SetComputeIntParam(compute, "_shIndexesOffset", shIndexesOffset);

            cmd.SetComputeBufferParam(compute, kernelID, "_chunks", chunks);

            cmd.SetComputeTextureParam(compute, kernelID, "_SplatColor", colorsTexture ?? _emptyTexture);
        }

        public void HandleUpload(GaussianSplatUpload upload)
        {
            AssetIntegrator.EnqueueProcessing(Upload, upload, false);
        }

        void Upload(object untypedUpload)
        {
            var upload = (GaussianSplatUpload)untypedUpload;
            var rawUpload = upload as GaussianSplatUploadRaw;
            var encodedUpload = upload as GaussianSplatUploadEncoded;

            if (_emptyTexture == null)
            {
                _emptyTexture = new UnityEngine.Texture2D(4, 4);
                _emptyTexture.Apply();
            }

            Bounds = upload.bounds.ToUnity();

            if (encodedUpload != null)
            {
                positionsFormat = encodedUpload.positionsFormat;
                rotationsFormat = encodedUpload.rotationsFormat;
                scalesFormat = encodedUpload.scalesFormat;
                colorsFormat = encodedUpload.colorsFormat;
                shFormat = encodedUpload.shFormat;

                splatChunkCount = encodedUpload.chunkCount;
                shIndexesOffset = encodedUpload.shIndexesOffset;

                colorsTexture = RenderingManager.Instance.Texture2Ds.GetAsset(encodedUpload.texture2DtextureAssetId)?.Texture;
            }
            else
            {
                positionsFormat = GaussianVectorFormat.Float32;
                rotationsFormat = (GaussianRotationFormat)(-1);
                scalesFormat = GaussianVectorFormat.Float32;
                colorsFormat = (GaussianColorFormat)(-1);
                shFormat = (GaussianSHFormat)(-1);

                splatChunkCount = 0;
                shIndexesOffset = 0;

                colorsTexture = null;
            }

            bool buffersChanged = false;

            if (upload.splatCount != SplatCount)
            {
                buffersChanged = true;

                // We must reallocate the compute buffers
                DisposeBuffers();

                if (upload.splatCount > 0)
                {
                    // These are always encoded
                    encodedPositions = new ComputeBuffer(MathHelper.AlignToNextMultiple(upload.positionsBuffer.length, 4) / 4, 4);
                    encodedScales = new ComputeBuffer(MathHelper.AlignToNextMultiple(upload.scalesBuffer.length, 4) / 4, 4);

                    if (rawUpload != null)
                    {
                        chunks = new ComputeBuffer(1, GaussianCloudHelper.CHUNK_SIZE);

                        encodedRotations = new ComputeBuffer(1, 4);
                        encodedColors = new ComputeBuffer(1, 4);
                        encodedSH = new ComputeBuffer(1, 4);

                        // We're doing a raw load
                        unsafe
                        {
                            rawRotations = new ComputeBuffer(upload.splatCount, sizeof(Quaternion));
                            rawOpacities = new ComputeBuffer(upload.splatCount, sizeof(float));
                            rawColorData = new ComputeBuffer(upload.splatCount, sizeof(float) * 3 * SH_COEFFICIENT_COUNT);
                        }
                    }
                    else
                    {
                        if (splatChunkCount > 0)
                            chunks = new ComputeBuffer(splatChunkCount, GaussianCloudHelper.CHUNK_SIZE);
                        else
                            chunks = new ComputeBuffer(1, GaussianCloudHelper.CHUNK_SIZE);

                        encodedRotations = new ComputeBuffer(MathHelper.AlignToNextMultiple(upload.rotationsBuffer.length, 4) / 4, 4);

                        encodedSH = new ComputeBuffer(MathHelper.AlignToNextMultiple(encodedUpload.shBuffer.length, 4) / 4, 4);

                        Debug.Log($"SH Format: {shFormat}, SplatCount: {upload.splatCount}, TotalBytes: {encodedUpload.shBuffer.length}, " +
                            $"Buffer Stride: {encodedSH.stride}, Buffer Count: {encodedSH.count}");

                        if (colorsFormat != GaussianColorFormat.BC7)
                            encodedColors = new ComputeBuffer(MathHelper.AlignToNextMultiple(upload.colorsBuffer.length, 4) / 4, 4);
                        else
                            encodedColors = new ComputeBuffer(1, 4);

                        unsafe
                        {
                            // Empty buffers
                            rawRotations = new ComputeBuffer(1, sizeof(Quaternion));
                            rawOpacities = new ComputeBuffer(1, sizeof(float));
                            rawColorData = new ComputeBuffer(1, sizeof(float) * 3 * SH_COEFFICIENT_COUNT);
                        }
                    }
                }
            }

            SplatCount = upload.splatCount;

            if (SplatCount > 0)
            {
                var sharedMemory = RenderingManager.Instance.SharedMemory;

                var positionsBuffer = sharedMemory.AccessData(upload.positionsBuffer);
                var rotationsBuffer = sharedMemory.AccessData(upload.rotationsBuffer);
                var scalesBuffer = sharedMemory.AccessData(upload.scalesBuffer);
                var colorsBuffer = sharedMemory.AccessData(upload.colorsBuffer);

                SetData(encodedPositions, MemoryMarshal.Cast<byte, uint>(positionsBuffer));
                SetData(encodedScales, MemoryMarshal.Cast<byte, uint>(scalesBuffer));

                if (rawUpload != null)
                {
                    var opacitiesBuffer = sharedMemory.AccessData(rawUpload.alphasBuffer);

                    SetData(rawRotations, MemoryMarshal.Cast<byte, uint>(rotationsBuffer));
                    SetData(rawOpacities, MemoryMarshal.Cast<byte, uint>(opacitiesBuffer));
                    SetData(rawColorData, MemoryMarshal.Cast<byte, uint>(colorsBuffer));
                }
                else
                {
                    var shBuffer = sharedMemory.AccessData(encodedUpload.shBuffer);

                    SetData(encodedRotations, MemoryMarshal.Cast<byte, uint>(rotationsBuffer));
                    SetData(encodedColors, MemoryMarshal.Cast<byte, uint>(colorsBuffer));
                    SetData(encodedSH, MemoryMarshal.Cast<byte, uint>(shBuffer));

                    if (splatChunkCount > 0)
                    {
                        var chunksBuffer = sharedMemory.AccessData(encodedUpload.chunksBuffer);
                        SetData(chunks, chunksBuffer);
                    }
                }
            }

            var result = new GaussianSplatResult();
            result.assetId = AssetId;

            result.instanceChanged = buffersChanged;

            RenderingManager.Instance.SendAssetUpdate(result);

            if(rawUpload != null)
                PackerMemoryPool.Instance.Return(rawUpload);

            if (encodedUpload != null)
                PackerMemoryPool.Instance.Return(encodedUpload);
        }

        static unsafe void SetData<T>(ComputeBuffer buffer, System.Span<T> data)
            where T : unmanaged
        {
            fixed (void* ptr = data)
            {
                var native = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, data.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                var safetyHandle = AtomicSafetyHandle.Create();
                AtomicSafetyHandle.SetAllowReadOrWriteAccess(safetyHandle, true);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref native, safetyHandle);
#endif

                buffer.SetData(native);
            }
        }

        void DisposeBuffers()
        {
            chunks?.Dispose();

            encodedPositions?.Dispose();
            encodedRotations?.Dispose();
            encodedScales?.Dispose();
            encodedColors?.Dispose();
            encodedSH?.Dispose();

            rawRotations?.Dispose();
            rawOpacities?.Dispose();
            rawColorData?.Dispose();

            chunks = null;

            encodedPositions = null;
            encodedRotations = null;
            encodedScales = null;
            encodedColors = null;
            encodedSH = null;

            rawRotations = null;
            rawOpacities = null;
            rawColorData = null;
        }

        public void Unload()
        {
            RenderingManager.Instance.GaussianSplats.RemoveAsset(this);

            AssetIntegrator.EnqueueProcessing(DisposeBuffers, true);
        }
    }
}
