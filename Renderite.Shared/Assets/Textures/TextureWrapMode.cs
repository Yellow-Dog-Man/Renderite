using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.TextureWrapMode", "FrooxEngine")]
    public enum TextureWrapMode
    {
        Repeat,
        Clamp,
        Mirror,
        MirrorOnce
    }
}
