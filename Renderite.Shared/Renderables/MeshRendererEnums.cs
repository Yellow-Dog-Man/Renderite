using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.ShadowCastMode", "FrooxEngine")]
    public enum ShadowCastMode : byte
    {
        Off,
        On,
        ShadowOnly,
        DoubleSided
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.MotionVectorMode", "FrooxEngine")]
    public enum MotionVectorMode : byte
    {
        Camera,
        Object,
        NoMotion
    }

    public enum MeshRendererType : byte
    {
        MeshRenderer,
        SkinnedMeshRenderer
    }
}
