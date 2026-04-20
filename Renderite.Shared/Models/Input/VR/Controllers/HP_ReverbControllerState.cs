using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class HP_ReverbControllerState : VR_ControllerState
    {
        public bool appMenu;

        public bool buttonYB;
        public bool buttonXA;

        public bool gripTouch;
        public bool gripClick;
        public float grip;

        public bool joystickClick;
        public RenderVector2 joystickRaw;

        public bool triggerHair;
        public bool triggerClick;
        public float trigger;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(appMenu,
                buttonYB, buttonXA,
                gripTouch, gripClick,
                joystickClick,
                triggerHair, triggerClick);

            packer.Write(grip);
            packer.Write(joystickRaw);
            packer.Write(trigger);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(out appMenu,
                out buttonYB, out buttonXA,
                out gripTouch, out gripClick,
                out joystickClick,
                out triggerHair, out triggerClick);

            unpacker.Read(ref grip);
            unpacker.Read(ref joystickRaw);
            unpacker.Read(ref trigger);
        }
    }
}
