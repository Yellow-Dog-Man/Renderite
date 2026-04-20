using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using System.Linq;
using Renderite.Shared;

namespace Renderite.Unity
{
    class DestroyProxy : MonoBehaviour
    {
        public System.Action DestroyCallback;

        void OnDestroy()
        {
            DestroyCallback?.Invoke();
        }
    }

    public static class GaussianSplatRendererManager
    {
        struct SplatRendererDist : IComparable<SplatRendererDist>
        {
            public GaussianSplatRenderer renderer;
            public float distance;

            public int CompareTo(SplatRendererDist other) => distance.CompareTo(other.distance);
        }

        struct SplatRendererSort : IComparable<SplatRendererSort>
        {
            public GaussianSplatRenderer renderer;
            public int lastFullSortFrame;

            public int CompareTo(SplatRendererSort other) => lastFullSortFrame.CompareTo(other.lastFullSortFrame);
        }

        class CameraData
        {
            public CommandBuffer Command;
        }

        public const int GROUP_SIZE = 1024;

        public static readonly int GaussianSplatRT = Shader.PropertyToID("_GaussianSplatRT");
        static int ComputeThreadGroups(int count) => MathHelper.CeilToInt(count / (double)GROUP_SIZE);

        static bool _dataInitialized;

        static HashSet<GaussianSplatRenderer> _renderers = new HashSet<GaussianSplatRenderer>();
        static Dictionary<Camera, CameraData> _registeredCameras = new Dictionary<Camera, CameraData>();

        static Material _renderMaterial;
        static Material _compositeMaterial;
        static MaterialPropertyBlock _renderPropertyBlock;

        static ComputeShader _renderCompute;
        static ComputeShader _sortCompute;

        static int _calcDistances;
        static int _calcViewDataMono;
        static int _calcViewDataStereo;

        static float sortMegaOperationsPerCamera = 1f;

        public static void RegisterRenderer(GaussianSplatRenderer renderer)
        {
            if (!_renderers.Add(renderer))
                throw new InvalidOperationException("Renderer already registered");

            // This is the first renderer, initialize everything
            if (_renderers.Count == 1)
                Initialize();
        }

        public static BufferSorter AllocateSorter(int splatCount)
        {
            return new BufferSorter(_sortCompute, splatCount);
        }

        public static void UnregisterRenderer(GaussianSplatRenderer renderer)
        {
            if (!_renderers.Remove(renderer))
                return;

            if (_renderers.Count == 0)
                Deinitialize();
        }

        static void Initialize()
        {
            if (!_dataInitialized)
                InitializeData();

            Camera.onPreCull += OnPreCullCamera;
        }

        static void Deinitialize()
        {
            Camera.onPreCull -= OnPreCullCamera;

            // Remove all the cameras
            var cameras = _registeredCameras.Keys.ToList();

            foreach (var c in cameras)
                CameraRemoved(c);
        }

        static void InitializeData()
        {
            _renderMaterial = Resources.Load<Material>("GaussianSplatting/Render");
            _compositeMaterial = Resources.Load<Material>("GaussianSplatting/Composite");
            _renderCompute = Resources.Load<ComputeShader>("GaussianSplatting/RenderCompute");
            _sortCompute = Resources.Load<ComputeShader>("GaussianSplatting/SortCompute");

            _renderPropertyBlock = new MaterialPropertyBlock();

            _calcDistances = _renderCompute.FindKernel("CSCalcDistances");
            _calcViewDataMono = _renderCompute.FindKernel("CSCalcViewDataMono");
            _calcViewDataStereo = _renderCompute.FindKernel("CSCalcViewDataStereo");

            _dataInitialized = true;
        }

        public static void ApplyConfig(GaussianSplatConfig config)
        {
            sortMegaOperationsPerCamera = config.sortingMegaOperationsPerCamera;
        }

        static void CameraRemoved(Camera camera)
        {
            if(_registeredCameras.TryGetValue(camera, out var data))
            {
                camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, data.Command);

                data.Command.Dispose();

                _registeredCameras.Remove(camera);

                foreach (var splat in _renderers)
                    splat.CameraRemoved(camera);
            }
        }

