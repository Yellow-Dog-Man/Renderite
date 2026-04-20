using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct HapticPointState
    {
        [FieldOffset(0)]
        public float force;

        [FieldOffset(4)]
        public float temperature;

        [FieldOffset(8)]
        public float pain;

        [FieldOffset(12)]
        public float vibration;
    }
}
