using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class MouseInput : MonoBehaviour
    {
        public void UpdateState(InputState state)
        {
            if (state.mouse == null)
                state.mouse = new MouseState();

            UpdateState(state.mouse);
        }

        public abstract void HandleStateUpdate(OutputState state);

        protected abstract void UpdateState(MouseState state);
    }
}
