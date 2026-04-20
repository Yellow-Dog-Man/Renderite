using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public abstract class RenderContextOverridesUpdate<TState> : RenderablesStateUpdate<TState>
        where TState : unmanaged, IRenderContextOverrideState
    {

    }
}
