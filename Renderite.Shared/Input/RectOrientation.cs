using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("Elements.Core.RectOrientation", "Elements.Core")]
    public enum RectOrientation
    {
        Default,
        Clockwise90,
        UpsideDown180,
        CounterClockwise90,
    }
}
