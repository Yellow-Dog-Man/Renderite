using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.BillboardRenderBufferRenderer+BillboardAlignment", "FrooxEngine")]
    public enum BillboardAlignment : byte
    {
        View,
        Facing,
        Local,
        Global,
        Direction
    }
}
