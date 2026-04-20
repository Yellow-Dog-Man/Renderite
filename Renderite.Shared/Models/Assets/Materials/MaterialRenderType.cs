using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Unity Material Render Type.
    /// IMPORTANT!!! Do not rename the enum members! They are converted to string and need to match
    /// Unity side render tags. If you need to rename them, this will need to be accounted for.
    /// </summary>
    public enum MaterialRenderType
    {
        Opaque,
        TransparentCutout,
        Transparent
    }
}
