using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.TouchController+Model", "FrooxEngine")]
    public enum TouchControllerModel : sbyte
    {
        CV1,
        QuestAndRiftS
    }

    public class TouchControllerState : VR_ControllerState
    {
        public TouchControllerModel model;

        public bool start;

        public bool buttonYB;
        public bool buttonXA;

        public bool buttonYB_touch;
        public bool buttonXA_touch;

        public bool thumbrestTouch;

        public float grip;
        public bool gripClick;

        public RenderVector2 joystickRaw;

        public bool joystickTouch;
        public bool joystickClick;

        public float trigger;
        public bool triggerTouch;
        public bool triggerClick;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(model);

            packer.Write(start,
                buttonYB, buttonXA, buttonYB_touch, buttonXA_touch,
                thumbrestTouch);

            packer.Write(gripClick,
                joystickTouch, joystickClick,
                triggerTouch, triggerClick);

            packer.Write(grip);
            packer.Write(joystickRaw);
            packer.Write(trigger);
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            base.Unpack(ref unpacker);

            unpacker.Read(ref model);

            unpacker.Read(out start,
                out buttonYB, out buttonXA, out buttonYB_touch, out buttonXA_touch,
                out  thumbrestTouch);

            unpacker.Read(out gripClick,
                out joystickTouch, out joystickClick,
                out triggerTouch, out triggerClick);

            unpacker.Read(ref grip);
            unpacker.Read(ref joystickRaw);
            unpacker.Read(ref trigger);
        }
    }
}
