using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Rect = UnityEngine.Rect;
using Renderite.Unity;
using Renderite.Shared;

// This is based on the modified version of the Unity water reflection 
// with added support for VR, some fixes and changes to make it more modular

[ExecuteInEditMode] // Make mirror live-update even when not in play mode
public class CameraPortal : MonoBehaviour
{
    public enum Mode
    {
        Mirror,
        Portal
    }

    public Mode RenderMode = Mode.Mirror;

    public bool DisablePixelLights = false;
    public bool DisableShadows = false;

    public float ClipPlaneOffset = 0.07f;
    public RenderTexture ReflectionTexture = null;
    public LayerMask ReflectLayers = -1;
    public Vector3 Normal = new Vector3(0, 0, 1);

    public Matrix4x4 PortalTransform;
    public Vector3 PortalPlanePosition;
    public Vector3 PortalPlaneNormal;

    //public float? OverrideNearClip;
    public float? OverrideFarClip;
    public CameraClearFlags? OverrideClearFlag;
    public Color ClearColor;

    private Dictionary<Camera, Camera> reflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table

    private static bool isRendering = false;

    static CommandBuffer stereoRenderCommandBuffer;

    // This is called when it's known that the object will be rendered by some
    // camera. We render reflections / refractions and do other updates here.
    // Because the script executes in edit mode, reflections for the scene view
    // camera will just work!
    public void OnWillRenderObject()
    {
        if (!enabled)
            return;

        var renderer = GetComponent<Renderer>();

        if (renderer == null || renderer.sharedMaterial == null || !renderer.enabled)
            return;

        if (ReflectionTexture == null)
            return;

        Camera cam = Camera.current;

        if (!cam)
            return;

        // Safeguard from recursive water reflections.		
        if (isRendering)
            return;

        //Debug.Log($"Rendering {RenderMode} for camera: {cam.transform.name}");

        var originalContext = RenderContextHelper.CurrentRenderingContext;
        RenderContextHelper.BeginRenderContext(RenderMode == Mode.Mirror ? RenderingContext.Mirror : RenderingContext.Portal);

        isRendering = true;

        var _oldNear = cam.nearClipPlane;
        var _oldFar = cam.farClipPlane;

        /*if (OverrideNearClip != null)
            cam.nearClipPlane = OverrideNearClip.Value;*/
        if (OverrideFarClip != null)
            cam.farClipPlane = OverrideFarClip.Value;

        Camera reflectionCamera;
        CreateObjects(cam, out reflectionCamera);

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.TransformDirection(Normal);

        // Optionally disable pixel lights for reflection/refraction
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        var oldShadows = QualitySettings.shadows;

        if (DisablePixelLights)
            QualitySettings.pixelLightCount = 0;

        if (DisableShadows)
            QualitySettings.shadows = ShadowQuality.Disable;

        UpdateCameraModes(cam, reflectionCamera);

        if (reflectionCamera.clearFlags != CameraClearFlags.Skybox)
        {
            // do a manual clear
            RenderTexture.active = ReflectionTexture;
            GL.Clear(reflectionCamera.clearFlags != CameraClearFlags.Nothing,
                reflectionCamera.clearFlags == CameraClearFlags.Color, reflectionCamera.backgroundColor);
            RenderTexture.active = null;

            reflectionCamera.clearFlags = CameraClearFlags.Nothing;
        }

        if (cam.stereoEnabled)
        {
            //Vector3 leftEyePos = cam.transform.TransformPoint(new Vector3(-0.5f * cam.stereoSeparation, 0, 0));
            //Matrix4x4 leftProjectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);

            //Vector3 rightEyePos = cam.transform.TransformPoint(new Vector3(0.5f * cam.stereoSeparation, 0, 0));
            ////Matrix4x4 viewMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            //Matrix4x4 rightProjectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            //reflectionCamera.stereoTargetEye = StereoTargetEyeMask.Both;

            //reflectionCamera.ResetCullingMatrix();

            //SetupCameraMatrix(reflectionCamera, leftEyePos, cam.transform.rotation, leftProjectionMatrix);
            //reflectionCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, reflectionCamera.projectionMatrix);
            //reflectionCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, reflectionCamera.worldToCameraMatrix);

            //SetupCameraMatrix(reflectionCamera, rightEyePos, cam.transform.rotation, rightProjectionMatrix);
            //reflectionCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, reflectionCamera.projectionMatrix);
            //reflectionCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, reflectionCamera.worldToCameraMatrix);

            //SetupStereoSinglePass(reflectionCamera);
            //RenderReflection(reflectionCamera, new Rect(0f, 0f, 1f, 1f));
            //ClearStereoSinglePass(reflectionCamera);

            cam.allowMSAA = false;

            if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Left)
            {
                Vector3 eyePos = cam.transform.TransformPoint(new Vector3(-0.5f * cam.stereoSeparation, 0, 0));
                //Matrix4x4 viewMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);

                //viewMatrix = viewMatrix.inverse;
                //Vector3 eyePos = new Vector3(viewMatrix.m03, viewMatrix.m13, viewMatrix.m23);

                SetupCameraMatrix(reflectionCamera, eyePos, cam.transform.rotation, projectionMatrix, cam);
                RenderReflection(reflectionCamera, new Rect(0f, 0f, 0.5f, 1f));
            }

            if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Right)
            {
                Vector3 eyePos = cam.transform.TransformPoint(new Vector3(0.5f * cam.stereoSeparation, 0, 0));
                //Matrix4x4 viewMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

                //viewMatrix = viewMatrix.inverse;
                //Vector3 eyePos = new Vector3(viewMatrix.m03, viewMatrix.m13, viewMatrix.m23);

                SetupCameraMatrix(reflectionCamera, eyePos, cam.transform.rotation, projectionMatrix, cam);
                RenderReflection(reflectionCamera, new Rect(0.5f, 0f, 0.5f, 1f));
            }
        }
        else
        {
            // IMPORTANT!!! For some reason, for Resonite made cameras, the projection matrix is missing any depth projection
            // which will make the Oblique matrix calculation produce matrix with NaN values
            // However the nonjittered matrix includes the depth parameter in the matrix, so we use that
            // to make projections work.
            SetupCameraMatrix(reflectionCamera, cam.transform.position, cam.transform.rotation, cam.nonJitteredProjectionMatrix, cam);

            RenderReflection(reflectionCamera, new Rect(0f, 0f, 1f, 1f));
        }

        cam.nearClipPlane = _oldNear;
        cam.farClipPlane = _oldFar;

        if (DisablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;

        if (DisableShadows)
            QualitySettings.shadows = oldShadows;

        isRendering = false;

        RenderingManager.Instance?.Stats?.CameraPortalRendered();

        if (originalContext != null)
            RenderContextHelper.BeginRenderContext(originalContext.Value);
    }

    void RenderReflection(Camera reflectionCamera, Rect viewPort)
    {
        if (Mathf.Abs(reflectionCamera.projectionMatrix.determinant) <= 1e-12f)
            return;

        if (Mathf.Abs(reflectionCamera.worldToCameraMatrix.determinant) <= 1e-12f)
            return;

        reflectionCamera.targetTexture = ReflectionTexture;
        reflectionCamera.rect = viewPort;

        reflectionCamera.cullingMask &= ReflectLayers.value;

        bool oldCulling = GL.invertCulling;
        if (RenderMode == Mode.Mirror)
            GL.invertCulling = !oldCulling;

        reflectionCamera.Render();

        if (RenderMode == Mode.Mirror)
            GL.invertCulling = oldCulling;
    }

    void SetupCameraMatrix(Camera reflectionCamera, Vector3 camPos, Quaternion camRot, Matrix4x4 camProjMatrix, Camera sourceCam = null)
    {
        // Copy camera position/rotation/reflection into the reflectionCamera
        reflectionCamera.ResetWorldToCameraMatrix();
        reflectionCamera.transform.position = camPos;
        reflectionCamera.transform.rotation = camRot;
        reflectionCamera.projectionMatrix = camProjMatrix;

        // Set custom culling matrix from the current camera
        // reflectionCamera.cullingMatrix = camProjMatrix * reflectionCamera.worldToCameraMatrix;

        // find out the reflection plane: position and normal in world space
        Vector3 pos;
        Vector3 normal;

        if (RenderMode == Mode.Mirror)
        {
            pos = transform.position;
            normal = transform.TransformDirection(Normal);

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);

            //Debug.Log($"Rendering in Mirror Mode. Pos: {pos}, Normal: {normal}, ClipPlaneOffset: {ClipPlaneOffset}, d: {d}," +
            //    $"ReflectionPlane: {reflectionPlane}\nReflection: {reflection}");

            reflectionCamera.worldToCameraMatrix *= reflection;
        }
        else
        {
            //Debug.Log("Rendering in Portal Mode");

            reflectionCamera.worldToCameraMatrix *= PortalTransform;

            //pos = PortalTransform.MultiplyPoint(pos);
            pos = PortalPlanePosition;
            normal = -PortalPlaneNormal;

            /*pos = PortalTransform.MultiplyPoint3x4(pos);
            normal = PortalTransform.MultiplyVector(normal).normalized;*/
        }

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1f);
        var obliqueMatrix = reflectionCamera.CalculateObliqueMatrix(clipPlane);

        //Debug.Log($"ClipPlane: {clipPlane}\nPos: {camPos}, Rot: {camRot:F4}, Oblique: {obliqueMatrix}\nProjMatrix: {camProjMatrix}\nWorldToCamera: {reflectionCamera.worldToCameraMatrix}\n" +
        //    $"NearClip: {reflectionCamera.nearClipPlane}, Far: {reflectionCamera.farClipPlane}, FOV: {reflectionCamera.fieldOfView}, Aspect: {reflectionCamera.aspect}\n\n" +
        //    $"SelfPosition: {transform.position}, SelfRotation: {transform.rotation:F4}, SelfScale: {transform.lossyScale}\n" +
        //    $"CamPosition: {sourceCam?.transform.position}, CamRotation: {sourceCam?.transform.rotation:F4}, CamScale: {sourceCam?.transform.lossyScale}," +
        //    $"CamNear: {sourceCam?.nearClipPlane}, CamFar: {sourceCam?.farClipPlane}\n\nNonJittered: {sourceCam?.nonJitteredProjectionMatrix}");

        //for (int m = 0; m < (4 * 4); m++)
        //    if (float.IsNaN(obliqueMatrix[m]))
        //    {
        //        Debug.Log("MATRIX INVALID");
        //        return;
        //    }

        //Debug.Log("MATRIX IS OK!");

        reflectionCamera.projectionMatrix = obliqueMatrix;

        // Set camera position and rotation
        reflectionCamera.transform.position = reflectionCamera.cameraToWorldMatrix.GetColumn(3);
        reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectionCamera.cameraToWorldMatrix.GetColumn(2), reflectionCamera.cameraToWorldMatrix.GetColumn(1));
    }


    HashSet<Camera> setupCameras = new HashSet<Camera>();
    void SetupStereoSinglePass(Camera camera)
    {
        if (stereoRenderCommandBuffer == null)
        {
            stereoRenderCommandBuffer = new CommandBuffer();
            stereoRenderCommandBuffer.SetSinglePassStereo(SinglePassStereoMode.SideBySide);
            stereoRenderCommandBuffer.EnableShaderKeyword("UNITY_SINGLE_PASS_STEREO");
        }

        if (setupCameras.Contains(camera))
            return;

        setupCameras.Add(camera);

        //camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, stereoRenderCommandBuffer);

        for (int i = 0; i <= (int)CameraEvent.AfterHaloAndLensFlares; i++)
            camera.AddCommandBuffer((CameraEvent)i, stereoRenderCommandBuffer);

        //camera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeLighting, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeFinalPass, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeSkybox, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeReflections, stereoRenderCommandBuffer);
        //camera.AddCommandBuffer(CameraEvent.BeforeHaloAndLensFlares, stereoRenderCommandBuffer);
    }

    void ClearStereoSinglePass(Camera camera)
    {
        //for (int i = 0; i <= (int)CameraEvent.AfterHaloAndLensFlares; i++)
        //    camera.RemoveCommandBuffer((CameraEvent)i, stereoRenderCommandBuffer);

        //camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, stereoRenderCommandBuffer);

        //camera.RemoveCommandBuffer(CameraEvent.BeforeDepthTexture, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeDepthNormalsTexture, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeFinalPass, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeSkybox, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, stereoRenderCommandBuffer);
        //camera.RemoveCommandBuffer(CameraEvent.BeforeHaloAndLensFlares, stereoRenderCommandBuffer);
    }

    // Cleanup all the objects we possibly have created
    void OnDisable()
    {
        foreach (KeyValuePair<Camera, Camera> kvp in reflectionCameras)
            Destroy((kvp.Value).gameObject);
        reflectionCameras.Clear();
    }

    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
            return;
        // set water camera to clear the same way as current camera
        if (OverrideClearFlag == null)
        {
            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;

            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
                Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }
        }
        else
        {
            dest.clearFlags = OverrideClearFlag.Value;
            dest.backgroundColor = ClearColor;
        }

        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = OverrideFarClip ?? src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
        dest.cullingMask = src.cullingMask;
    }

    // On-demand create any objects we need for water
    private void CreateObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        reflectionCamera = null;

        // Camera for reflection
        reflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
        if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            GameObject go = new GameObject("Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.stereoTargetEye = StereoTargetEyeMask.None;
            go.hideFlags = HideFlags.HideAndDontSave;
            reflectionCameras[currentCamera] = reflectionCamera;

            RenderingManager.Instance?.CameraInitializer?.RegisterCamera(reflectionCamera);
        }
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    // Calculates reflection matrix around the given plane
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }
}
