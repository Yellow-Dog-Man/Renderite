using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("Elements.Assets.TextureFilterMode", "Elements.Assets")]
    [OldTypeName("FrooxEngine.TextureFilterMode", "FrooxEngine")]
    public enum TextureFilterMode
    {
        Point,
        Bilinear,
        Trilinear,
        Anisotropic,
    }
}
