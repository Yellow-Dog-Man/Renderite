using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Diagnostics;

namespace Renderite.Unity
{
    public class PerformanceStats
    {
        public int RenderedFramesSinceLast { get; set; }

        public float FrameBeginToSubmitTime { get; set; }
        public float FrameProcessedToNextBeginTime { get; set; }
        public float IntegrationProcessingTime { get; set; }
        public float ExtraParticleProcessingTime { get; set; }
        public int ProcessedAssetIntegratorTasks { get; set; }
        public int ProcessingHandleWaits { get; set; }
        public int IntegrationHighPriorityTasks { get; set; }
        public int IntegrationTasks { get; set; }
        public int IntegrationRenderTasks { get; set; }
        public int IntegrationParticleTasks { get; set; }


        public float FrameUpdateHandleTime {  get; set; }

        public int RenderedCameras { get; private set; }
        public int RenderedCameraPortals { get; private set; }
        public int UpdatedTextures { get; private set; }
        public int TextureSliceUploads { get; private set; }

        public int UploadedParticles { get; private set; }

        public void CameraRendered() => RenderedCameras++;
        public void CameraPortalRendered() => RenderedCameraPortals++;
        public void TextureUpdated() => UpdatedTextures++;
        public void TextureSliceUpdated() => TextureSliceUploads++;

        public void ParticlesUploaded(int count) => UploadedParticles += count;

        Stopwatch _framerateUpdate = new Stopwatch();
        int _framerateCounter;

        float _fps;

        public void Update()
        {
            // Update FPS
            if (!_framerateUpdate.IsRunning)
                _framerateUpdate.Restart();
            else
            {
                _framerateCounter++;

                var elapsedMilliseconds = _framerateUpdate.Elapsed.TotalMilliseconds;

                if (elapsedMilliseconds >= 500)
                {
                    _fps = (float)(_framerateCounter / (elapsedMilliseconds * 0.001f));

                    _framerateCounter = 0;
                    _framerateUpdate.Restart();
                }
            }
        }

        public void UpdateStats(FrameStartData data)
        {
            if (data.performance == null)
                data.performance = new PerformanceState();

            data.performance.renderedFramesSinceLast = RenderedFramesSinceLast;

            data.performance.frameBeginToSubmitTime = FrameBeginToSubmitTime;
            data.performance.frameProcessedToNextBeginTime = FrameProcessedToNextBeginTime;
            data.performance.integrationProcessingTime = IntegrationProcessingTime;
            data.performance.extraParticleProcessingTime = ExtraParticleProcessingTime;
            data.performance.processedAssetIntegratorTasks = ProcessedAssetIntegratorTasks;

            data.performance.integrationHighPriorityTasks = IntegrationHighPriorityTasks;
            data.performance.integrationTasks = IntegrationTasks;
            data.performance.integrationRenderTasks = IntegrationRenderTasks;
            data.performance.integrationParticleTasks = IntegrationParticleTasks;

            data.performance.processingHandleWaits = ProcessingHandleWaits;
            data.performance.frameUpdateHandleTime = FrameUpdateHandleTime;

            UpdateRenderStats(data.performance);
            UpdateFrameRate(data.performance);
        }

        void UpdateRenderStats(PerformanceState state)
        {
            state.renderedCameras = RenderedCameras;
            state.renderedCameraPortals = RenderedCameraPortals;
            state.updatedTextures = UpdatedTextures;
            state.textureSliceUploads = TextureSliceUploads;

            RenderedCameras = 0;
            RenderedCameraPortals = 0;
            UpdatedTextures = 0;
            TextureSliceUploads = 0;
        }

        void UpdateFrameRate(PerformanceState state)
        {
            if (UnityEngine.XR.XRStats.TryGetGPUTimeLastFrame(out float gpuTimeLastFrame))
                state.renderTime = gpuTimeLastFrame * 0.001f;
            else
                state.renderTime = -1;

            state.immediateFPS = 1f / Time.unscaledDeltaTime;

            state.fps = _fps;
        }
    }
}
