using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.RenderingContext", "FrooxEngine")]
    public enum RenderingContext : byte
    {
        UserView,
        ExternalView,
        Camera,
        Mirror,
        Portal,
        RenderToAsset,
    }
}
