using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;

namespace Renderite.Unity
{
    public class TextureFormatData
    {
        public TextureType type;
        public int width;
        public int height;
        public int depth;
        public int mips;
        public TextureFormat format;
        public ColorProfile profile;

        public Action oldCleanup;

        public int ArraySize => type switch
        {
            TextureType.Texture2D => 1,
            TextureType.Cubemap => 6,
            TextureType.Texture3D => depth,
            _ => throw new Exception("Invalid texture type: " + type)
        };
    }
}
