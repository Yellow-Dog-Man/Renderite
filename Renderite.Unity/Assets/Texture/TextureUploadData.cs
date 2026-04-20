using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;

namespace Renderite.Unity
{
    public class TextureUploadData
    {
        public TextureType type;
        public SharedMemoryViewSlice<byte> data;
        public int startMip;
        public TextureUploadHint hint2D;
        public Texture3DUploadHint hint3D;

        public List<RenderVector2i> mipMapSizes;
        public List<List<int>> mipStarts;

        public bool flipY;

        public int MipMapCount => mipMapSizes.Count;

        public RenderVector2i FaceSize => mipMapSizes[0];

        public RenderVector2i MipMapSize(int mip) => mipMapSizes[mip];

        public int PixelStart(int x, int y, int mip, int face)
        {
            var mipList = mipStarts[face];
            var mipStart = mipList[mip];
            var mipSize = mipMapSizes[mip];

            if (flipY)
                y = mipSize.y - y - 1;

            return mipStart + (x + y * mipSize.x);
        }
    }
}
