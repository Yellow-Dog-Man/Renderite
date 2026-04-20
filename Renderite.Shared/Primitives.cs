using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    // This contains basic data primitives. These are separate from Elements.Core, so they can use .NET Standard 2.0 for
    // compatibility purposes. However they are blittable with those types.
    // These don't provide any advanced functionality, they are just for raw data exchange

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct RenderVector2
    {
        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        public RenderVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString() => $"X: {x}, Y: {y}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct RenderVector3
    {

        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        [FieldOffset(8)]
        public float z;

        public float this[int index] => index switch
        {
            0 => x,
            1 => y,
            2 => z,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        public RenderVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"X: {x}, Y: {y}, Z: {z}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct RenderVector4
    {

        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        [FieldOffset(8)]
        public float z;

        [FieldOffset(12)]
        public float w;

        public RenderVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString() => $"X: {x}, Y: {y}, Z: {z}, W: {w}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct RenderQuaternion
    {

        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        [FieldOffset(8)]
        public float z;

        [FieldOffset(12)]
        public float w;

        public RenderQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString() => $"X: {x}, Y: {y}, Z: {z}, W: {w}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct RenderMatrix4x4
    {
        // Note the order of these is matching Unity Matrix4x4 to be blittable
        [FieldOffset(0)] public float m00;
        [FieldOffset(4)] public float m10;
        [FieldOffset(8)] public float m20;
        [FieldOffset(12)] public float m30;

        [FieldOffset(16)] public float m01;
        [FieldOffset(20)] public float m11;
        [FieldOffset(24)] public float m21;
        [FieldOffset(28)] public float m31;

        [FieldOffset(32)] public float m02;
        [FieldOffset(36)] public float m12;
        [FieldOffset(40)] public float m22;
        [FieldOffset(44)] public float m32;

        [FieldOffset(48)] public float m03;
        [FieldOffset(52)] public float m13;
        [FieldOffset(56)] public float m23;
        [FieldOffset(60)] public float m33;

        // Note that the order here is row by row, so different than the order of the actual fields
        public RenderMatrix4x4(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            this.m00 = m00; this.m01 = m01; this.m02 = m02; this.m03 = m03;
            this.m10 = m10; this.m11 = m11; this.m12 = m12; this.m13 = m13;
            this.m20 = m20; this.m21 = m21; this.m22 = m22; this.m23 = m23;
            this.m30 = m30; this.m31 = m31; this.m32 = m32; this.m33 = m33;
        }

        public override string ToString() =>
            $"[{m00}, {m01}, {m02}, {m03}], " +
            $"[{m10}, {m11}, {m12}, {m13}], " +
            $"[{m20}, {m21}, {m22}, {m23}], " +
            $"[{m30}, {m31}, {m32}, {m33}]";
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public struct RenderTransform
    {
        [FieldOffset(0)]
        public RenderVector3 position;

        [FieldOffset(12)]
        public RenderVector3 scale;

        [FieldOffset(24)]
        public RenderQuaternion rotation;

        public RenderTransform(RenderVector3 position, RenderQuaternion rotation, RenderVector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override string ToString() => $"Position: {position}, Rotation: {rotation}, Scale: {scale}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct RenderVector2i
    {
        [FieldOffset(0)]
        public int x;

        [FieldOffset(4)]
        public int y;

        public RenderVector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public RenderVector2i(int xy) : this(xy, xy)
        {

        }

        public override string ToString() => $"X: {x}, Y: {y}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct RenderVector3i
    {
        [FieldOffset(0)]
        public int x;

        [FieldOffset(4)]
        public int y;

        [FieldOffset(8)]
        public int z;

        public RenderVector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public RenderVector3i(int xyz) : this(xyz, xyz, xyz)
        {

        }

        public override string ToString() => $"X: {x}, Y: {y}, Z: {z}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct RenderBoundingBox
    {
        [FieldOffset(0)]
        public RenderVector3 center;

        [FieldOffset(12)]
        public RenderVector3 extents;

        public RenderBoundingBox(RenderVector3 center, RenderVector3 extents)
        {
            this.center = center;
            this.extents = extents;
        }

        public override string ToString() => $"Center: {center}, Extents: {extents}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct RenderRect
    {
        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        [FieldOffset(8)]
        public float width;

        [FieldOffset(12)]
        public float height;

        public RenderRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public override string ToString() => $"X: {x}, Y: {y}, Width: {width}, Height: {height}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct RenderIntRect
    {
        [FieldOffset(0)]
        public int x;

        [FieldOffset(4)]
        public int y;

        [FieldOffset(8)]
        public int width;

        [FieldOffset(12)]
        public int height;

        public RenderIntRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public override string ToString() => $"X: {x}, Y: {y}, Width: {width}, Height: {height}";
    }

    [StructLayout(LayoutKind.Explicit, Size = 108)]
    public struct RenderSH2
    {
        [FieldOffset(0)] public RenderVector3 sh0;
        [FieldOffset(12)] public RenderVector3 sh1;
        [FieldOffset(24)] public RenderVector3 sh2;
        [FieldOffset(36)] public RenderVector3 sh3;
        [FieldOffset(48)] public RenderVector3 sh4;
        [FieldOffset(60)] public RenderVector3 sh5;
        [FieldOffset(72)] public RenderVector3 sh6;
        [FieldOffset(84)] public RenderVector3 sh7;
        [FieldOffset(96)] public RenderVector3 sh8;

        public RenderVector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return this.sh0;
                    case 1: return this.sh1;
                    case 2: return this.sh2;
                    case 3: return this.sh3;
                    case 4: return this.sh4;
                    case 5: return this.sh5;
                    case 6: return this.sh6;
                    case 7: return this.sh7;
                    case 8: return this.sh8;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public RenderSH2(RenderVector3 sh0,
            RenderVector3 sh1,
            RenderVector3 sh2,
            RenderVector3 sh3,
            RenderVector3 sh4,
            RenderVector3 sh5,
            RenderVector3 sh6,
            RenderVector3 sh7,
            RenderVector3 sh8
            )
        {
            this.sh0 = sh0;
            this.sh1 = sh1;
            this.sh2 = sh2;
            this.sh3 = sh3;
            this.sh4 = sh4;
            this.sh5 = sh5;
            this.sh6 = sh6;
            this.sh7 = sh7;
            this.sh8 = sh8;
        }

        public override string ToString() =>
            $"SH0: {sh0}\n" +
            $"SH1: {sh1}\n" +
            $"SH2: {sh2}\n" +
            $"SH3: {sh3}\n" +
            $"SH4: {sh4}\n" +
            $"SH5: {sh5}\n" +
            $"SH6: {sh6}\n" +
            $"SH7: {sh7}\n" +
            $"SH8: {sh8}";
    }
}
