using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renderite.Unity
{
    public class LightsBufferRenderer : Renderable
    {
        public int GlobalUniqueId { get; private set; } = -1;

        List<UnityEngine.Light> _lights = new List<UnityEngine.Light>();

        object _dataLock = new object();

        bool _submissionScheduled;
        int _dataLength;
        UnityLightData[] _bufferedData = null;

        UnityEngine.LightType type;
        LightShadows shadows;
        float shadowStrength;
        float shadowNearPlane;
        int shadowCustomResolution;
        float shadowBias;
        float shadowNormalBias;

        Texture cookie;

        protected override void Cleanup()
        {
            RenderingManager.Instance.Unregister(this);
            GlobalUniqueId = -2;

            foreach (var light in _lights)
                UnityEngine.Object.Destroy(light.gameObject);

            _lights.Clear();
        }

        protected override void Setup(Transform root)
        {
            
        }

        public void HandleSubmission(LightsBufferRendererSubmission submission)
        {
            // Create a copy of the data to allow double buffering and locking the buffer for too long
            var data = RenderingManager.Instance.SharedMemory.AccessData(submission.lights.As<UnityLightData>());
            data = data.Slice(0, submission.lightsCount);

            lock (_dataLock)
            {
                if ((_bufferedData?.Length ?? 0) < data.Length)
                    _bufferedData = new UnityLightData[data.Length];

                data.CopyTo(_bufferedData);

                _dataLength = data.Length;

                if(!_submissionScheduled)
                {
                    // Schedule this to be processed
                    RenderingManager.Instance.AssetIntegrator.EnqueueParticleProcessing(SubmitLightsData);

                    _submissionScheduled = true;
                }
            }

            // Send buffer consumed
            var consumed = new LightsBufferRendererConsumed();
            consumed.globalUniqueId = submission.lightsBufferUniqueId;

            RenderingManager.Instance.SendBufferConsumed(consumed);

            PackerMemoryPool.Instance.Return(submission);
        }

        void SubmitLightsData()
        {
            lock(_dataLock)
            {
                if(_bufferedData != null)
                    SubmitLightsBuffer(_bufferedData.AsSpan(0, _dataLength));

                _submissionScheduled = false;
            }
        }

        void SubmitLightsBuffer(Span<UnityLightData> lights)
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Submitting lights buffer: {lights.Length}");

            var globalScale = ActualTransform.lossyScale;
            var avgScale = (globalScale.x + globalScale.y + globalScale.z) * 0.333333333333333f;

            var root = ActualTransform;

            for (int i = 0; i < lights.Length; i++)
            {
                UnityEngine.Light light;

                if (_lights.Count <= i)
                {
                    // Allocate a new light
                    var go = new GameObject(RenderingManager.IsDebug ? $"LightBufferLight {GlobalUniqueId}" : "");
                    go.transform.SetParent(root, false);
                    go.layer = root.gameObject.layer;

                    light = go.AddComponent<UnityEngine.Light>();

                    AssignLightProperties(light);

                    _lights.Add(light);
                }
                else
                    light = _lights[i];

                ref var l = ref lights[i];

                light.transform.localPosition = l.point;
                light.transform.localRotation = l.orientation;

                light.intensity = Mathf.Clamp(MathHelper.FilterInvalid(l.intensity), -1024, 1024);

                var rgb = l.color;

                var r = Mathf.Clamp(MathHelper.FilterInvalid(rgb.x), -64, 64);
                var g = Mathf.Clamp(MathHelper.FilterInvalid(rgb.y), -64, 64);
                var b = Mathf.Clamp(MathHelper.FilterInvalid(rgb.z), -64, 64);

                light.color = new Color(r, g, b);

                light.range = MathHelper.FilterInvalid(l.range * avgScale);
                light.spotAngle = Mathf.Clamp(MathHelper.FilterInvalid(l.angle), 0f, 180f);
            }

            // Remove excess lights
            while (_lights.Count > lights.Length)
            {
                var lastIndex = _lights.Count - 1;

                UnityEngine.Object.Destroy(_lights[lastIndex].gameObject);
                _lights.RemoveAt(lastIndex);
            }
        }

        public void ApplyState(ref LightsBufferRendererState state)
        {
            if (GlobalUniqueId < 0)
            {
                GlobalUniqueId = state.globalUniqueId;
                RenderingManager.Instance.Register(this);
            }
            else if (GlobalUniqueId != state.globalUniqueId)
                throw new InvalidOperationException("GlobalUniqueID cannot be changed after being assigned");

            type = state.lightType.ToUnity();
            shadows = state.shadowType.ToUnity();
            shadowStrength = state.shadowStrength;
            shadowNearPlane = state.shadowNearPlane;
            shadowCustomResolution = state.shadowMapResolution;
            shadowBias = state.shadowBias;
            shadowNormalBias = state.shadowNormalBias;

            cookie = TextureHelper.GetTexture(state.cookieTextureAssetId);

            AssignAllLightProperties();
        }

        void AssignAllLightProperties()
        {
            // Update properties of all current lights
            foreach (var light in _lights)
                AssignLightProperties(light);
        }

        void AssignLightProperties(UnityEngine.Light light)
        {
            light.type = type;
            light.shadows = shadows;
            light.shadowStrength = shadowStrength;
            light.shadowNearPlane = shadowNearPlane;
            light.shadowCustomResolution = shadowCustomResolution;
            light.shadowBias = shadowBias;
            light.shadowNormalBias = shadowNormalBias;
            light.cookie = cookie;
        }
    }
}
