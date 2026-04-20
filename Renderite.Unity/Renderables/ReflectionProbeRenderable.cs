using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Renderite.Unity
{
    public class ReflectionProbeRenderable : Renderable
    {
        public ReflectionProbe Probe { get; private set; }

        public bool MarkedForReset => _resetProbe;

        GameObject root;

        int? _currentRenderIndex;
        bool _resetProbe;
        bool _renderAgain;
        int _renderAgainUniqueId;

        bool? _lastBaked;

        protected override void Cleanup()
        {
            if (Probe == null)
                return;

            // IMPORTANT!!! This has to be DestroyImmediate, because we might possibly need to re-attach
            // the probe right away and that would interfere
            UnityEngine.Object.DestroyImmediate(Probe);
            Probe = null;
        }

        internal void ApplyState(ref ReflectionProbeState update, AssetManager<CubemapAsset> cubemaps)
        {
            // If it was realtime and we switch it back to baked, this can break so we want to reset the probe
            var isBaked = update.type == ReflectionProbeType.Baked;

            if (_lastBaked != null && isBaked != _lastBaked)
                MarkProbeForReset();

            _lastBaked = isBaked;

            EnsureValidProbe();

            switch (update.type)
            {
                case ReflectionProbeType.Baked:
                    Probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;

                    if (update.cubemapAssetId < 0)
                        Probe.customBakedTexture = null;
                    else
                        Probe.customBakedTexture = cubemaps.GetAsset(update.cubemapAssetId).Texture;

                    Probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
                    break;

                case ReflectionProbeType.Realtime:
                    Probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                    Probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
                    Probe.customBakedTexture = null;
                    break;

                case ReflectionProbeType.OnChanges:
                    Probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                    Probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
                    Probe.customBakedTexture = null;
                    break;
            }

            Probe.importance = update.importance;
            Probe.intensity = update.intensity;
            Probe.blendDistance = update.blendDistance;
            Probe.boxProjection = update.useBoxProjection;

            Probe.size = update.boxSize.ToUnity();

            Probe.timeSlicingMode = update.timeSlicingMode.ToUnity();

            Probe.resolution = update.resolution;
            Probe.hdr = update.HDR;
            Probe.clearFlags = update.clearFlags.ToUnity();

            Probe.backgroundColor = update.backgroundColor.ToUnity();

            Probe.nearClipPlane = update.nearClip;
            Probe.farClipPlane = update.farClip;

            Probe.cullingMask = update.skyboxOnly ? 0 : RenderHelper.PUBLIC_RENDER_MASK;
        }

        protected override void Setup(Transform root)
        {
            this.root = root.gameObject;

            AttachProbe();
        }

        public void MarkProbeForReset()
        {
            _resetProbe = true;
        }

        public void EnsureValidProbe()
        {
            if (!_resetProbe)
                return;

            Cleanup();
            AttachProbe();

            _resetProbe = false;
        }

        void AttachProbe()
        {
            Probe = root.AddComponent<UnityEngine.ReflectionProbe>();
            Probe.cullingMask = RenderHelper.PUBLIC_RENDER_MASK;
        }

        public void StartRender(int uniqueId)
        {
            if (_currentRenderIndex != null)
            {
                _renderAgain = true;
                _renderAgainUniqueId = uniqueId;
                return;
            }

            _currentRenderIndex = Probe.RenderProbe();

            RenderingManager.Instance.StartCoroutine(HandleRenderResult(uniqueId));
        }

        IEnumerator HandleRenderResult(int uniqueId)
        {
            bool renderingFinished = false;

            do
            {
                yield return new WaitForEndOfFrame();

                // Check if the probe finished rendering
                try
                {
                    var probe = Probe;

                    // We use this check, because it checks both against Probe actually being null and also being "Unity Null", which
                    // happens when it's destroyed
                    if (probe == null)
                        continue;

                    renderingFinished = probe.IsFinishedRendering(_currentRenderIndex.Value);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Exception when checking state of ReflectionProbe render. " +
                        $"RenderIndex: {_currentRenderIndex}, RenderAgain: {_renderAgain}, Reset: {_resetProbe}, Probe: {Probe}:\n" + ex);
                    break;
                }

            } while (!renderingFinished && Probe != null && Probe.enabled && Probe.gameObject.activeInHierarchy
                && Probe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting);

            // Reset the probe for the render. There's a bug in Unity where if the probe ends up disabled mid-timesliced render
            // it'll end up perpetually stuck and not be able to render anymore. We'll need to destroy it and create new one
            // in its stead so it will continue rendering.
            if (Probe != null)
            {
                if (!renderingFinished || !Probe.enabled || !Probe.gameObject.activeInHierarchy)
                    MarkProbeForReset();
            }

            _currentRenderIndex = null;

            if(Probe != null)
                RenderingManager.Instance.Results.ProbeFinishedRendering(this, uniqueId);

            if(renderingFinished && _renderAgain)
            {
                _renderAgain = false;
                StartRender(_renderAgainUniqueId);
            }
        }

        public void RenderToTexture(ReflectionProbeRenderTask task)
        {
            List<GameObject> excludeObjects = null;

            if ((task.excludeTransformIds?.Count ?? 0) > 0)
            {
                excludeObjects = new List<GameObject>();

                var renderSpace = Space;

                for (int i = 0; i < task.excludeTransformIds.Count; i++)
                    excludeObjects.Add(renderSpace.Transforms[task.excludeTransformIds[i]].gameObject);
            }

            RenderingManager.Instance.AssetIntegrator.EnqueueTask(() =>
            {
                var renderer = ActualTransform.gameObject.AddComponent<ReflectionProbeRenderer>();

                renderer.probe = Probe;
                renderer.task = task;
                renderer.renderable = this;

                var desc = new UnityEngine.RenderTextureDescriptor(task.size, task.size,
                    task.hdr ? UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat
                    : UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, 24, -1);

                desc.useMipMap = true;
                desc.dimension = UnityEngine.Rendering.TextureDimension.Cube;
                desc.autoGenerateMips = false;

                var tex = UnityEngine.RenderTexture.GetTemporary(desc);

                renderer.texture = tex;

                if (excludeObjects != null)
                {
                   renderer.previousLayers = new Dictionary<GameObject, int>();
                   RenderHelper.SetHiearchyLayer(excludeObjects, UnityEngine.LayerMask.NameToLayer(RenderHelper.TEMP_LAYER), renderer.previousLayers);
                }

                Probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing;

                renderer.renderId = Probe.RenderProbe(tex);
            });
        }
    }
}
