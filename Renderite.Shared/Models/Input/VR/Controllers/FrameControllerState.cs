using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class FrameControllerState : VR_ControllerState
    {
        public bool bumperTouch;
        public bool bumperClick;

        public float trigger;
        public bool triggerTouch;
        public bool triggerClick;

        public float grip;
        public bool gripTouch;
        public bool gripClick;

        public RenderVector2 thumbstickRaw;
        public bool thumbstickTouch;
        public bool thumbstickClick;

        public bool viewMenuTouch;
        public bool viewMenuClick;

        public bool systemTouch;
        public bool systemClick;

        // Following values are shared between the DPad and the ABXY buttons
        // The positions of each button match where they are on the controllers
        // This allows symmetrical handling, without needing to split classes between left & right
        public bool down_A_Touch;
        public bool down_A_Click;

        public bool right_B_Touch;
        public bool right_B_Click;

        public bool left_X_Touch;
        public bool left_X_Click;

        public bool up_Y_Touch;
        public bool up_Y_Click;


        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(bumperTouch, bumperClick,
                triggerTouch, triggerClick,
                gripTouch, gripClick,
                thumbstickTouch, thumbstickClick);

            packer.Write(viewMenuTouch, viewMenuClick, systemTouch, systemClick);

            packer.Write(
                down_A_Touch, down_A_Click,
                right_B_Touch, right_B_Click,
                left_X_Touch, left_X_Click,
                up_Y_Touch, up_Y_Click);

            packer.Write(trigger);
            packer.Write(grip);
            packer.Write(thumbstickRaw);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(out bumperTouch, out bumperClick,
                out triggerTouch, out triggerClick,
                out gripTouch, out gripClick,
                out thumbstickTouch, out thumbstickClick);

            unpacker.Read(out viewMenuTouch, out viewMenuClick, out systemTouch, out systemClick);

            unpacker.Read( 
                out down_A_Touch, out down_A_Click,
                out right_B_Touch, out right_B_Click,
                out left_X_Touch, out left_X_Click,
                out up_Y_Touch, out up_Y_Click);

            unpacker.Read(ref trigger);
            unpacker.Read(ref grip);
            unpacker.Read(ref thumbstickRaw);
        }
    }
}
