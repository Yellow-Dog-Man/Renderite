using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class DisplayInput : MonoBehaviour
    {
        public abstract IDisplayTextureSource TryGetDisplayTexture(int index);

        public void UpdateState(InputState state)
        {
            if (state.displays == null)
                state.displays = new List<DisplayState>();

            UpdateState(state.displays);
        }

        protected abstract void UpdateState(List<DisplayState> states);
    }
}
