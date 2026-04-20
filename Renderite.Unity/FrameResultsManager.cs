using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class FrameResultsManager
    {
        List<ReflectionProbeChangeRenderResult> _finishedProbes = new List<ReflectionProbeChangeRenderResult>();

        List<VideoTextureClockErrorState> _videoTextureClockErrors = new List<VideoTextureClockErrorState>();

        public void ProbeFinishedRendering(ReflectionProbeRenderable probe, int uniqueId)
        {
            _finishedProbes.Add(new ReflectionProbeChangeRenderResult()
            {
                renderSpaceId = probe.Space.Id,
                renderProbeUniqueId = uniqueId,
                requireReset = probe.MarkedForReset
            });
        }

        public void UpdateVideoClockError(int assetId, float currentClockError)
        {
            _videoTextureClockErrors.Add(new VideoTextureClockErrorState()
            {
                assetId = assetId,
                currentClockError = currentClockError
            });
        }

        public void CollectResults(FrameStartData data)
        {
            data.renderedReflectionProbes?.Clear();
            data.videoClockErrors?.Clear();

            if (_finishedProbes.Count > 0)
            {
                if (data.renderedReflectionProbes == null)
                    data.renderedReflectionProbes = new List<ReflectionProbeChangeRenderResult>();

                data.renderedReflectionProbes.AddRange(_finishedProbes);

                _finishedProbes.Clear();
            }

            if (_videoTextureClockErrors.Count > 0)
            {
                if (data.videoClockErrors == null)
                    data.videoClockErrors = new List<VideoTextureClockErrorState>();

                data.videoClockErrors.AddRange(_videoTextureClockErrors);

                _videoTextureClockErrors.Clear();
            }
        }
    }
}
