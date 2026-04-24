using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Renderite.Unity
{
    struct SplatViewData
    {
        Vector4 pos;
        Vector2 axis1, axis2;
        uint color_a, color_b; // 4xFP16
    };

    public class GaussianSplatRenderer : MonoBehaviour
    {
        class CameraSortData : IDisposable
        {
            public BufferSorter sorter;
            public ComputeBuffer orderBuffer;
            public int lastFullSortFrame;

            public void Dispose()
            {
                sorter.Dispose();
                orderBuffer.Dispose();
            }
        }

        public GaussianSplatAsset Asset
        {
            get => asset;
            set
            {
                asset = value;

                if(lastSplatCount != SplatCount)
                {
                    // Reinitialize the buffers and re-register self if number of splats changed
                    InitBuffers();

                    lastSplatCount = SplatCount;
                }
            }
        }

        GaussianSplatAsset asset;

        public bool IsAssetReady => Asset != null && Asset.IsLoaded;
        public bool IsValidToRender => IsAssetReady && SplatCount == lastSplatCount;
        public int SplatCount => Asset?.SplatCount ?? 0;

        public float SplatScale = 1;
        [Range(0, 3)]
        public int SHOrder = 3;
        public float OpacityScale = 1;
        public bool SHOnly;

        public ComputeBuffer SplatViewData => splatViewData;
        public ComputeBuffer DistanceBuffer => distancesBuffer;

        int lastSplatCount;

        ComputeBuffer splatViewData;
        ComputeBuffer distancesBuffer;

        Dictionary<Camera, CameraSortData> sortData;

        void OnDestroy()
        {
            Cleanup();
        }

        unsafe void InitBuffers()
        {
            // Dispose any previous data
            Cleanup();

            if (!IsAssetReady)
                return;

            sortData = new Dictionary<Camera, CameraSortData>();

            splatViewData = new ComputeBuffer(SplatCount * 2, sizeof(SplatViewData));
            distancesBuffer = new ComputeBuffer(SplatCount, sizeof(uint));

            GaussianSplatRendererManager.RegisterRenderer(this);
        }

        public void AssignDataBuffers(CommandBuffer cmd, ComputeShader compute, int kernelID)
        {
            Asset.AssignDataBuffers(cmd, compute, kernelID);
        }

        public int GetLastFullSortFrame(Camera camera) => GetCameraSortData(camera).lastFullSortFrame;

        public ComputeBuffer GetOrderBuffer(Camera camera, out bool initSort)
        {
            var data = GetCameraSortData(camera);
            
            initSort = !data.sorter.IsSortRunning;

            return data.orderBuffer;
        }

        CameraSortData GetCameraSortData(Camera camera)
        {
            if (!sortData.TryGetValue(camera, out var data))
            {
                data = new CameraSortData();
                data.sorter = GaussianSplatRendererManager.AllocateSorter(SplatCount);
                data.orderBuffer = new ComputeBuffer(SplatCount, sizeof(uint));

                sortData.Add(camera, data);
            }

            return data;
        }

        public void CameraRemoved(Camera camera)
        {
            if (sortData.TryGetValue(camera, out var data))
            {
                data.Dispose();
                sortData.Remove(camera);
            }
        }

        public void RunSortChunk(CommandBuffer cmd, Camera camera, ref long? availableSortOps)
        {
            var sortData = GetCameraSortData(camera);

            // This will schedule as many sorting operations as possible within the available sort ops budget
            var done = sortData.sorter.RunSortChunk(cmd, distancesBuffer, sortData.orderBuffer, ref availableSortOps, reverse: true);

            // If we just sorted, update the last full sort frame, so this one gets less priority over other renderers
            if (done)
                sortData.lastFullSortFrame = Time.frameCount;
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

        void Cleanup()
        {
            GaussianSplatRendererManager.UnregisterRenderer(this);

            lastSplatCount = 0;

            splatViewData?.Dispose();
            distancesBuffer?.Dispose();

            if (sortData != null)
            {
                foreach (var group in sortData)
                    group.Value.Dispose();
            }

            splatViewData = null;
            distancesBuffer = null;
            sortData = null;
        }
    }

}
