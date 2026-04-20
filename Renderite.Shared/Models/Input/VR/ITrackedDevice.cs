using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public interface ITrackedDevice
    {
        public bool IsTracking { get; set; }
        public RenderVector3 Position { get; set; }
        public RenderQuaternion Rotation { get; set; }
    }
}
