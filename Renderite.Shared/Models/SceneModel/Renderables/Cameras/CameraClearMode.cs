using System;
using System.Collections.Generic;
using System.Text;
using Elements.Data;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.CameraClearMode", "FrooxEngine")]
    public enum CameraClearMode : byte
    {
        Skybox,
        Color,
        Depth,
        Nothing
    }
}
