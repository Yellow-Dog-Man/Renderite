using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class SetTaskbarProgress : RendererCommand
    {
        public TaskbarProgressBarMode mode;

        public ulong completed;
        public ulong total;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(mode);

            packer.Write(completed);
            packer.Write(total);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref mode);

            unpacker.Read(ref completed);
            unpacker.Read(ref total);
        }
    }
}
