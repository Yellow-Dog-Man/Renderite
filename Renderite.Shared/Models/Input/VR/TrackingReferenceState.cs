using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class TrackingReferenceState : IMemoryPackable, ITrackedDevice
    {
        public string uniqueId;

        public bool isTracking;

        public RenderVector3 position;
        public RenderQuaternion rotation;

        bool ITrackedDevice.IsTracking { get => isTracking; set => isTracking = value; }
        RenderVector3 ITrackedDevice.Position { get => position; set => position = value; }
        RenderQuaternion ITrackedDevice.Rotation { get => rotation; set => rotation = value; }

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(uniqueId);

            packer.Write(isTracking);

            if (isTracking)
            {
                packer.Write(position);
                packer.Write(rotation);
            }
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref uniqueId);

            unpacker.Read(ref isTracking);

            if (isTracking)
            {
                unpacker.Read(ref position);
                unpacker.Read(ref rotation);
            }
        }
    }
}
