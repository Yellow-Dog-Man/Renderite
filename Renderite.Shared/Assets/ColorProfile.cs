using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("Elements.Core.ColorProfile", "Elements.Core")]
    public enum ColorProfile
    {
        /// <summary>
        /// Typically used for attributes like smoothness, and alpha clip.
        /// </summary>
		Linear = 0,
        /// <summary>
        /// Most assets are authored in sRGB.  Used for albedo, specular color, emission, etc.
        /// </summary>
		sRGB,
        /// <summary>
        /// Only used for legacy alpha blending.
        /// </summary>
        //[Obsolete]
        sRGBAlpha
    }
}
