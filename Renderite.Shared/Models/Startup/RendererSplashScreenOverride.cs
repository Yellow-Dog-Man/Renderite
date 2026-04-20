using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class RendererSplashScreenOverride : IMemoryPackable
    {
        /// <summary>
        /// Size of the texture for splash screen in pixels
        /// </summary>
        public RenderVector2i textureSize;

        /// <summary>
        /// Texture data in BGRA32 format
        /// </summary>
        public SharedMemoryBufferDescriptor<byte> textureData;

        /// <summary>
        /// Offset of the loading bar on the screen, in screen relative units
        /// </summary>
        public RenderVector2 loadingBarOffset;

        /// <summary>
        /// Relative size of the texture on the screen (from 0.0 to 1.0 covering the screen fully)
        /// </summary>
        public float textureRelativeScreenSize;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(textureSize);
            packer.Write(textureData);
            packer.Write(loadingBarOffset);
            packer.Write(textureRelativeScreenSize);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref textureSize);
            unpacker.Read(ref textureData);
            unpacker.Read(ref loadingBarOffset);
            unpacker.Read(ref textureRelativeScreenSize);
        }
    }
}
