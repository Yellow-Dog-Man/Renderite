using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Unity
{
    [StructLayout(LayoutKind.Explicit, Size = 44)]
    public struct UnityTransformPoseUpdate
    {
        [FieldOffset(0)]
        public int transformId;

        [FieldOffset(4)]
        public UnityRenderTransform pose;
    }
}
