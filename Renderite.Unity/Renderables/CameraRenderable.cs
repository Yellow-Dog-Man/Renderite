using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class CameraRenderable : Renderable
    {
        public Camera Camera { get; private set; }
        public CameraController Helper { get; private set; }

        public bool PostprocessingSetup = false;
        public bool ScreenspaceReflectionsSetup = false;
        public bool MotionBlurSetup = false;

        protected override void Cleanup()
        {
            RenderingManager.Instance.CameraInitializer.CleanupCamera(Camera);

            if (PostprocessingSetup)
            {
                RenderingManager.Instance.CameraInitializer.RemovePostProcessing(Camera);
                PostprocessingSetup = false;
            }

            UnityEngine.Object.Destroy(Helper);
            UnityEngine.Object.Destroy(Camera);

            Camera = null;
            Helper = null;
        }

        protected override void Setup(Transform root)
        {
            var go = root.gameObject;

            Camera = go.AddComponent<Camera>();

            Camera.allowHDR = true;
            Camera.stereoTargetEye = StereoTargetEyeMask.None;

            RenderingManager.Instance.CameraInitializer.RegisterCamera(Camera);

            Helper = go.AddComponent<CameraController>();
            Helper.Camera = Camera;
        }
    }
}
