using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 36)]
    public struct LightsBufferRendererState
    {
        /// <summary>
        /// The index of the renderable this is for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// The global unique ID of this lights buffer
        /// </summary>
        [FieldOffset(4)]
        public int globalUniqueId;

        /// <summary>
        /// Strength of shadows if on
        /// </summary>
        [FieldOffset(8)]
        public float shadowStrength;

        /// <summary>
        /// The near plane for rendering shadows
        /// </summary>
        [FieldOffset(12)]
        public float shadowNearPlane;

        /// <summary>
        /// Override for shadow map resolution if above 0
        /// </summary>
        [FieldOffset(16)]
        public int shadowMapResolution;

        /// <summary>
        /// Bias for rendering shadows
        /// </summary>
        [FieldOffset(20)]
        public float shadowBias;

        /// <summary>
        /// Bias for normals when rendering shadows
        /// </summary>
        [FieldOffset(24)]
        public float shadowNormalBias;

        /// <summary>
        /// Packed AssetID for a cookie texture
        /// </summary>
        [FieldOffset(28)]
        public int cookieTextureAssetId;

        /// <summary>
        /// The type of the light
        /// </summary>
        [FieldOffset(32)]
        public LightType lightType;

        /// <summary>
        /// Type of shadow (if any)
        /// </summary>
        [FieldOffset(33)]
        public ShadowType shadowType;
    }
}
