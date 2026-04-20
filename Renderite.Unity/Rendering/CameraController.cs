using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Renderite.Shared;

namespace Renderite.Unity
{
    public class CameraController : UnityEngine.MonoBehaviour
    {
        public float NearClip;
        public float FarClip;
        public bool DoubleBuffer;
        public bool UseTransformScale;
        public float OrthographicSize;
        public int RenderToDisplay;
        public bool RenderShadows;
        public UnityEngine.Camera Camera;
        public UnityEngine.RenderTexture Texture;

        public List<UnityEngine.GameObject> SelectiveRender = new List<UnityEngine.GameObject>();
        public List<UnityEngine.GameObject> ExcludeRender = new List<UnityEngine.GameObject>();

        UnityEngine.RenderTexture _prevTexture;
        UnityEngine.Rect? _prevRect;

        UnityEngine.ShadowQuality? _prevShadowQuality;

        Dictionary<UnityEngine.GameObject, int> _previousLayers;

        RenderingContext? _prevContext;

        public void OnPreCull()
        {
            try
            {
                if (!RenderShadows)
                {
                    _prevShadowQuality = UnityEngine.QualitySettings.shadows;
                    UnityEngine.QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
                }

                if (SelectiveRender.Count > 0)
                {
                    int layer = UnityEngine.LayerMask.NameToLayer(RenderHelper.TEMP_LAYER);

                    if (_previousLayers == null)
                        _previousLayers = new Dictionary<UnityEngine.GameObject, int>();

                    RenderHelper.SetHiearchyLayer(SelectiveRender, layer, _previousLayers);
                    RenderHelper.RestoreHiearachyLayer(ExcludeRender, _previousLayers);
                }
                else if (ExcludeRender.Count > 0)
                {
                    int layer = UnityEngine.LayerMask.NameToLayer(RenderHelper.TEMP_LAYER);

                    if (_previousLayers == null)
                        _previousLayers = new Dictionary<UnityEngine.GameObject, int>();

                    RenderHelper.SetHiearchyLayer(ExcludeRender, layer, _previousLayers);
                }

                _prevContext = RenderContextHelper.CurrentRenderingContext;
                RenderContextHelper.BeginRenderContext(RenderingContext.Camera);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Exception in Camera OnPreCull\n" + ex);
            }
        }

        public void OnPreRender()
        {
            try
            {
                float scale;

                if (UseTransformScale)
                {
                    var lossyScale = Camera.transform.lossyScale;
                    scale = (lossyScale.x + lossyScale.y + lossyScale.z) * 0.333333333333333f;
                }
                else
                    scale = 1;

                if (float.IsNaN(scale))
                    scale = 0f;
                
                scale = Mathf.Clamp(scale, 1e-5f, 1e6f);

                var orthoSize = OrthographicSize * scale;
                var nearClip = NearClip * scale;
                var farClip = FarClip * scale;

                orthoSize = Mathf.Clamp(orthoSize, 1e-6f, 1e6f);
                nearClip = Mathf.Clamp(nearClip, 1e-4f, 1e6f);
                farClip = Mathf.Clamp(farClip, Mathf.Max(1e-4f, nearClip + 1e-4f), 1e6f);

                Camera.orthographicSize = orthoSize;
                Camera.nearClipPlane = nearClip;
                Camera.farClipPlane = farClip;

                // For some reason unity sometimes resets the targetTexture outside of our control, force it to be assigned here
                if (Texture != null)
                    Camera.targetTexture = Texture;

                if (DoubleBuffer)
                {
                    if (Camera.targetTexture == null)
                        return;

                    var descriptor = Camera.targetTexture.descriptor;

                    if (Camera.rect != new UnityEngine.Rect(0f, 0f, 1f, 1f))
                    {
                        descriptor.height = (int)(Camera.rect.height * descriptor.height);
                        descriptor.width = (int)(Camera.rect.width * descriptor.width);

                        _prevRect = Camera.rect;
                        Camera.rect = new UnityEngine.Rect(0f, 0f, 1f, 1f);
                    }

                    var temp = UnityEngine.RenderTexture.GetTemporary(descriptor);
                    _prevTexture = Camera.targetTexture;
                    Camera.targetTexture = temp;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Exception in Camera OnPreRender\n" + ex);
            }
        }

        public void OnPostRender()
        {
            try
            {
                RenderingManager.Instance.Stats.CameraRendered();

                if (_prevShadowQuality.HasValue)
                {
                    UnityEngine.QualitySettings.shadows = _prevShadowQuality.Value;
                    _prevShadowQuality = null;
                }

                if (_previousLayers != null && _previousLayers.Count > 0)
                {
                    RenderHelper.RestoreLayers(_previousLayers);
                    _previousLayers.Clear();
                }

                if (_prevContext != null)
                    RenderContextHelper.BeginRenderContext(_prevContext.Value);

                if (DoubleBuffer)
                {
                    if (Camera.targetTexture == null)
                        return;

                    if (_prevRect != null)
                    {
                        UnityEngine.Graphics.CopyTexture(Camera.targetTexture, 0, 0, 0, 0, Camera.targetTexture.width, Camera.targetTexture.height,
                            _prevTexture, 0, 0, (int)(_prevRect.Value.x * _prevTexture.width), (int)(_prevRect.Value.y * _prevTexture.height));

                        Camera.rect = _prevRect.Value;
                        _prevRect = null;
                    }
                    else
                        UnityEngine.Graphics.CopyTexture(Camera.targetTexture, _prevTexture);

                    var temp = Camera.targetTexture;
                    Camera.targetTexture = _prevTexture;
                    _prevTexture = null;
                    UnityEngine.RenderTexture.ReleaseTemporary(temp);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Exception in Camera OnPreCull\n" + ex);
            }
        }
    }
}
