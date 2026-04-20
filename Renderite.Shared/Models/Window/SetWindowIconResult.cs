using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Response set for the SetWindowIcon request
    /// </summary>
    public class SetWindowIconResult : RendererCommand
    {
        /// <summary>
        /// Unique ID of the request this was for
        /// </summary>
        public int requestId;

        /// <summary>
        /// Indicates if the icon set was success
        /// </summary>
        public bool success;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(requestId);
            packer.Write(success);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref requestId);
            unpacker.Read(ref success);
        }
    }
}
