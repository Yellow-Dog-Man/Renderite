using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public abstract class VR_ControllerState : PolymorphicMemoryPackableEntity<VR_ControllerState>, ITrackedDevice
    {
        public string deviceID;
        public string deviceModel;

        public Chirality side;
        public BodyNode bodyNode;

        public bool isDeviceActive;
        public bool isTracking;

        public RenderVector3 position;
        public RenderQuaternion rotation;

        public bool hasBoundHand;

        public RenderVector3 handPosition;
        public RenderQuaternion handRotation;

        public float batteryLevel;
        public bool batteryCharging;

        bool ITrackedDevice.IsTracking { get => isTracking; set => isTracking = value; }
        RenderVector3 ITrackedDevice.Position { get => position; set => position = value; }
        RenderQuaternion ITrackedDevice.Rotation { get => rotation; set => rotation = value; }

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(deviceID);
            packer.Write(deviceModel);

            packer.Write(side);
            packer.Write(bodyNode);

            packer.Write(isDeviceActive, isTracking, hasBoundHand);

            if (isTracking)
            {
                packer.Write(position);
                packer.Write(rotation);

                if(hasBoundHand)
                {
                    packer.Write(handPosition);
                    packer.Write(handRotation);
                }

                // Battery level isn't updated when the controller is not tracking
                packer.Write(batteryLevel);
                packer.Write(batteryCharging);
            }
        }

        public override void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(ref deviceID);
            unpacker.Read(ref deviceModel);

            unpacker.Read(ref side);
            unpacker.Read(ref bodyNode);

            unpacker.Read(out isDeviceActive, out isTracking, out hasBoundHand);

            if (isTracking)
            {
                unpacker.Read(ref position);
                unpacker.Read(ref rotation);

                if(hasBoundHand)
                {
                    unpacker.Read(ref handPosition);
                    unpacker.Read(ref handRotation);
                }

                unpacker.Read(ref batteryLevel);
                unpacker.Read(ref batteryCharging);
            }
        }

        static VR_ControllerState()
        {
            InitTypes(new List<Type>()
            {
                typeof(CosmosControllerState),
                typeof(GenericControllerState),
                typeof(HP_ReverbControllerState),
                typeof(IndexControllerState),
                typeof(PicoNeo2ControllerState),
                typeof(TouchControllerState),
                typeof(ViveControllerState),
                typeof(WindowsMR_ControllerState),
            });
        }
    }
}
