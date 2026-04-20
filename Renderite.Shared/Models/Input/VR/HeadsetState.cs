using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public enum HeadsetConnection
    {
        Wired,
        WirelessGeneral,
        WirelessSteamLink
    }

    public class HeadsetState : IMemoryPackable, ITrackedDevice
    {
        public bool isTracking;

        public RenderVector3 position;
        public RenderQuaternion rotation;

        public float batteryLevel;
        public bool batteryCharging;

        public HeadsetConnection connectionType;
        public string headsetManufacturer;
        public string headsetModel;

        bool ITrackedDevice.IsTracking { get => isTracking; set => isTracking = value; }
        RenderVector3 ITrackedDevice.Position { get => position; set => position = value; }
        RenderQuaternion ITrackedDevice.Rotation { get => rotation; set => rotation = value; }

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(isTracking,
                batteryCharging);

            if (isTracking)
            {
                packer.Write(position);
                packer.Write(rotation);

                packer.Write(batteryLevel);

            }

            packer.Write(connectionType);
            packer.Write(headsetManufacturer);
            packer.Write(headsetModel);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(out isTracking,
                out batteryCharging);

            if (isTracking)
            {
                unpacker.Read(ref position);
                unpacker.Read(ref rotation);

                unpacker.Read(ref batteryLevel);
            }

            unpacker.Read(ref connectionType);
            unpacker.Read(ref headsetManufacturer);
            unpacker.Read(ref headsetModel);
        }
    }
}
