using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VR_ControllerOutputState : IMemoryPackable
    {
        /// <summary>
        /// Which controller is this vibration data for
        /// </summary>
        public Chirality side;

        /// <summary>
        /// When this is > 0 it will trigger explicit vibration. Otherwise this should be 0
        /// </summary>
        public double vibrateTime;

        /// <summary>
        /// The current haptic point state. This represents continuous haptics.
        /// </summary>
        public HapticPointState hapticState;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(side);

            packer.Write(vibrateTime);
            packer.Write(hapticState);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref side);

            unpacker.Read(ref vibrateTime);
            unpacker.Read(ref hapticState);
        }
    }
}
