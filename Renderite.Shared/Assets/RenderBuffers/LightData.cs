using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    // Important!!! The layout of this must stay in sync with Elements.Assets.LightData
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct LightData
    {
        [FieldOffset(0)]
        public RenderVector3 point;

        [FieldOffset(12)]
        public RenderQuaternion orientation;

        [FieldOffset(28)]
        public RenderVector3 color;

        [FieldOffset(40)]
        public float intensity;

        [FieldOffset(44)]
        public float range;

        [FieldOffset(48)]
        public float angle;
    }
}
