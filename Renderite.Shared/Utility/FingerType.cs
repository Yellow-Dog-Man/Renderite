using Elements.Data;
using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.FingerType", "FrooxEngine")]
    public enum FingerType
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky
    }

    public static class FingerExtensions
    {
        public static bool IsFinger(this BodyNode node)
        {
            node = node.GetLeftSide();

            return node >= BodyNode.LEFT_FINGER_START && node <= BodyNode.LEFT_FINGER_END;
        }

        public static FingerType GetFingerType(this BodyNode node, bool throwOnInvalid = false)
        {
            node = node.GetLeftSide();

            if (node >= BodyNode.LeftThumb_Metacarpal && node <= BodyNode.LeftThumb_Tip)
                return FingerType.Thumb;

            if (node >= BodyNode.LeftIndexFinger_Metacarpal && node <= BodyNode.LeftIndexFinger_Tip)
                return FingerType.Index;

            if (node >= BodyNode.LeftMiddleFinger_Metacarpal && node <= BodyNode.LeftMiddleFinger_Tip)
                return FingerType.Middle;

            if (node >= BodyNode.LeftRingFinger_Metacarpal && node <= BodyNode.LeftRingFinger_Tip)
                return FingerType.Ring;

            if (node >= BodyNode.LeftPinky_Metacarpal && node <= BodyNode.LeftPinky_Tip)
                return FingerType.Pinky;

            if (throwOnInvalid)
                throw new Exception("Not a finger node");

            return (FingerType)(-1);
        }

        public static bool IsValidFinger(this FingerType finger, FingerSegmentType segment)
        {
            if (finger == FingerType.Thumb)
                return segment != FingerSegmentType.Intermediate;

            return true;
        }

        public static BodyNode ComposeFinger(this FingerType finger, FingerSegmentType segment, Chirality chirality,
            bool throwOnInvalid = true)
        {
            BodyNode node;

            switch (finger)
            {
                case FingerType.Thumb:
                    switch (segment)
                    {
                        case FingerSegmentType.Distal:
                        case FingerSegmentType.Intermediate:
                            node = BodyNode.LeftThumb_Distal;
                            break;

                        case FingerSegmentType.Metacarpal:
                            node = BodyNode.LeftThumb_Metacarpal;
                            break;

                        case FingerSegmentType.Proximal:
                            node = BodyNode.LeftThumb_Proximal;
                            break;

                        case FingerSegmentType.Tip:
                            node = BodyNode.LeftThumb_Tip;
                            break;

                        default:
                            if (throwOnInvalid)
                                throw new ArgumentException($"Invalid Thumb FingerSegment: {segment}");

                            return (BodyNode)(-1);
                    }
                    break;

                case FingerType.Index:
                    node = BodyNode.LeftIndexFinger_Metacarpal + (int)segment;
                    break;

                case FingerType.Middle:
                    node = BodyNode.LeftMiddleFinger_Metacarpal + (int)segment;
                    break;

                case FingerType.Ring:
                    node = BodyNode.LeftRingFinger_Metacarpal + (int)segment;
                    break;

                case FingerType.Pinky:
                    node = BodyNode.LeftPinky_Metacarpal + (int)segment;
                    break;

                default:
                    if (throwOnInvalid)
                        throw new ArgumentException("Invalid finger");

                    return (BodyNode)(-1);
            }

            return node.GetSide(chirality);
        }
    }
}
