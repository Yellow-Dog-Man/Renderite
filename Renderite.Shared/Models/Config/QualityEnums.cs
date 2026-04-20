using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.ShadowCascadeMode", "FrooxEngine")]
    public enum ShadowCascadeMode
    {
        None,
        TwoCascades,
        FourCascades,
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.ShadowResolutionMode", "FrooxEngine")]
    public enum ShadowResolutionMode
    {
        Low,
        Medium,
        High,
        Ultra
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.SkinWeightMode", "FrooxEngine")]
    public enum SkinWeightMode
    {
        OneBone,
        TwoBones,
        FourBones,
        Unlimited
    }
}