        static CameraData GetCameraData(Camera cam)
        {
            if (_registeredCameras.TryGetValue(cam, out var data))
                return data;

            var destroyProxy = cam.gameObject.AddComponent<DestroyProxy>();
            destroyProxy.DestroyCallback += () => CameraRemoved(cam);

            // The camera isn't registered yet, create data!
            data = new CameraData();
            data.Command = new CommandBuffer() { name = $"GaussianSplats - {cam.name}" };

            cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, data.Command);

            _registeredCameras.Add(cam, data);

            return data;
        }

        static List<SplatRendererDist> toRender = new List<SplatRendererDist>();
        static List<SplatRendererSort> toSort = new List<SplatRendererSort>();

        static void OnPreCullCamera(Camera cam)
        {
            if (cam.cameraType == CameraType.Preview)
                return;

            var data = GetCameraData(cam);
            var cmd = data.Command;

            cmd.Clear();

            toRender.Clear();

            // Add all the splats
            CollectAndSortRenderersForCamera(cam, data, toRender);

            if (toRender.Count > 0)
            {
                var width = cam.pixelWidth;
                var height = cam.pixelHeight;

                if (cam.stereoEnabled)
                    width *= 2;

                cmd.GetTemporaryRT(GaussianSplatRT, width, height, 0, FilterMode.Point, GraphicsFormat.R16G16B16A16_SFloat);
                cmd.SetRenderTarget(GaussianSplatRT, BuiltinRenderTextureType.CurrentActive);
                cmd.ClearRenderTarget(false, true, new Color(0, 0, 0, 0), 0);

                toSort.Clear();

                foreach (var renderer in toRender)
                    toSort.Add(new SplatRendererSort()
                    {
                        renderer = renderer.renderer,
                        lastFullSortFrame = renderer.renderer.GetLastFullSortFrame(cam)
                    });

                // Sort them based on when they were last sorted (sortception!!!)
                // This way ones that were sorted longest ago (lowest number) will be first
                // and thus will get to be sorted now
                toSort.Sort();

                long? availableSortOps = null;

                if(cam.tag != RenderHelper.CAPTURE_CAMERA_TAG)
                    availableSortOps = MathHelper.RoundToLong(1024 * 1024 * sortMegaOperationsPerCamera);

                // Enqueue all the sorting operations
                foreach (var renderer in toSort)
                    EnqueueSort(cam, data, renderer.renderer, ref availableSortOps);

                toSort.Clear();

                foreach (var renderer in toRender)
                    EnqueueRender(cam, data, renderer.renderer);

                // compose
                cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                cmd.DrawProcedural(Matrix4x4.identity, _compositeMaterial, 0, MeshTopology.Triangles, 3, 1);
                cmd.ReleaseTemporaryRT(GaussianSplatRT);
            }

            toRender.Clear();
        }

        static void CollectAndSortRenderersForCamera(Camera camera, CameraData data, List<SplatRendererDist> toRender)
        {
            foreach (var renderer in _renderers)
            {
                if (!renderer.IsValidToRender)
                    continue;

                // It's disabled, don't render it
                if (!renderer.enabled)
                    continue;

                // Check if the renderer is actually active
                if (!renderer.gameObject.activeInHierarchy)
                    continue;

                // Check the mask
                var layerMask = 1 << renderer.gameObject.layer;

                if ((camera.cullingMask & layerMask) == 0)
                    continue;

                var entry = new SplatRendererDist();
                entry.renderer = renderer;

                var localPos = camera.transform.InverseTransformPoint(renderer.transform.position);
                entry.distance = localPos.z;

                toRender.Add(entry);
            }

            toRender.Sort();
        }

        static void SetStereoCameraParams(CommandBuffer cmd, Camera camera, Camera.StereoscopicEye eye, Matrix4x4 matrixM)
        {
            var matrixV = camera.GetStereoViewMatrix(eye);
            var matrixP = GL.GetGPUProjectionMatrix(camera.GetStereoProjectionMatrix(eye), true);

            var matrixVP = matrixP * matrixV;
            var matrixMV = matrixV * matrixM;

            var invMatrixV = matrixV.inverse;
            var pos = new Vector3(invMatrixV[0, 3], invMatrixV[1, 3], invMatrixV[2, 3]);

            var suffix = eye == Camera.StereoscopicEye.Left ? "_L" : "_R";

            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixVP" + suffix, matrixVP);
            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixMV" + suffix, matrixMV);
            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixP" + suffix, matrixP);
            cmd.SetComputeVectorParam(_renderCompute, "_VecWorldSpaceCameraPos" + suffix, pos);
        }

        static void EnqueueSort(Camera camera, CameraData data, GaussianSplatRenderer renderer, ref long? availableSortOps)
        {
            var cmd = data.Command;

            var orderBuffer = renderer.GetOrderBuffer(camera, out var initSort);

            if (initSort)
            {
                // We need to calculate the distances, set the camera params for this
                SetCameraParams(camera, data, renderer, out _);

                renderer.AssignDataBuffers(cmd, _renderCompute, _calcDistances);

                cmd.SetComputeBufferParam(_renderCompute, _calcDistances, "_SplatSortDistances", renderer.DistanceBuffer);
                cmd.DispatchCompute(_renderCompute, _calcDistances, ComputeThreadGroups(renderer.SplatCount), 1, 1);
            }

            renderer.RunSortChunk(cmd, camera, ref availableSortOps);
        }

        static void SetCameraParams(Camera camera, CameraData data, GaussianSplatRenderer renderer, out Matrix4x4 matrixM)
        {
            var cmd = data.Command;

            cmd.SetComputeIntParam(_renderCompute, "_SplatCount", renderer.SplatCount);

            cmd.SetComputeIntParam(_renderCompute, "_SHOrder", renderer.SHOrder);
            cmd.SetComputeFloatParam(_renderCompute, "_SplatScale", renderer.SplatScale);
            cmd.SetComputeFloatParam(_renderCompute, "_SplatOpacityScale", renderer.OpacityScale);
            cmd.SetComputeIntParam(_renderCompute, "_SHOnly", renderer.SHOnly ? 1 : 0);

            var matrixV = camera.worldToCameraMatrix;
            var matrixP = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);

            matrixM = renderer.transform.localToWorldMatrix;
            var matrixM_Inv = renderer.transform.worldToLocalMatrix;

            var matrixVP = matrixP * matrixV;
            var matrixMV = matrixV * matrixM;

            cmd.SetComputeVectorParam(_renderCompute, "_VecScreenParams", new Vector4(camera.pixelWidth, camera.pixelHeight));

            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixObjectToWorld", matrixM);
            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixWorldToObject", matrixM_Inv);

            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixVP", matrixVP);
            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixMV", matrixMV);
            cmd.SetComputeMatrixParam(_renderCompute, "_MatrixP", matrixP);
            cmd.SetComputeVectorParam(_renderCompute, "_VecWorldSpaceCameraPos", camera.transform.position);

            if (camera.stereoEnabled)
            {
                SetStereoCameraParams(cmd, camera, Camera.StereoscopicEye.Left, matrixM);
                SetStereoCameraParams(cmd, camera, Camera.StereoscopicEye.Right, matrixM);
            }
        }

        static void EnqueueRender(Camera camera, CameraData data, GaussianSplatRenderer renderer)
        {
            var cmd = data.Command;

            SetCameraParams(camera, data, renderer, out var matrixM);

            var calcKernel = camera.stereoEnabled ? _calcViewDataStereo : _calcViewDataMono;

            renderer.AssignDataBuffers(cmd, _renderCompute, calcKernel);

            cmd.SetComputeBufferParam(_renderCompute, calcKernel, "_SplatViewData", renderer.SplatViewData);

            cmd.DispatchCompute(_renderCompute, calcKernel, ComputeThreadGroups(renderer.SplatCount), 1, 1);

            var orderBuffer = renderer.GetOrderBuffer(camera, out _);

            _renderPropertyBlock.SetBuffer("_SplatViewData", renderer.SplatViewData);
            _renderPropertyBlock.SetBuffer("_OrderBuffer", orderBuffer);
            _renderPropertyBlock.SetInt("_SplatCount", renderer.SplatCount);

            cmd.DrawProcedural(matrixM, _renderMaterial, 0, MeshTopology.Triangles, 6, renderer.SplatCount, _renderPropertyBlock);
        }
    }
}
