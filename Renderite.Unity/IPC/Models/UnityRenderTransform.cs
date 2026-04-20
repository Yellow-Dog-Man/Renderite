using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public struct UnityRenderTransform
    {
        [FieldOffset(0)]
        public Vector3 position;

        [FieldOffset(12)]
        public Vector3 scale;

        [FieldOffset(24)]
        public Quaternion rotation;

        public override string ToString() => $"Position: {position}, Rotation: {rotation}, Scale: {scale}";
    }
}
