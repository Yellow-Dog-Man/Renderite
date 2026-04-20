using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renderite.Unity
{
    public class ShaderCameraProperties : MonoBehaviour
    {
        private void OnPreRender()
        {
            int eyeIndex = -1;

            var cam = Camera.current;

            if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                eyeIndex = 0;
            else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
                eyeIndex = 1;

            Shader.SetGlobalInt("_stereoActiveEye", eyeIndex);
            Shader.SetGlobalVector("_nonJitteredWorldSpaceCameraPos", transform.position);
        }
    }
}
