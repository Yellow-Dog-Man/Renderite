using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Set's the window's icon
    /// </summary>
    public class SetWindowIcon : RendererCommand
    {
        /// <summary>
        /// Unique ID of this set window icon request. This is used to indicate to 
        /// </summary>
        public int requestId;

        /// <summary>
        /// Is this icon an overlay?
        /// </summary>
        public bool isOverlay;

        /// <summary>
        /// Size of the icon. Generally should be power of two for this to work
        /// </summary>
        public RenderVector2i size;

        /// <summary>
        /// The raw BGRA data of the icon data
        /// </summary>
        public SharedMemoryBufferDescriptor<byte> iconData;

        /// <summary>
        /// Description for overlay icons
        /// </summary>
        public string overlayDescription;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(requestId);
            packer.Write(isOverlay);
            packer.Write(size);
            packer.Write(iconData);

            if (isOverlay)
                packer.Write(overlayDescription);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref requestId);
            unpacker.Read(ref isOverlay);
            unpacker.Read(ref size);
            unpacker.Read(ref iconData);

            if (isOverlay)
                unpacker.Read(ref overlayDescription);
        }
    }
}
