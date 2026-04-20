using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class ViveHandState : IMemoryPackable
    {
        public float confidence;

        public RenderVector3 position;
        public RenderQuaternion rotation;

        public float pinchStrength;

        public List<RenderVector3> points = new List<RenderVector3>();

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(confidence);

            packer.Write(position);
            packer.Write(rotation);

            packer.Write(pinchStrength);

            packer.WriteValueList(points);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref confidence);

            unpacker.Read(ref position);
            unpacker.Read(ref rotation);

            unpacker.Read(ref pinchStrength);

            unpacker.ReadValueList(ref points);
        }
    }

    public class ViveHandTrackingInputState : IMemoryPackable
    {
        public bool isTracking;

        public ViveHandState left;
        public ViveHandState right;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(isTracking);

            if (isTracking)
            {
                packer.WriteObject(left);
                packer.WriteObject(right);
            }
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref isTracking);

            if (isTracking)
            {
                unpacker.ReadObject(ref left);
                unpacker.ReadObject(ref right);
            }
        }
    }
}
