using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Renderite.Unity;

public class CameraRenderer
{
    static bool _initialized;

    static Camera360 camera360;
    static UnityEngine.Camera camera;

    static int _privateLayerMask;
    static int _hiddenLayerMask;

    static Dictionary<GameObject, int> previousLayers = new Dictionary<GameObject, int>();
    static List<GameObject> renderObjects = new List<GameObject>();
    static List<GameObject> excludeObjects = new List<GameObject>();

    public static void Initialize()
    {
        if (_initialized)
            throw new InvalidOperationException("CameraRenderer is already initialized");

        _initialized = true;

        var camGo = new GameObject("CaptureCam");
        var camGo360 = new GameObject("CaptureCam360");

        camGo.tag = RenderHelper.CAPTURE_CAMERA_TAG;
        camGo360.tag = RenderHelper.CAPTURE_CAMERA_TAG;

        camera = camGo.AddComponent<UnityEngine.Camera>();
        camGo.AddComponent<ShaderCameraProperties>();
        camera.stereoTargetEye = StereoTargetEyeMask.None;
        camera.enabled = false;
        camera.nearClipPlane = 0.05f;

        var settings = new CameraSettings()
        {
            IsSingleCapture = true,
            SetupPostProcessing = true,
        };

        RenderingManager.Instance.CameraInitializer.SetupCamera(camera, settings);

        camera360 = camGo360.AddComponent<Camera360>();
        camera360.DisplayCamera.enabled = false;
        camera360.Camera.nearClipPlane = 0.05f;

        camera360.projectionMaterial = Resources.Load<UnityEngine.Material>("EquirectangularProjection");

        RenderingManager.Instance.CameraInitializer.SetupCamera(camera360.Camera, settings);
        camera360.Camera.gameObject.AddComponent<ShaderCameraProperties>();

        _privateLayerMask = ~LayerMask.GetMask(RenderHelper.PRIVATE_LAYER);
        _hiddenLayerMask = ~LayerMask.GetMask(RenderHelper.HIDDEN_LAYER, RenderHelper.OVERLAY_LAYER);
    }

