using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public struct Texture3DUploadHint
    {
        [FieldOffset(0)]
        public bool readable;

        public Texture3DUploadHint(bool readable)
        {
            this.readable = readable;
        }
    }
}
