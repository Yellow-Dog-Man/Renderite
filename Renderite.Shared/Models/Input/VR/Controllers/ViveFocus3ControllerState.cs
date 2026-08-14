namespace Renderite.Shared
{
    public class ViveFocus3ControllerState : VR_ControllerState
    {
        public RenderVector2 joystickRaw;
        public bool joystickTouch;
        public bool joystickClick;

        public float trigger;
        public bool triggerTouch;
        public bool triggerClick;

        public float grip;
        public bool gripTouch;
        public bool gripClick;

        public bool buttonXA;
        public bool buttonYB;

        public bool menu;
        public bool thumbrestTouch;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(joystickRaw);
            packer.Write(trigger);
            packer.Write(grip);

            packer.Write(joystickTouch, joystickClick,
                triggerTouch, triggerClick,
                gripTouch, gripClick,
                buttonXA, buttonYB);

            packer.Write(menu, thumbrestTouch);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(ref joystickRaw);
            unpacker.Read(ref trigger);
            unpacker.Read(ref grip);

            unpacker.Read(out joystickTouch, out joystickClick,
                out triggerTouch, out triggerClick,
                out gripTouch, out gripClick,
                out buttonXA, out buttonYB);

            unpacker.Read(out menu, out thumbrestTouch);
        }
    }
}
