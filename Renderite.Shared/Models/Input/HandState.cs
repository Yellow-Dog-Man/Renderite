using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class HandState : IMemoryPackable
    {
        public string uniqueId;

        public int priority;

        public Chirality chirality;

        public bool isDeviceActive;
        public bool isTracking;

        public bool tracksMetacarpals;

        public float confidence;

        public RenderVector3 wristPosition;
        public RenderQuaternion wristRotation;

        public List<RenderVector3> segmentPositions;
        public List<RenderQuaternion> segmentRotations;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(uniqueId);

            packer.Write(priority);

            packer.Write(chirality);

            packer.Write(isDeviceActive, isTracking,
                tracksMetacarpals);

            packer.Write(confidence);

            if (isTracking)
            {
                packer.Write(wristPosition);
                packer.Write(wristRotation);

                packer.WriteValueList(segmentPositions);
                packer.WriteValueList(segmentRotations);
            }
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref uniqueId);

            unpacker.Read(ref priority);

            unpacker.Read(ref chirality);

            unpacker.Read(out isDeviceActive, out isTracking,
                out tracksMetacarpals);

            unpacker.Read(ref confidence);

            if (isTracking)
            {
                unpacker.Read(ref wristPosition);
                unpacker.Read(ref wristRotation);

                unpacker.ReadValueList(ref segmentPositions);
                unpacker.ReadValueList(ref segmentRotations);
            }
        }
    }
}
