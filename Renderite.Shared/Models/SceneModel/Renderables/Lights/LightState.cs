using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 60)]
    public struct LightState
    {
        /// <summary>
        /// Identifies the light this state is for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// Intensity of the light
        /// </summary>
        [FieldOffset(4)]
        public float intensity;

        /// <summary>
        /// The range of this light for point and spot lights
        /// </summary>
        [FieldOffset(8)]
        public float range;

        /// <summary>
        /// Angle of spot lights
        /// </summary>
        [FieldOffset(12)]
        public float spotAngle;

        /// <summary>
        /// Color of the light
        /// </summary>
        [FieldOffset(16)]
        public RenderVector4 color;

        /// <summary>
        /// When enabled, indicates the strength of the shadows
        /// </summary>
        [FieldOffset(32)]
        public float shadowStrength;

        /// <summary>
        /// Near plane for the shadow rendering
        /// </summary>
        [FieldOffset(36)]
        public float shadowNearPlane;

        /// <summary>
        /// Overrides the shadow map resolution. If 0, it's automatic
        /// </summary>
        [FieldOffset(40)]
        public int shadowMapResolutionOverride;

        /// <summary>
        /// Bias for shadows
        /// </summary>
        [FieldOffset(44)]
        public float shadowBias;

        /// <summary>
        /// Bias for shadow normals
        /// </summary>
        [FieldOffset(48)]
        public float shadowNormalBias;

        /// <summary>
        /// The AssetID of a cookie texture when active
        /// </summary>
        [FieldOffset(52)]
        public int cookieTextureAssetId;

        /// <summary>
        /// The type of light
        /// </summary>
        [FieldOffset(56)]
        public LightType type;

        /// <summary>
        /// The type of shadows
        /// </summary>
        [FieldOffset(57)]
        public ShadowType shadowType;
    }
}
