using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct BlitToDisplayState
    {
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// Packed texture ID of the texture that should be blitted to the display
        /// </summary>
        [FieldOffset(4)]
        public int textureId;

        [FieldOffset(8)]
        public RenderVector4 backgroundColor;

        [FieldOffset(24)]
        public short displayIndex;

        [FieldOffset(26)]
        byte _flags;

        public bool flipHorizontally
        {
            get => _flags.HasFlag(0);
            set => _flags.SetFlag(0, value);
        }

        public bool flipVertically
        {
            get => _flags.HasFlag(1);
            set => _flags.SetFlag(1, value);
        }
    }
}
