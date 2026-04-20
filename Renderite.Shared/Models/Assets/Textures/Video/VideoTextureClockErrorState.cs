using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit)]
    public struct VideoTextureClockErrorState
    {
        [FieldOffset(0)]
        public int assetId;

        [FieldOffset(4)]
        public float currentClockError;
    }
}
