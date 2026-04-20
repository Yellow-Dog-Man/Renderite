using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct RenderMaterialOverrideState : IRenderContextOverrideState
    {
        RenderingContext IRenderContextOverrideState.Context { get => context; set => context = value; }

        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// Index of the mesh renderer that this is for packed with the type
        /// </summary>
        [FieldOffset(4)]
        public int packedMeshRendererIndex;

        /// <summary>
        /// How many materials are being overriden by this particular override. The list of overrides is part of another buffer
        /// since the number of materials can be variable
        /// </summary>
        [FieldOffset(8)]
        public short materrialOverrideCount;

        /// <summary>
        /// The rendering context this targets
        /// </summary>
        [FieldOffset(10)]
        public RenderingContext context;
    }
}
