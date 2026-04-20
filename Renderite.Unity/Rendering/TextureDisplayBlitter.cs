using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class TextureDisplayBlitter : UnityEngine.MonoBehaviour
    {
        public UnityEngine.Texture Texture;
        public int DisplayIndex;
        public UnityEngine.Color Color;

        public bool FlipHorizontally;
        public bool FlipVertically;

        int lastRenderedDisplay = -1;

        void OnDisable()
        {
            ClearDisplay();
        }

        void OnEnable()
        {
            StartCoroutine(Blit());
        }

        void OnDestroy()
        {
            ClearDisplay();

            Deinitialize();
        }

        internal void Deinitialize()
        {
            Texture = null;
        }

        void ClearDisplay()
        {
            if (lastRenderedDisplay >= 0 && lastRenderedDisplay < UnityEngine.Display.displays.Length)
            {
                var display = UnityEngine.Display.displays[lastRenderedDisplay];

                if (display.active)
                {
                    UnityEngine.Graphics.SetRenderTarget(display.colorBuffer, display.depthBuffer);
                    UnityEngine.GL.PushMatrix();
                    UnityEngine.GL.LoadOrtho();
                    UnityEngine.GL.Color(UnityEngine.Color.white);
                    UnityEngine.GL.Clear(true, true, new UnityEngine.Color(0, 0, 0, 1));
                    UnityEngine.GL.PopMatrix();

                    UnityEngine.Graphics.SetRenderTarget(null as UnityEngine.RenderTexture);
                }
            }

            lastRenderedDisplay = -1;
        }

        IEnumerator Blit()
        {
            var material = new UnityEngine.Material(UnityEngine.Shader.Find("Unlit/Texture"));

            for (; ; )
            {
                yield return new UnityEngine.WaitForEndOfFrame();

                if (DisplayIndex != lastRenderedDisplay)
                    ClearDisplay();

                if (DisplayIndex >= 0 && DisplayIndex < UnityEngine.Display.displays.Length)
                {
                    if (Texture != null)
                    {
                        var display = UnityEngine.Display.displays[DisplayIndex];
                        if (!display.active)
                            display.Activate();

                        lastRenderedDisplay = DisplayIndex;

                        Vector2 resolution = new Vector2(display.renderingWidth, display.renderingHeight);
                        Vector2 texSize = new Vector2(Texture.width, Texture.height);

                        // normalize, so the bigger side is 1
                        var normResolution = resolution / Mathf.Max(resolution.x, resolution.y);
                        var normTexSize = texSize / Mathf.Max(texSize.x, texSize.y);

                        Rect rect;

                        if (normTexSize.x > normResolution.x || normResolution.y > normTexSize.y)
                        {
                            // match height
                            normTexSize *= normResolution.x / normTexSize.x;
                            float gap = normResolution.y - normTexSize.y;

                            rect = new Rect(0, gap * 0.5f, 1f, 1f - gap);
                        }
                        else
                        {
                            normTexSize *= normResolution.y / normTexSize.y;
                            float gap = normResolution.x - normTexSize.x;

                            rect = new Rect(gap * 0.5f, 0f, 1f - gap, 1f);
                        }

                        // flip vertically
                        rect = new Rect(rect.x * resolution.x, rect.y * resolution.y, rect.width * resolution.x, rect.height * resolution.y);

                        Graphics.SetRenderTarget(display.colorBuffer, display.depthBuffer);

                        if (FlipHorizontally ^ FlipVertically)
                            GL.invertCulling = !GL.invertCulling;

                        GL.PushMatrix();
                        GL.LoadPixelMatrix(
                            FlipHorizontally ? resolution.x : 0, FlipHorizontally ? 0 : resolution.x,
                            FlipVertically ? 0 : resolution.y, FlipVertically ? resolution.y : 0);
                        GL.Color(Color.white);
                        GL.Clear(true, true, Color);
                        material.mainTexture = Texture;
                        Graphics.DrawTexture(rect, Texture, material);
                        GL.PopMatrix();

                        if (FlipHorizontally ^ FlipVertically)
                            GL.invertCulling = !GL.invertCulling;

                        Graphics.SetRenderTarget(null as RenderTexture);
                    }
                    else
                        ClearDisplay();
                }
            }
        }
    }
}
