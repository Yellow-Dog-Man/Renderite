using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class CameraInitializer : MonoBehaviour
    {
        public void RegisterCamera(Camera camera)
        {
            camera.gameObject.AddComponent<ShaderCameraProperties>();
        }

        public void CleanupCamera(Camera camera)
        {
            Destroy(camera.gameObject.GetComponent<ShaderCameraProperties>());
        }

        public void SetupCamera(Camera camera, CameraSettings settings)
        {
            camera.backgroundColor = Color.black;
            camera.cullingMask = ~LayerMask.GetMask("Hidden", "Overlay");

            if(settings.SetupPostProcessing)
            {
                camera.allowHDR = true;
                SetupPostprocessing(camera, settings);
            }
        }

        public abstract void SetupPostprocessing(Camera camera, CameraSettings settings);
        public abstract void RemovePostProcessing(Camera camera);
    }
}
