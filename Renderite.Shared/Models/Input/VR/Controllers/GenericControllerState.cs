using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class GenericControllerState : VR_ControllerState
    {
        public float strength;
        public RenderVector2 axis;

        public bool touchingStrength;
        public bool touchingAxis;

        public bool primary;
        public bool menu;
        public bool grab;
        public bool secondary;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(strength);
            packer.Write(axis);

            packer.Write(touchingStrength, touchingAxis,
                primary, menu, grab, secondary);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(ref strength);
            unpacker.Read(ref axis);

            unpacker.Read(out touchingStrength, out touchingAxis,
                out primary, out menu, out grab, out secondary);
        }
    }
}
