using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class GamepadState : IMemoryPackable
    {
        // TODO!!! Avoid re-serializing this every single time? It can be tricky though as it need to identify the gamepad
        // somehow between the two, so providing the "full state" every time is easiest
        public string displayName;

        public RenderVector2 leftThumbstick;
        public RenderVector2 rightThumbstick;

        public RenderVector2 dPad;

        public float leftTrigger;
        public float rightTrigger;

        public bool leftThumbstickClick;
        public bool rightThumbstickClick;

        public bool dPadUp;
        public bool dPadRight;
        public bool dPadDown;
        public bool dPadLeft;

        public bool leftBumper;
        public bool rightBumper;

        public bool start;
        public bool menu;

        public bool a;
        public bool b;
        public bool x;
        public bool y;

        public bool paddle0;
        public bool paddle1;
        public bool paddle2;
        public bool paddle3;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(displayName);

            packer.Write(leftThumbstick);
            packer.Write(rightThumbstick);

            packer.Write(dPad);

            packer.Write(leftTrigger);
            packer.Write(rightTrigger);

            packer.Write(
                leftThumbstickClick,
                rightThumbstickClick,
                dPadUp,
                dPadRight,
                dPadDown,
                dPadLeft,
                leftBumper,
                rightBumper);

            packer.Write(
                start,
                menu);

            packer.Write(
                a,
                b,
                x,
                y,
                paddle0,
                paddle1,
                paddle2,
                paddle3
                );
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref displayName);

            unpacker.Read(ref leftThumbstick);
            unpacker.Read(ref rightThumbstick);

            unpacker.Read(ref dPad);

            unpacker.Read(ref leftTrigger);
            unpacker.Read(ref rightTrigger);

            unpacker.Read(
                out leftThumbstickClick,
                out rightThumbstickClick,
                out dPadUp,
                out dPadRight,
                out dPadDown,
                out dPadLeft,
                out leftBumper,
                out rightBumper);

            unpacker.Read(
                out start,
                out menu);

            unpacker.Read(
                out a,
                out b,
                out x,
                out y,
                out paddle0,
                out paddle1,
                out paddle2,
                out paddle3
                );
        }
    }
}
