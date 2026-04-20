using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.HeadOutputDevice", "FrooxEngine")]
    public enum HeadOutputDevice
    {
        Autodetect,
        Headless,
        Screen,
        Screen360,
        StaticCamera,
        StaticCamera360,
        SteamVR,
        WindowsMR,
        Oculus,
        OculusQuest,
        UNKNOWN,
    }

    public static class HeadOutputDeviceExtension
    {
        public static bool IsScreenViewSupported(this HeadOutputDevice device)
        {
            switch (device)
            {
                case HeadOutputDevice.OculusQuest:
                    return false;

                default:
                    return true;
            }
        }

        public static bool IsVRViewSupported(this HeadOutputDevice device)
        {
            switch (device)
            {
                case HeadOutputDevice.Screen:
                case HeadOutputDevice.Screen360:
                case HeadOutputDevice.StaticCamera:
                case HeadOutputDevice.StaticCamera360:
                    return false;

                default:
                    return true;
            }
        }

        public static bool IsNewInteractionMode(this HeadOutputDevice device) => !device.IsCameraMode();

        public static bool IsCameraMode(this HeadOutputDevice device)
        {
            switch (device)
            {
                case HeadOutputDevice.StaticCamera:
                case HeadOutputDevice.StaticCamera360:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsCamera(this HeadOutputDevice device)
        {
            switch (device)
            {
                case HeadOutputDevice.StaticCamera:
                case HeadOutputDevice.StaticCamera360:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasTwoControllers(this HeadOutputDevice device)
        {
            switch (device)
            {
                case HeadOutputDevice.SteamVR:
                case HeadOutputDevice.Oculus:
                case HeadOutputDevice.WindowsMR:
                case HeadOutputDevice.OculusQuest:
                    return true;

                default:
                    return false;
            }
        }

        public static bool HasVoice(this HeadOutputDevice device)
        {
            if (device.IsCamera())
                return false;

            return true;
        }

        public static bool IsVR(this HeadOutputDevice device)
        {
            switch (device)
            {
                case HeadOutputDevice.SteamVR:
                case HeadOutputDevice.WindowsMR:
                case HeadOutputDevice.OculusQuest:
                case HeadOutputDevice.Oculus:
                    return true;

                default:
                    return false;
            }
        }
    }
}
