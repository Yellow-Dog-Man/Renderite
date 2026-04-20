using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class EngineInitProgress : MonoBehaviour
    {
        public abstract void InitStarted();
        public abstract void ApplySplashScreenOverride(RendererSplashScreenOverride splashScreen);
        public abstract void UpdateProgress(RendererInitProgressUpdate update);
        public abstract void InitCompleted();
    }
}
