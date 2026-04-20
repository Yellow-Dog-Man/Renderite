using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct TransformParentUpdate
    {
        [FieldOffset(0)]
        public int transformId;

        [FieldOffset(4)]
        public int newParentId;
    }
}
