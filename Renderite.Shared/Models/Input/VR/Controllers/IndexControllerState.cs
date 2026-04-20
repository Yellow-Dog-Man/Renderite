using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class IndexControllerState : VR_ControllerState
    {
        public float grip;
        public bool gripTouch;
        public bool gripClick;

        public bool buttonA;
        public bool buttonB;

        public bool buttonAtouch;
        public bool buttonBtouch;

        public float trigger;
        public bool triggerTouch;
        public bool triggerClick;

        public RenderVector2 joystickRaw;
        public bool joystickTouch;
        public bool joystickClick;

        public RenderVector2 touchpad;
        public bool touchpadTouch;
        public bool touchpadPress;
        public float touchpadForce;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(gripTouch, gripClick,
                buttonA, buttonB, buttonAtouch, buttonBtouch,
                triggerTouch, triggerClick);

            packer.Write(grip);
            packer.Write(trigger);

            packer.Write(joystickTouch, joystickClick,
                touchpadTouch, touchpadPress);

            packer.Write(joystickRaw);

            packer.Write(touchpad);
            packer.Write(touchpadForce);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(out gripTouch, out gripClick,
                out buttonA, out buttonB, out buttonAtouch, out buttonBtouch,
                out triggerTouch, out triggerClick);

            unpacker.Read(ref grip);
            unpacker.Read(ref trigger);

            unpacker.Read(out joystickTouch, out joystickClick,
                out touchpadTouch, out touchpadPress);

            unpacker.Read(ref joystickRaw);

            unpacker.Read(ref touchpad);
            unpacker.Read(ref touchpadForce);
        }
    }
}
