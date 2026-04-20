using System;
using System.Collections.Generic;
using System.Text;
using Elements.Data;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.CameraProjection", "FrooxEngine")]
    [OldTypeName("ElementsCore.CameraProjection", "Elements.Core")]
    public enum CameraProjection : byte
    {
        Perspective,
        Orthographic,
        Panoramic
    }
}
