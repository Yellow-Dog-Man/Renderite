using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.MeshRenderBufferRenderer+MeshAlignment", "FrooxEngine")]
    public enum MeshAlignment : byte
    {
        View,
        Facing,
        Local,
        Global
    }
}
