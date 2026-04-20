using Renderite.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Camera = UnityEngine.Camera;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace Renderite.Unity
{
    public class HeadOutput : MonoBehaviour
    {
        public const float INITIAL_HEIGHT = 1.75f;

        public enum HeadOutputType
        {
            VR,
            Screen,
            Screen360,
            Static
        }

        public HeadOutputType Type;

        public bool AllowMotionBlur;
        public bool AllowScreenSpaceReflection;
        public List<Camera> cameras;

        public Transform CameraRoot => cameras[0].transform;

        bool _isUserView;

        bool _overrideView;

        Vector3 _viewPos = Vector3.up * 1.75f;
        Quaternion _viewRot = Quaternion.identity;
        Vector3 _viewScl = Vector3.one;
        Vector3 _rootScl = Vector3.one;

        public float NearClipPlane
        {
            get => cameras[0].nearClipPlane;
            set
            {
                foreach (var c in cameras)
                    c.nearClipPlane = value;
            }
        }

        public float FarClipPlane
        {
            get => cameras[0].farClipPlane;
            set
            {
                foreach (var c in cameras)
                    c.farClipPlane = value;
            }
        }

        void Awake()
        {
            _overrideView = Type != HeadOutputType.VR;

            if (_overrideView)
                UpdateOverridenView();

            var settings = new CameraSettings()
            {
                IsPrimary = true,
                IsVR = Type == HeadOutputType.VR,
                MotionBlur = AllowMotionBlur,
                ScreenSpaceReflection = AllowScreenSpaceReflection,
                SetupPostProcessing = Application.platform != RuntimePlatform.Android
            };

            Camera.onPreCull += OnPreCull;
            Camera.onPostRender += OnPostRender;

            foreach (var c in cameras)
            {
                // this fixes bloom leaking around the edges in VR
                if (Type == HeadOutputType.VR)
                {
                    var buffer = new CommandBuffer();
                    buffer.name = "ClearRenderTarget";
                    buffer.ClearRenderTarget(false, true, Color.black);

                    c.AddCommandBuffer(CameraEvent.BeforeGBuffer, buffer);
                }

                RenderingManager.Instance.CameraInitializer.SetupCamera(c, settings);
            }
        }

        static Vector3 FilterScale(in Vector3 scale) => Mathf.Min(scale.x, scale.y, scale.z) <= 1e-8 ? Vector3.one : scale;
        static Vector3 Mul(in Vector3 a, in Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        static Vector3 Div(in Vector3 a, in Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        void OnPreCull(Camera camera)
        {
            if (camera != cameras[0])
                return;

            RenderContextHelper.BeginRenderContext(_isUserView ? RenderingContext.UserView : RenderingContext.ExternalView);

            UpdateOverridenView();
        }

        void OnPostRender(Camera camera)
        {
            if (camera != cameras[0])
                return;

            // Make sure we end the render context after head finished rendering so it doesn't linger
            RenderContextHelper.EndCurrentRenderContext();
        }

        void UpdateOverridenView()
        {
            var cameraTransform = CameraRoot;

            if (_overrideView)
            {
                switch (Type)
                {
                    case HeadOutputType.Screen:
                        // simply position the camera view independently of the root
                        cameraTransform.position = _viewPos;
                        cameraTransform.rotation = _viewRot;
                        cameraTransform.localScale = FilterScale(Div(_viewScl, _rootScl));

                        break;

                    case HeadOutputType.Screen360:
                        // simply position the camera view independently of the root
                        cameraTransform.position = _viewPos;
                        cameraTransform.localScale = FilterScale(Div(_viewScl, _rootScl));
                        break;

                    case HeadOutputType.VR:
                        // This must actually calculate position of the root, so it can be compensated
                        var currentViewScl = CameraRoot.lossyScale;
                        transform.localScale = Mul(FilterScale(Div(_viewScl, currentViewScl)), transform.localScale);

                        var currentViewRot = CameraRoot.rotation;
                        transform.localRotation = (_viewRot * UnityEngine.Quaternion.Inverse(currentViewRot)) * transform.localRotation;

                        var currentViewPos = CameraRoot.position;
                        transform.localPosition += _viewPos - currentViewPos;
                        break;
                }
            }
        }

        public void UpdatePositioning(RenderSpace renderSpace)
        {
            var rootPos = renderSpace.RootPosition;
            var rootRot = renderSpace.RootRotation;
            var rootScl = FilterScale(renderSpace.RootScale);

            transform.position = rootPos;
            transform.rotation = rootRot;
            transform.localScale = rootScl;

            var nearClip = RenderingManager.Instance.NearClip;
            var farClip = RenderingManager.Instance.FarClip;

            nearClip = Mathf.Max(Type == HeadOutputType.Screen360 ? 0.25f : 0.001f, nearClip);
            farClip = Mathf.Max(0.5f, farClip);

            NearClipPlane = nearClip * rootScl.x;
            FarClipPlane = farClip;

            if (Type == HeadOutputType.Screen || Type == HeadOutputType.Static)
            {
                foreach (var c in cameras)
                    c.fieldOfView = RenderingManager.Instance.DesktopFOV;
            }

            if (renderSpace.OverrideViewPosition)
            {
                _overrideView = true;

                _viewPos = renderSpace.OverridenViewPosition;
                _viewRot = renderSpace.OverridenViewRotation;
                _viewScl = FilterScale(renderSpace.OverridenViewScale);
                _rootScl = rootScl;

                // need to update it right away so the userspace is properly offset after this function finishes
                UpdateOverridenView();
            }
            else
                _overrideView = false;

            _isUserView = !renderSpace.ViewPositionIsExternal;
        }

        public static HeadOutput GetHeadObject(HeadOutputDevice device)
        {
            string deviceName;

            switch (device)
            {
                case HeadOutputDevice.Screen:
                    deviceName = "Screen";
                    break;

                case HeadOutputDevice.Screen360:
                    deviceName = "Screen360";
                    break;

                case HeadOutputDevice.Oculus:
                case HeadOutputDevice.OculusQuest:
                    deviceName = "Oculus";
                    break;

                case HeadOutputDevice.SteamVR:
                case HeadOutputDevice.WindowsMR:
                    deviceName = "SteamVR";
                    break;

                default:
                    deviceName = device.ToString();
                    break;
            }

            Debug.Log("DeviceName: " + deviceName);

            return Instantiate(Resources.Load<GameObject>("HeadRenderers/" + device)).GetComponentInChildren<HeadOutput>();
        }
    }
}
