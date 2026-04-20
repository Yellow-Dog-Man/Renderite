
using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.LightTyle", "FrooxEngine")]
    public enum LightType : byte
    {
        Point,
        Directional,
        Spot,
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.ShadowType", "FrooxEngine")]
    public enum ShadowType : byte
    {
        None,
        Hard,
        Soft,
    }
}
