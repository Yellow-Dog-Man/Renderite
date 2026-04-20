using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [Flags]
    public enum TextureUpdateResultType
    {
        FormatSet       =   (1 << 0),
        PropertiesSet   =   (1 << 1),
        DataUpload      =   (1 << 2),
    }
}
