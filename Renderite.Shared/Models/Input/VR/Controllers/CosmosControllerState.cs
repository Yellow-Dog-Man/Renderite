using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class CosmosControllerState : VR_ControllerState
    {
        public bool joystickTouch;
        public bool joystickClick;
        public RenderVector2 joystickRaw;

        public bool triggerTouch;
        public bool triggerClick;
        public float trigger;

        public bool gripClick;

        public bool vive;

        public bool buttonAX;
        public bool buttonBY;

        public bool bumper;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(joystickTouch, joystickClick,
                triggerTouch, triggerClick,
                gripClick,
                vive,
                buttonAX, buttonBY);

            packer.Write(joystickRaw);
            packer.Write(trigger);

            // Sadness. Why is there 9 bools? ;_; My OCD hates this.
            packer.Write(bumper);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(out joystickTouch, out joystickClick,
                out triggerTouch, out triggerClick,
                out gripClick,
                out vive,
                out buttonAX, out buttonBY);

            unpacker.Read(ref joystickRaw);
            unpacker.Read(ref trigger);

            unpacker.Read(ref bumper);
        }
    }
}
