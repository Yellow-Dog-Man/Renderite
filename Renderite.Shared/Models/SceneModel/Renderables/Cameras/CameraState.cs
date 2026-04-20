using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 72)]
    public struct CameraState
    {
        /// <summary>
        /// Identifies which camera is this for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// FOV
        /// </summary>
        [FieldOffset(4)]
        public float fieldOfView;

        /// <summary>
        /// Rendering size in the ortographic mode
        /// </summary>
        [FieldOffset (8)]
        public float orthographicSize;

        /// <summary>
        /// Near clippping plane
        /// </summary>
        [FieldOffset(12)]
        public float nearClip;

        /// <summary>
        /// Far clipping plane
        /// </summary>
        [FieldOffset(16)]
        public float farClip;

        /// <summary>
        /// The color used to clear the texture when solid color is used
        /// </summary>
        [FieldOffset(20)]
        public RenderVector4 backgroundColor;

        /// <summary>
        /// The viewport rect used for rendering
        /// </summary>
        [FieldOffset(36)]
        public RenderRect viewport;

        /// <summary>
        /// The depth of the camera used to sort it with any other cameras rendering to given target
        /// </summary>
        [FieldOffset(52)]
        public float depth;

        /// <summary>
        /// The render texture that this camera renders to. This is NOT a packed texture asset ID, but just the raw
        /// render texture ID, because that's the only valid texture type target for this
        /// </summary>
        [FieldOffset(56)]
        public int renderTextureAssetId;

        /// <summary>
        /// How many transforms are being selectively rendered. The actual ID's are provided in the ID's buffer
        /// </summary>
        [FieldOffset(60)]
        public int selectiveRenderCount;

        /// <summary>
        /// How many transforms are being excluded by this camera. The actual ID's are provided in the ID's buffer
        /// </summary>
        [FieldOffset(64)]
        public int excludeRenderCount;

        /// <summary>
        /// How does the camera clear the texture before each render
        /// </summary>
        [FieldOffset(68)]
        public CameraClearMode clearMode;

        /// <summary>
        /// Projection that this camera uses
        /// </summary>
        [FieldOffset(69)] // NICE (this wasn't planned by the way, it just worked out that way)
        public CameraProjection projection;

        [FieldOffset(70)]
        ushort flags;

        /// <summary>
        /// Is the camera enabled and rendering?
        /// </summary>
        public bool enabled
        {
            get => flags.HasFlag(0);
            set => flags.SetFlag(0, value);
        }

        /// <summary>
        /// Should the transform size affect the camera ortographic projection?
        /// </summary>
        public bool useTransformScale
        {
            get => flags.HasFlag(1);
            set => flags.SetFlag(1, value);
        }

        /// <summary>
        /// Should the camera render double-buffered? This will let it see its own render output
        /// </summary>
        public bool doubleBuffered
        {
            get => flags.HasFlag(2);
            set => flags.SetFlag(2, value);
        }

        /// <summary>
        /// Whether this camera is allowed to render private UI layer or not
        /// </summary>
        public bool renderPrivateUI
        {
            get => flags.HasFlag(3);
            set => flags.SetFlag(3, value);
        }

        /// <summary>
        /// Indicates if this camera should only use forward rendering path
        /// </summary>
        public bool forwardOnly
        {
            get => flags.HasFlag(4);
            set => flags.SetFlag(4, value);
        }

        /// <summary>
        /// Indicates if it should render shadows
        /// </summary>
        public bool renderShadows
        {
            get => flags.HasFlag(5);
            set => flags.SetFlag(5, value);
        }

        /// <summary>
        /// Should the camera use postprocessing effects?
        /// </summary>
        public bool postprocessing
        {
            get => flags.HasFlag(6);
            set => flags.SetFlag(6, value);
        }

        /// <summary>
        /// Should screen-space reflections be used?
        /// </summary>
        public bool screenSpaceReflections
        {
            get => flags.HasFlag(7);
            set => flags.SetFlag(7, value);
        }

        /// <summary>
        /// Should motion blur be used?
        /// </summary>
        public bool motionBlur
        {
            get => flags.HasFlag(8);
            set => flags.SetFlag(8, value);
        }
    }
}
