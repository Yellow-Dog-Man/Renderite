using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 56)]
    public struct RenderTransformOverrideState : IRenderContextOverrideState
    {
        RenderingContext IRenderContextOverrideState.Context { get => context; set => context = value; }

        [FieldOffset(0)]
        public int renderableIndex;

        [FieldOffset(4)]
        public RenderVector3 positionOverride;

        [FieldOffset(20)]
        public RenderQuaternion rotationOverride;

        [FieldOffset(36)]
        public RenderVector3 scaleOverride;

        /// <summary>
        /// List of skinned mesh renderers that need to be flagged for recomputations as part of this update.
        /// The indexes are in a separate buffer.
        /// </summary>
        [FieldOffset(48)]
        public int skinnedMeshRendererCount;

        [FieldOffset(52)]
        public RenderingContext context;

        [FieldOffset(53)]
        public byte overrideFlags;

        public RenderVector3? PositionOverride
        {
            get
            {
                if ((overrideFlags & 0b001) == 0)
                    return null;

                return positionOverride;
            }

            set
            {
                if(value == null)
                    overrideFlags = (byte)(overrideFlags & ~0b001);
                else
                {
                    overrideFlags = (byte)(overrideFlags | 0b001);
                    positionOverride = value.Value;
                }
            }
        }

        public RenderQuaternion? RotationOverride
        {
            get
            {
                if ((overrideFlags & 0b010) == 0)
                    return null;

                return rotationOverride;
            }

            set
            {
                if (value == null)
                    overrideFlags = (byte)(overrideFlags & ~0b010);
                else
                {
                    overrideFlags = (byte)(overrideFlags | 0b010);
                    rotationOverride = value.Value;
                }
            }
        }

        public RenderVector3? ScaleOverride
        {
            get
            {
                if ((overrideFlags & 0b100) == 0)
                    return null;

                return scaleOverride;
            }

            set
            {
                if (value == null)
                    overrideFlags = (byte)(overrideFlags & ~0b100);
                else
                {
                    overrideFlags = (byte)(overrideFlags | 0b100);
                    scaleOverride = value.Value;
                }
            }
        }
    }
}
