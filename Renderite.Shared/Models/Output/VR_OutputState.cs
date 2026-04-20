using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VR_OutputState : IMemoryPackable
    {
        public VR_ControllerOutputState leftController;
        public VR_ControllerOutputState rightController;

        public bool useViveHandTracking;

        public void Pack(ref MemoryPacker packer)
        {
            packer.WriteObject(leftController);
            packer.WriteObject(rightController);

            packer.Write(useViveHandTracking);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.ReadObject(ref leftController);
            unpacker.ReadObject(ref rightController);

            unpacker.Read(ref useViveHandTracking);
        }
    }
}
