using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class ViveControllerState : VR_ControllerState
    {
        public bool grip;
        public bool app;

        public bool triggerHair;
        public bool triggerClick;
        public float trigger;

        public bool touchpadTouch;
        public bool touchpadClick;
        public RenderVector2 touchpad;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(grip, app, triggerHair, triggerClick, touchpadTouch, touchpadClick);

            packer.Write(trigger);
            packer.Write(touchpad);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(out grip, out app, out triggerHair, out triggerClick, out touchpadTouch, out touchpadClick);

            unpacker.Read(ref trigger);
            unpacker.Read(ref touchpad);
        }
    }
}
