using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.ParticleTrailTextureMode", "FrooxEngine")]
    [OldTypeName("FrooxEngine.TrailTextureMode", "FrooxEngine")]
    public enum TrailTextureMode : byte
    {
        Stretch = 0,
        Tile = 1,
        DistributePerSegment = 2,
        RepeatPerSegment = 3
    }
}
