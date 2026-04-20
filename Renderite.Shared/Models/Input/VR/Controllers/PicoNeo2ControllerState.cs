using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class PicoNeo2ControllerState : VR_ControllerState
    {
        public bool app;
        public bool pico;

        public bool buttonYB;
        public bool buttonXA;

        public bool gripClick;

        public bool joystickTouch;
        public bool joystickClick;
        public RenderVector2 joystick;

        public bool triggerClick;
        public float trigger;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(app, pico,
                buttonYB, buttonXA,
                gripClick,
                joystickTouch, joystickClick,
                triggerClick);

            packer.Write(joystick);
            packer.Write(trigger);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(out app, out pico,
                out buttonYB, out buttonXA,
                out gripClick,
                out joystickTouch, out joystickClick,
                out triggerClick);

            unpacker.Read(ref joystick);
            unpacker.Read(ref trigger);
        }
    }
}
