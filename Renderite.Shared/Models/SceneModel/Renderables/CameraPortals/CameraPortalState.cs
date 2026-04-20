using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct CameraPortalState
    {
        /// <summary>
        /// Identifies which camera portal is this for
        /// </summary>
        [FieldOffset(0)]
        public int renderableIndex;

        /// <summary>
        /// The renderable index of the mesh renderer that this should be setup on.
        /// Note that the index can change once this update is processes, but that's ok - any new updates
        /// will fetch a new index.
        /// However it's important that mesh renderer updates happen BEFORE camera portals! 
        /// </summary>
        [FieldOffset(4)]
        public int meshRendererIndex;

        /// <summary>
        /// The plane normal of this camera portal
        /// </summary>
        [FieldOffset(8)]
        public RenderVector3 planeNormal;

        /// <summary>
        /// Offset from the plane for rendering
        /// </summary>
        [FieldOffset(20)]
        public float planeOffset;

        /// <summary>
        /// The asset ID of render texture used to render this camera portal
        /// </summary>
        [FieldOffset(24)]
        public int renderTextureId;

        /// <summary>
        /// When in portal mode, this represensets transform from the reference to render plane
        /// </summary>
        [FieldOffset(28)]
        public RenderMatrix4x4 portalTransform;

        /// <summary>
        /// Position of the portal plane
        /// </summary>
        [FieldOffset(92)]
        public RenderVector3 portalPlanePosition;

        /// <summary>
        /// The normal of the portal plane
        /// </summary>
        [FieldOffset(104)]
        public RenderVector3 portalPlaneNormal;

        /// <summary>
        /// Overriden far clip
        /// </summary>
        [FieldOffset(116)]
        float overrideFarClipValue;

        [FieldOffset(120)]
        CameraClearMode overrideClearFlagValue;

        [FieldOffset(124)]
        int flags;

        /// <summary>
        /// Overriden far clip
        /// </summary>
        public float? overrideFarClip
        {
            get => HasFarClipValue ? overrideFarClipValue : null;
            set
            {
                HasFarClipValue = value != null;
                overrideFarClipValue = value ?? default;
            }
        }

        /// <summary>
        /// Overriden clear flag
        /// </summary>
        public CameraClearMode? overrideClearFlag
        {
            get => HasCameraClearMode ? overrideClearFlagValue : null;
            set
            {
                HasCameraClearMode = value != null;
                overrideClearFlagValue = value ?? default;
            }
        }

        public bool HasFarClipValue
        {
            get => flags.HasFlag(0);
            set => flags.SetFlag(0, value);
        }

        public bool HasCameraClearMode
        {
            get => flags.HasFlag(1);
            set => flags.SetFlag(1, value);
        }

        /// <summary>
        /// Forces per-pixel lights to be disabled when rendering this camera portal
        /// </summary>
        public bool disablePerPixelLights
        {
            get => flags.HasFlag(2);
            set => flags.SetFlag(2, value);
        }

        /// <summary>
        /// Forces shadows to not be rendered
        /// </summary>
        public bool disableShadows
        {
            get => flags.HasFlag(3);
            set => flags.SetFlag(3, value);
        }

        /// <summary>
        /// Whether this is in portal mode or mirror mode
        /// </summary>
        public bool portalMode
        {
            get => flags.HasFlag(4);
            set => flags.SetFlag(4, value);
        }
    }
}
