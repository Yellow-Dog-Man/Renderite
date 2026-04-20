using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public class VR_InputsState : IMemoryPackable
    {
        public bool userPresentInHeadset;
        public bool dashboardOpen;

        public HeadsetState headsetState;

        public List<VR_ControllerState> controllers;
        public List<TrackerState> trackers;
        public List<TrackingReferenceState> trackingReferences;
        public List<HandState> hands;

        public ViveHandTrackingInputState viveHandTracking;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(
                userPresentInHeadset,
                dashboardOpen);

            packer.WriteObject(headsetState);

            packer.WritePolymorphicList(controllers);
            packer.WriteObjectList(trackers);
            packer.WriteObjectList(trackingReferences);
            packer.WriteObjectList(hands);

            packer.WriteObject(viveHandTracking);
        }

        public void Unpack(ref MemoryUnpacker unpacker)
        {
            unpacker.Read(
                out userPresentInHeadset,
                out dashboardOpen);

            unpacker.ReadObject(ref headsetState);

            unpacker.ReadPolymorphicList(ref controllers);
            unpacker.ReadObjectList(ref trackers);
            unpacker.ReadObjectList(ref trackingReferences);
            unpacker.ReadObjectList(ref hands);

            unpacker.ReadObject(ref viveHandTracking);
        }
    }
}
