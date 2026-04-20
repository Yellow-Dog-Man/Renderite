using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.ControllerNode", "FrooxEngine")]
    [OldTypeName("FrooxEngine.Chirality", "FrooxEngine")]
    public enum Chirality : sbyte
    {
        Left, Right
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.BodyNode", "FrooxEngine")]
    public enum BodyNode
    {
        NONE,

        Root,   // Represents the root space of the avatar, usually tracking space root

        // View Node
        View,

        // Controller nodes
        LeftController,
        RightController,

        Hips,
        Spine,
        Chest,
        UpperChest,

        Neck,
        Head,
        Jaw,

        // eyes
        LeftEye,
        RightEye,

        // Arm/Hand bones
        LeftShoulder,
        LeftUpperArm,
        LeftLowerArm,   // forearm
        LeftHand,
        LeftPalm,

        LeftThumb_Metacarpal,
        LeftThumb_Proximal,
        LeftThumb_Distal,
        LeftThumb_Tip,

        LeftIndexFinger_Metacarpal,
        LeftIndexFinger_Proximal,
        LeftIndexFinger_Intermediate,
        LeftIndexFinger_Distal,
        LeftIndexFinger_Tip,

        LeftMiddleFinger_Metacarpal,
        LeftMiddleFinger_Proximal,
        LeftMiddleFinger_Intermediate,
        LeftMiddleFinger_Distal,
        LeftMiddleFinger_Tip,

        LeftRingFinger_Metacarpal,
        LeftRingFinger_Proximal,
        LeftRingFinger_Intermediate,
        LeftRingFinger_Distal,
        LeftRingFinger_Tip,

        LeftPinky_Metacarpal,
        LeftPinky_Proximal,
        LeftPinky_Intermediate,
        LeftPinky_Distal,
        LeftPinky_Tip,


        RightShoulder,
        RightUpperArm,
        RightLowerArm,
        RightHand,
        RightPalm,

        RightThumb_Metacarpal,
        RightThumb_Proximal,
        RightThumb_Distal,
        RightThumb_Tip,

        RightIndexFinger_Metacarpal,
        RightIndexFinger_Proximal,
        RightIndexFinger_Intermediate,
        RightIndexFinger_Distal,
        RightIndexFinger_Tip,

        RightMiddleFinger_Metacarpal,
        RightMiddleFinger_Proximal,
        RightMiddleFinger_Intermediate,
        RightMiddleFinger_Distal,
        RightMiddleFinger_Tip,

        RightRingFinger_Metacarpal,
        RightRingFinger_Proximal,
        RightRingFinger_Intermediate,
        RightRingFinger_Distal,
        RightRingFinger_Tip,

        RightPinky_Metacarpal,
        RightPinky_Proximal,
        RightPinky_Intermediate,
        RightPinky_Distal,
        RightPinky_Tip,

        // legs
        LeftUpperLeg,
        LeftLowerLeg,
        LeftFoot,
        LeftToes,

        RightUpperLeg,
        RightLowerLeg,
        RightFoot,
        RightToes,

        END,

        // special values
        LEFT_FINGER_START = LeftThumb_Metacarpal,
        LEFT_FINGER_END = LeftPinky_Tip,

        RIGHT_FINGER_START = RightThumb_Metacarpal,
        RIGHT_FINGER_END = RightPinky_Tip
    }

    public static class BodyNodeExtensions
    {
        public static Chirality GetOther(this Chirality side) => side == Chirality.Left ? Chirality.Right : Chirality.Left;

        public static int GetFingerNodeIndex(this BodyNode node, out Chirality chirality)
        {
            if (node >= BodyNode.LEFT_FINGER_START && node <= BodyNode.LEFT_FINGER_END)
            {
                chirality = Chirality.Left;
                return node - BodyNode.LEFT_FINGER_START;
            }
            else if (node >= BodyNode.RIGHT_FINGER_START && node <= BodyNode.RIGHT_FINGER_END)
            {
                chirality = Chirality.Right;
                return node - BodyNode.RIGHT_FINGER_START;
            }

            chirality = (Chirality)(-1);
            return -1;
        }

        public static BodyNode GetRelativeNode(this BodyNode node)
        {
            if (node == BodyNode.NONE)
                return BodyNode.NONE;

            if (node.IsFinger() || node.IsPalm())
                return BodyNode.LeftHand.GetSide(node.GetChirality());

            if (node.IsEye())
                return BodyNode.Head;

            return BodyNode.Root;
        }

        public static bool IsEye(this BodyNode node) => node == BodyNode.LeftEye || node == BodyNode.RightEye;

        public static BodyNode GetLeftSide(this BodyNode node) => GetSide(node, Chirality.Left);
        public static BodyNode GetRightSide(this BodyNode node) => GetSide(node, Chirality.Right);

        public static BodyNode GetOtherSide(this BodyNode node)
        {
            var side = node.GetChirality();

            if (side == Chirality.Left)
                return node.GetRightSide();
            else
                return node.GetLeftSide();
        }

        public static Chirality GetChirality(this BodyNode node)
        {
            if (node == BodyNode.LeftController)
                return Chirality.Left;
            if (node == BodyNode.RightController)
                return Chirality.Right;

            if (node >= BodyNode.LeftShoulder && node <= BodyNode.LeftPinky_Tip)
                return Chirality.Left;

            if (node >= BodyNode.RightShoulder && node <= BodyNode.RightPinky_Tip)
                return Chirality.Right;


            if (node >= BodyNode.LeftUpperLeg && node <= BodyNode.LeftToes)
                return Chirality.Left;

            if (node >= BodyNode.RightUpperLeg && node <= BodyNode.RightToes)
                return Chirality.Right;

            if (node == BodyNode.LeftEye)
                return Chirality.Left;

            if (node == BodyNode.RightEye)
                return Chirality.Right;

            return (Chirality)(-1);
        }

        public static BodyNode GetSide(this BodyNode node, Chirality chirality)
        {
            bool left = chirality == Chirality.Left;

            if (node == BodyNode.LeftController || node == BodyNode.RightController)
                return left ? BodyNode.LeftController : BodyNode.RightController;

            if (node >= BodyNode.LeftShoulder && node <= BodyNode.LeftPinky_Tip)
            {
                if (left)
                    return node;
                else
                    return node + (BodyNode.RightShoulder - BodyNode.LeftShoulder);
            }

            if (node >= BodyNode.LeftUpperLeg && node <= BodyNode.LeftToes)
            {
                if (left)
                    return node;
                else
                    return node + (BodyNode.RightUpperLeg - BodyNode.LeftUpperLeg);
            }

            if (node >= BodyNode.RightShoulder && node <= BodyNode.RightPinky_Tip)
            {
                if (left)
                    return node - (BodyNode.RightShoulder - BodyNode.LeftShoulder);
                else
                    return node;
            }

            if (node >= BodyNode.RightUpperLeg && node <= BodyNode.RightToes)
            {
                if (left)
                    return node - (BodyNode.RightUpperLeg - BodyNode.LeftUpperLeg);
                else
                    return node;
            }

            if (node.IsEye())
                return left ? BodyNode.LeftEye : BodyNode.RightEye;

            return BodyNode.NONE;
        }
    }

    public static class HandExtensions
    {
        public static bool IsPalm(this BodyNode node) => node == BodyNode.LeftPalm || node == BodyNode.RightPalm;
        public static bool IsHand(this BodyNode node) => node == BodyNode.LeftHand || node == BodyNode.RightHand;
        public static bool IsForearm(this BodyNode node) => node == BodyNode.LeftLowerArm || node == BodyNode.RightLowerArm;
        public static bool IsUpperArm(this BodyNode node) => node == BodyNode.LeftUpperArm || node == BodyNode.RightUpperArm;
        public static bool IsShoulder(this BodyNode node) => node == BodyNode.LeftShoulder || node == BodyNode.RightShoulder;
        public static bool IsFoot(this BodyNode node) => node == BodyNode.LeftFoot || node == BodyNode.RightFoot;

        public static Chirality GetHandChirality(this BodyNode node)
        {
            if (node >= BodyNode.LeftShoulder && node <= BodyNode.LeftPinky_Tip)
                return Chirality.Left;

            if (node >= BodyNode.RightShoulder && node <= BodyNode.RightPinky_Tip)
                return Chirality.Right;

            throw new Exception("Not a hand node");
        }
    }
}
