using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class TrackerState : IMemoryPackable, ITrackedDevice
    {
        public string uniqueId;

        public bool isTracking;

        public RenderVector3 position;
        public RenderQuaternion rotation;

        public float batteryLevel;
        public bool batteryCharging;

        bool ITrackedDevice.IsTracking { get => isTracking; set => isTracking = value; }
        RenderVector3 ITrackedDevice.Position { get => position; set => position = value; }
        RenderQuaternion ITrackedDevice.Rotation { get => rotation; set => rotation = value; }

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(uniqueId);

            packer.Write(isTracking,
                batteryCharging);

            if (isTracking)
            {
                packer.Write(position);
                packer.Write(rotation);
            }

            packer.Write(batteryLevel);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref uniqueId);

            unpacker.Read(out isTracking,
                out batteryCharging);

            if (isTracking)
            {
                unpacker.Read(ref position);
                unpacker.Read(ref rotation);
            }

            unpacker.Read(ref batteryLevel);
        }
    }
}
