using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class KeyboardInput : MonoBehaviour
    {
        public void UpdateState(InputState state)
        {
            if (state.keyboard == null)
                state.keyboard = new KeyboardState();

            UpdateState(state.keyboard);
        }

        protected abstract void UpdateState(KeyboardState state);

        public abstract void HandleOutputState(OutputState output);
    }
}
