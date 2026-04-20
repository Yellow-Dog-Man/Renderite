using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public interface IRenderContextOverrideState
    {
        public RenderingContext Context { get; set; }
    }
}
