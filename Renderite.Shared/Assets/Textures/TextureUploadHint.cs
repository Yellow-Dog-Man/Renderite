using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct TextureUploadHint
    {
        public bool IsEmptyRegion
        {
            get
            {
                if (!hasRegion)
                    return false;

                return regionData.width == 0 || regionData.height == 0;
            }
        }

        public RenderIntRect? region
        {
            get => hasRegion ? regionData : null;
            set
            {
                if(value == null)
                {
                    hasRegion = false;
                    regionData = default;
                }
                else
                {
                    hasRegion = true;
                    regionData = value.Value;
                }
            }
        }

        // Region

        [FieldOffset(0)]
        public RenderIntRect regionData;

        // Whether it should be readable or not
        [FieldOffset(16)]
        public bool readable;

        [FieldOffset(17)]
        public bool hasRegion;
    }
}
