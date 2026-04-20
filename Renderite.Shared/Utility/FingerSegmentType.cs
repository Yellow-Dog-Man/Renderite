using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.FingerSegmentType", "FrooxEngine")]
    public enum FingerSegmentType
    {
        Metacarpal,
        Proximal,
        Intermediate,
        Distal,
        Tip
    }

    public static class FingerSegmentExtensions
    {
        public static int GetFingerSegmentIndex(this BodyNode node)
        {
            node = node.GetLeftSide();

            return (int)(node - BodyNode.LEFT_FINGER_START);
        }

        public static FingerSegmentType GetFingerSegmentType(this BodyNode node, bool throwOnException = false)
        {
            // offset it from the right side to the left
            node = node.GetLeftSide();

            switch (node)
            {
                // special cases for thumb, because it only has 4
                case BodyNode.LeftThumb_Metacarpal:
                    return FingerSegmentType.Metacarpal;
                case BodyNode.LeftThumb_Proximal:
                    return FingerSegmentType.Proximal;
                case BodyNode.LeftThumb_Distal:
                    return FingerSegmentType.Distal;
                case BodyNode.LeftThumb_Tip:
                    return FingerSegmentType.Tip;

                default:
                    if (node > BodyNode.LeftPinky_Tip || node < BodyNode.LeftIndexFinger_Metacarpal)
                    {
                        if (throwOnException)
                            throw new Exception("Not a finger node");
                        else
                            return (FingerSegmentType)(-1);
                    }

                    return (FingerSegmentType)((node - BodyNode.LeftIndexFinger_Metacarpal) % 5);
            }
        }
    }
}
