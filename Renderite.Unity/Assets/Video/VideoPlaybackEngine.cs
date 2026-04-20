using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class VideoPlaybackEngine
    {
        public string Name { get; private set; }
        public Func<GameObject, IVideoPlaybackInstance> Instantiate { get; private set; }
        public int InitializationAttempts { get; private set; }

        public VideoPlaybackEngine(string name, Func<GameObject, IVideoPlaybackInstance> instantiate, int initializationAttempts)
        {
            Name = name;
            Instantiate = instantiate;
            InitializationAttempts = initializationAttempts;
        }
    }
}
