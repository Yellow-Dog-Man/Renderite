using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    // Important!!! The layout of this must stay in sync with Renderite.Shared.LightData
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct UnityLightData
    {
        [FieldOffset(0)]
        public Vector3 point;

        [FieldOffset(12)]
        public Quaternion orientation;

        [FieldOffset(28)]
        public Vector3 color;

        [FieldOffset(40)]
        public float intensity;

        [FieldOffset(44)]
        public float range;

        [FieldOffset(48)]
        public float angle;
    }
}
