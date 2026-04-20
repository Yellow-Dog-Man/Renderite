using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    public class InputState : IMemoryPackable
    {
        public MouseState mouse = new MouseState();
        public KeyboardState keyboard = new KeyboardState();
        public WindowState window = new WindowState();
        public VR_InputsState vr = new VR_InputsState();

        public List<GamepadState> gamepads;
        public List<TouchState> touches;
        public List<DisplayState> displays;

        public void Pack(ref MemoryPacker packer)
        {
            packer.WriteObject(mouse);
            packer.WriteObject(keyboard);
            packer.WriteObject(window);
            packer.WriteObject(vr);

            packer.WriteObjectList(gamepads);
            packer.WriteObjectList(touches);
            packer.WriteObjectList(displays);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.ReadObject(ref mouse);
            packer.ReadObject(ref keyboard);
            packer.ReadObject(ref window);
            packer.ReadObject(ref vr);

            packer.ReadObjectList(ref gamepads);
            packer.ReadObjectList(ref touches);
            packer.ReadObjectList(ref displays);
        }
    }
}
