using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 44)]
    public struct TransformPoseUpdate
    {
        [FieldOffset(0)]
        public int transformId;

        [FieldOffset(4)]
        public RenderTransform pose;
    }
}
