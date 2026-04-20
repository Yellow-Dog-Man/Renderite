using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct ReflectionProbeChangeRenderResult
    {
        /// <summary>
        /// The ID of the render space this probe belongs to
        /// </summary>
        [FieldOffset(0)]
        public int renderSpaceId;

        /// <summary>
        ///  Unique ID of the probe
        ///  IMPORTANT: We cannot use the renderable index, because that can change between runs
        /// </summary>
        [FieldOffset(4)]
        public int renderProbeUniqueId;

        /// <summary>
        /// Indicates if the renderer requires the probe to be reset & resubmitted
        /// NOTE: This is mostly to deal with a bug in Unity where reflection probes disabled mid-render just
        /// die and stop responding altogether.
        /// </summary>
        [FieldOffset(8)]
        public bool requireReset;
    }
}
