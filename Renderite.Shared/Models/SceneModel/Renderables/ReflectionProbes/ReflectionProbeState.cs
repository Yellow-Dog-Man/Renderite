using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 68)]
    public struct ReflectionProbeState
    {
        /// <summary>
        /// Identifies the light this state is for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// The importance of the probe over others
        /// </summary>
        [FieldOffset(4)]
        public int importance;

        /// <summary>
        /// Intensity of the reflection probe visuals. 
        /// </summary>
        [FieldOffset(8)]
        public float intensity;

        /// <summary>
        /// The distance over this probe blends with other nearby probes
        /// </summary>
        [FieldOffset(12)]
        public float blendDistance;

        /// <summary>
        /// The size of the area the reflection probe affects
        /// </summary>
        [FieldOffset(16)]
        public RenderVector3 boxSize;

        /// <summary>
        /// In baked mode this is the asset ID of the cubemap texture used for the probe
        /// </summary>
        [FieldOffset(28)]
        public int cubemapAssetId;

        /// <summary>
        /// Resolution used in realtime mode
        /// </summary>
        [FieldOffset(32)]
        public int resolution;

        /// <summary>
        /// How far should shadows be rendered in realtime mode
        /// </summary>
        [FieldOffset(36)]
        public float shadowDistance;

        /// <summary>
        /// When the background is cleared with solid color, this is the color to be used
        /// </summary>
        [FieldOffset(40)]
        public RenderVector4 backgroundColor;

        /// <summary>
        /// Near clipping used for rendering
        /// </summary>
        [FieldOffset(56)]
        public float nearClip;

        /// <summary>
        /// Far clipping used for rendering
        /// </summary>
        [FieldOffset(60)]
        public float farClip;

        /// <summary>
        /// The type of the reflection probe
        /// </summary>
        [FieldOffset(64)]
        public ReflectionProbeType type;

        /// <summary>
        /// How is the background cleared when rendering
        /// </summary>
        [FieldOffset(65)]
        public ReflectionProbeClear clearFlags;

        /// <summary>
        /// In realtime mode this is the time slicing mode used
        /// </summary>
        [FieldOffset(66)]
        public ReflectionProbeTimeSlicingMode timeSlicingMode;

        [FieldOffset(67)]
        byte flags;

        /// <summary>
        /// When on, this reflection probe will only render the skybox and no objects around
        /// </summary>
        public bool skyboxOnly
        {
            get => flags.HasFlag(0);
            set => flags.SetFlag(0, value);
        }

        /// <summary>
        /// Should the probe use HDR for rendering?
        /// </summary>
        public bool HDR
        {
            get => flags.HasFlag(1);
            set => flags.SetFlag(1, value);
        }

        /// <summary>
        /// Whether to use box projection which will make the cubemap match the size of the probe for better parallax
        /// </summary>
        public bool useBoxProjection
        {
            get => flags.HasFlag(2);
            set => flags.SetFlag(2, value);
        }
    }
}
