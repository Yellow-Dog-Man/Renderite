using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class WindowInput : MonoBehaviour
    {
        public bool IsFocused { get; private set; }

        bool _resolutionChanged;

        public void FlagResolutionChanged()
        {
            _resolutionChanged = true;
        }

        public void UpdateState(InputState state)
        {
            if (state.window == null)
                state.window = new WindowState();

            UpdateState(state.window);
        }

        public virtual void UpdateState(WindowState state)
        {
            state.windowResolution = new RenderVector2i(UnityEngine.Screen.width, UnityEngine.Screen.height);
            state.isFullscreen = UnityEngine.Screen.fullScreen;

            // We have to use native method for Windows, because the Application.isFocused is very unreliable
            // There is apparently a bug in Unity, where if vSyncCount is set to 0 after the focus has changed
            // Unity will just think it's constantly focused. This breaks the frame limiting when in background
            // because we need to turn off vSync for that - but that also makes it think it's focused again.
            // This is pretty bad and it makes me sad and hate Unity even more, but this works for now ;_;
            IsFocused = WindowsNativeHelper.ApplicationIsActivated();

            state.isWindowFocused = IsFocused;

            state.resolutionSettingsApplied = _resolutionChanged;
            _resolutionChanged = false;
        }
    }
}