    public static void Render(CameraRenderTask task)
    {
        var textureData = RenderingManager.Instance.SharedMemory.AccessData(task.resultData);

        var renderSpace = RenderingManager.Instance.TryGetRenderSpace(task.renderSpaceId);

        // If the space doesn't exist anymore or is not active, fail the render
        if(renderSpace == null || !renderSpace.IsActive)
        {
            textureData.Clear();
            return;
        }

        var tex = new UnityEngine.Texture2D(task.parameters.resolution.x, task.parameters.resolution.y,
            task.parameters.textureFormat.ToUnity(), false);

        var renderTex = UnityEngine.RenderTexture.GetTemporary(task.parameters.resolution.x, task.parameters.resolution.y,
            24, RenderTextureFormat.ARGB32);

        var previousTex = UnityEngine.RenderTexture.active;

        var layerIndex = LayerMask.NameToLayer(RenderHelper.TEMP_LAYER);
        var layerMask = 1 << layerIndex;

        if (task.excludeRenderList != null)
        {
            foreach (var s in task.excludeRenderList)
            {
                var go = renderSpace.Transforms[s].gameObject;

                excludeObjects.Add(go);
            }
        }

        if (task.onlyRenderList != null)
        {
            foreach (var s in task.onlyRenderList)
            {
                var go = renderSpace.Transforms[s].gameObject;

                renderObjects.Add(go);
            }
        }

        if (renderObjects.Count > 0)
        {
            RenderHelper.SetHiearchyLayer(renderObjects, layerIndex, previousLayers);
            // restore the excluded objects
            RenderHelper.RestoreHiearachyLayer(excludeObjects, previousLayers);
        }
        else if (excludeObjects.Count > 0)
            RenderHelper.SetHiearchyLayer(excludeObjects, layerIndex, previousLayers);

        var cameraSettings = new CameraSettings()
        {
            IsPrimary = false,
            IsSingleCapture = true,
            IsVR = false,
            MotionBlur = false,
            SetupPostProcessing = task.parameters.postProcessing,
            ScreenSpaceReflection = task.parameters.screenSpaceReflections,
        };

        // TODO!!! Better handling, this just renders 360 automatically
        if (task.parameters.fov >= 180f)
        {
            if (renderObjects.Count > 0)
                camera360.Camera.cullingMask = layerMask & _hiddenLayerMask;
            else
            {
                camera360.Camera.cullingMask = ~layerMask & _hiddenLayerMask;

                if (!task.parameters.renderPrivateUI)
                    camera360.Camera.cullingMask &= _privateLayerMask;
            }

            RenderingManager.Instance.CameraInitializer.SetupPostprocessing(camera360.Camera, cameraSettings);
            //camera360.Camera.gameObject.GetComponent<PostProcessingBehaviour>().enabled = r.postProcesing;

            camera360.transform.position = task.position.ToUnity();
            camera360.transform.rotation = task.rotation.ToUnity();
            camera360.Camera.clearFlags = task.parameters.clearMode.ToUnity();
            camera360.Camera.backgroundColor = task.parameters.clearColor.ToUnity();
            camera360.Camera.nearClipPlane = task.parameters.nearClip;
            camera360.Camera.farClipPlane = task.parameters.farClip;
            camera360.Render(renderTex);
        }
        else
        {
            if (renderObjects.Count > 0)
                camera.cullingMask = layerMask & _hiddenLayerMask;
            else
            {
                camera.cullingMask = ~layerMask & _hiddenLayerMask;

                if (!task.parameters.renderPrivateUI)
                    camera.cullingMask &= _privateLayerMask;
            }

            RenderingManager.Instance.CameraInitializer.SetupPostprocessing(camera, cameraSettings);
            //camera.gameObject.GetComponent<PostProcessingBehaviour>().enabled = r.postProcesing;

            camera.transform.position = task.position.ToUnity();
            camera.transform.rotation = task.rotation.ToUnity();
            camera.clearFlags = task.parameters.clearMode.ToUnity();
            camera.backgroundColor = task.parameters.clearColor.ToUnity();
            camera.nearClipPlane = task.parameters.nearClip;
            camera.farClipPlane = task.parameters.farClip;
            camera.targetTexture = renderTex;
            camera.fieldOfView = task.parameters.fov;
            camera.orthographicSize = task.parameters.orthographicSize;
            camera.orthographic = task.parameters.projection == CameraProjection.Orthographic;
            camera.Render();
        }

        if (renderObjects.Count > 0)
            RenderHelper.RestoreHiearachyLayer(renderObjects, previousLayers);
        else if (excludeObjects.Count > 0)
        {
            RenderHelper.RestoreHiearachyLayer(excludeObjects, previousLayers);
        }

        previousLayers.Clear();

        renderObjects.Clear();
        excludeObjects.Clear();

        UnityEngine.RenderTexture.active = renderTex;

        tex.ReadPixels(new UnityEngine.Rect(0, 0, task.parameters.resolution.x, task.parameters.resolution.y), 0, 0, false);

        UnityEngine.RenderTexture.active = previousTex;

        UnityEngine.RenderTexture.ReleaseTemporary(renderTex);

        // Copy the result to the shared memory buffer
        if (RenderingManager.IsDebug)
        {
            var data = tex.GetRawTextureData();
            data.CopyTo(textureData);
        }
        else
        {
            unsafe
            {
                using (var data = tex.GetRawTextureData<byte>())
                {
                    var dataSpan = new Span<byte>(data.GetUnsafeReadOnlyPtr(), data.Length);
                    dataSpan.CopyTo(textureData);
                }
            }
        }        

        UnityEngine.Object.Destroy(tex);
    }
}
