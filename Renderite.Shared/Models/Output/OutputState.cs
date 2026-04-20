using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    public class OutputState : IMemoryPackable
    {
        public bool lockCursor;
        
        public RenderVector2i? lockCursorPosition;
        
        public bool keyboardInputActive;

        public VR_OutputState vr;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(lockCursor);
            packer.Write(lockCursorPosition);
            packer.Write(keyboardInputActive);

            packer.WriteObject(vr);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref lockCursor);
            packer.Read(ref lockCursorPosition);
            packer.Read(ref keyboardInputActive);

            packer.ReadObject(ref vr);
        }
    }
}
