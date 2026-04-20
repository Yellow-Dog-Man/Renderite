using UnityEngine;
using System.Collections;

namespace Renderite.Unity;

public class Camera360 : MonoBehaviour
{
    RenderTexture tex;
    public Camera Camera { get; private set; }
    public Camera DisplayCamera { get; private set; }
    public int CubemapSize = -1; // Auto

    public Material projectionMaterial;

    public int TotalOutputPixels
    {
        get
        {
            if (RenderTexture.active == null)
                return Screen.width * Screen.height;
            return RenderTexture.active.width * RenderTexture.active.height;
        }
    }

    void Awake()
    {
        Camera = this.GetComponent<Camera>();
        if (Camera == null)
            Camera = gameObject.AddComponent<Camera>();

        Camera.enabled = false;
        Camera.stereoTargetEye = StereoTargetEyeMask.None;

        var displayGo = new GameObject("Display");
        displayGo.transform.SetParent(this.transform, false);

        displayGo.AddComponent<Camera360Display>().SetCamera(this);
        DisplayCamera = displayGo.AddComponent<Camera>();
        DisplayCamera.clearFlags = CameraClearFlags.Depth;
        DisplayCamera.cullingMask = 0;
        DisplayCamera.stereoTargetEye = StereoTargetEyeMask.None;
    }

    public void Render(RenderTexture tex)
    {
        var oldTex = DisplayCamera.targetTexture;
        DisplayCamera.targetTexture = tex;
        DisplayCamera.Render();
        DisplayCamera.targetTexture = oldTex;
    }

    public void RenderCubemap()
    {
        var rotation = transform.rotation;

        // check if new render texture is necessary
        int cubemapSize = CubemapSize == -1 ? Mathf.NextPowerOfTwo((int)Mathf.Sqrt((TotalOutputPixels) / 6f)) :
            Mathf.NextPowerOfTwo(CubemapSize);

        if (tex == null || tex.width != cubemapSize)
        {
            //Debug.Log("Creating CubeRenderTexture: " + cubemapSize);
            if (tex != null)
                Destroy(tex);

            tex = new RenderTexture(cubemapSize, cubemapSize, 0);
            tex.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            tex.Create();
            projectionMaterial.EnableKeyword("FLIP"); // custom cubemap render below with texture copy copies them flipped
        }

        // set it every frame, because there's a strange bug where it doesn't get assigned if there are two changes in a row
        projectionMaterial.SetTexture("_Cube", tex);
        projectionMaterial.SetMatrix("_Rotation", Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one));

        var renderTex = RenderTexture.GetTemporary(cubemapSize, cubemapSize, 24, tex.format);

        var oldTex = RenderTexture.active;

        Camera.fieldOfView = 90f;
        Camera.targetTexture = renderTex;

        // TEMPORARY!!! Putting Thread.Sleep because Unity seems to have a bug (since 2018.2.6) which causes a crash due to illegal memory access
        // Adding the Thread.Sleep(2) seems to greatly reduce the likelyhood of the crash. It also only happens in particular scene.

        Camera.transform.eulerAngles = new Vector3(0, -90, 0);
        Camera.Render();
        Graphics.CopyTexture(renderTex, 0, 0, tex, 0, 0);

        Camera.transform.eulerAngles = new Vector3(0, 90, 0);
        Camera.Render();
        Graphics.CopyTexture(renderTex, 0, 0, tex, 1, 0);

        Camera.transform.eulerAngles = new Vector3(90, 180, 0);
        Camera.Render();
        Graphics.CopyTexture(renderTex, 0, 0, tex, 2, 0);

        Camera.transform.eulerAngles = new Vector3(-90, 180, 0);
        Camera.Render();
        Graphics.CopyTexture(renderTex, 0, 0, tex, 3, 0);

        Camera.transform.eulerAngles = new Vector3(0, 180, 0);
        Camera.Render();
        Graphics.CopyTexture(renderTex, 0, 0, tex, 4, 0);

        Camera.transform.eulerAngles = new Vector3(0, 0, 0);
        Camera.Render();
        Graphics.CopyTexture(renderTex, 0, 0, tex, 5, 0);

        RenderTexture.active = oldTex;
        RenderTexture.ReleaseTemporary(renderTex);

        //Camera.RenderToCubemap(tex);

        Camera.transform.rotation = rotation;
    }
}
