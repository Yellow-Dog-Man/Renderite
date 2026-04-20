using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MouseState : IMemoryPackable
    {
        public bool isActive;
        
        public bool leftButtonState;
        public bool rightButtonState;
        public bool middleButtonState;
        public bool button4State;
        public bool button5State;

        
        public RenderVector2 desktopPosition;
        public RenderVector2 windowPosition;

        public RenderVector2 directDelta;       
        public RenderVector2 scrollWheelDelta;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(
                isActive,

                leftButtonState,
                rightButtonState,
                middleButtonState,
                button4State,
                button5State
                );

            packer.Write(desktopPosition);
            packer.Write(windowPosition);

            packer.Write(directDelta);
            packer.Write(scrollWheelDelta);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(
                out isActive,

                out leftButtonState,
                out rightButtonState,
                out middleButtonState,
                out button4State,
                out button5State
                );

            packer.Read(ref desktopPosition);
            packer.Read(ref windowPosition);

            packer.Read(ref directDelta);
            packer.Read(ref scrollWheelDelta);
        }
    }
}
