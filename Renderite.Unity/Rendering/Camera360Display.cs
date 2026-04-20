using Renderite.Unity;
using System.Collections;
using UnityEngine;

namespace Renderite.Unity;

public class Camera360Display : MonoBehaviour
{
    Camera360 camera360;

    public void SetCamera(Camera360 camera360)
    {
        this.camera360 = camera360;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        camera360.RenderCubemap();
        Graphics.Blit(/*src*/null, dest, camera360.projectionMaterial);
    }
}
