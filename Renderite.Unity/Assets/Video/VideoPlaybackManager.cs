using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class VideoPlaybackManager : MonoBehaviour
    {
        public IReadOnlyList<VideoPlaybackEngine> AvailablePlaybackEngines => _playbackEngines;

        List<VideoPlaybackEngine> _playbackEngines = new List<VideoPlaybackEngine>();

        public VideoPlaybackEngine FindPlaybackEngine(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return _playbackEngines.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        protected void RegisterPlaybackEngine(VideoPlaybackEngine engine) => _playbackEngines.Add(engine);
    }
}
