using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.AntiAliasingMethod", "FrooxEngine")]
    public enum AntiAliasingMethod
    {
        Off,
        FXAA,
        SMAA,
        TAA
    }
}
