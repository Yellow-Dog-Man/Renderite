using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public static class Helper
    {
        public static UnityEngine.Vector2 ToUnity(this Renderite.Shared.RenderVector2 vector) =>
            new UnityEngine.Vector2(vector.x, vector.y);

        public static UnityEngine.Vector3 ToUnity(this Renderite.Shared.RenderVector3 vector) =>
            new UnityEngine.Vector3(vector.x, vector.y, vector.z);

        public static UnityEngine.Vector4 ToUnity(this Renderite.Shared.RenderVector4 vector) =>
            new UnityEngine.Vector4(vector.x, vector.y, vector.z, vector.w);

        public static UnityEngine.Quaternion ToUnity(this Renderite.Shared.RenderQuaternion quaternion) =>
            new UnityEngine.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);

        public static Renderite.Shared.RenderVector2 ToRender(this UnityEngine.Vector2 vector) =>
            new Renderite.Shared.RenderVector2(vector.x, vector.y);

        public static Renderite.Shared.RenderVector3 ToRender(this UnityEngine.Vector3 vector) =>
            new Renderite.Shared.RenderVector3(vector.x, vector.y, vector.z);

        public static Renderite.Shared.RenderVector4 ToRender(this UnityEngine.Vector4 vector) =>
            new Renderite.Shared.RenderVector4(vector.x, vector.y, vector.z, vector.w);

        public static Renderite.Shared.RenderQuaternion ToRender(this UnityEngine.Quaternion quaternion) =>
            new Renderite.Shared.RenderQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);

        public static Renderite.Shared.RenderVector2i ToRender(this UnityEngine.Vector2Int vector) =>
            new Renderite.Shared.RenderVector2i(vector.x, vector.y);

        public static Renderite.Shared.RenderVector3i ToRender(this UnityEngine.Vector3Int vector) =>
            new Renderite.Shared.RenderVector3i(vector.x, vector.y, vector.z);

        // We need to multiply by 2, because Unity expects size, not extents.
        // However we keep it to extents internally, because we want this struct to be blittable
        // when not using explicitly conversion like this.
        public static UnityEngine.Bounds ToUnity(this Renderite.Shared.RenderBoundingBox bounds) =>
            new UnityEngine.Bounds(bounds.center.ToUnity(), bounds.extents.ToUnity() * 2);

        public static Renderite.Shared.RenderBoundingBox ToRender(this UnityEngine.Bounds bounds) =>
            new RenderBoundingBox(bounds.center.ToRender(), bounds.extents.ToRender());

        // Note that the order of the members matches Unity's internal order, so this struct is blittable
        public static UnityEngine.Matrix4x4 ToUnity(this Renderite.Shared.RenderMatrix4x4 matrix) =>
            new UnityEngine.Matrix4x4(
                new UnityEngine.Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30),
                new UnityEngine.Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31),
                new UnityEngine.Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32),
                new UnityEngine.Vector4(matrix.m03, matrix.m13, matrix.m23, matrix.m33)
                );

        public static UnityEngine.Rect ToUnity(this Renderite.Shared.RenderRect rect) =>
            new Rect(rect.x, rect.y, rect.width, rect.height);

        public static UnityEngine.Rendering.SphericalHarmonicsL2 ToUnity(this Renderite.Shared.RenderSH2 sh)
        {
            var unity = new UnityEngine.Rendering.SphericalHarmonicsL2();

            void Assign(RenderVector3 v, int index, float scale)
            {
                for (int ch = 0; ch < 3; ch++)
                    unity[ch, index] = v[ch] * scale;
            }

            // Unity values are pre-scaled by the coefficients
            Assign(sh.sh0, 0, 0.2820947917f);

            Assign(sh.sh1, 1, 0.4886025119f);
            Assign(sh.sh2, 2, 0.4886025119f);
            Assign(sh.sh3, 3, 0.4886025119f);

            Assign(sh.sh4, 4, 1.0925484306f);
            Assign(sh.sh5, 5, 1.0925484306f);
            Assign(sh.sh7, 7, 1.0925484306f);

            Assign(sh.sh6, 6, 0.3153915652f);

            Assign(sh.sh8, 8, 0.5462742153f);

            return unity;
        }

        public static UnityEngine.Vector2Int ToUnity(this Renderite.Shared.RenderVector2i vector) =>
            new UnityEngine.Vector2Int(vector.x, vector.y);

        public static UnityEngine.Vector3Int ToUnity(this Renderite.Shared.RenderVector3i vector) =>
            new UnityEngine.Vector3Int(vector.x, vector.y, vector.z);

        public static UnityEngine.Rendering.VertexAttribute ToUnity(this Renderite.Shared.VertexAttributeType attribute)
        {
            switch (attribute)
            {
                case VertexAttributeType.Position: return UnityEngine.Rendering.VertexAttribute.Position;
                case VertexAttributeType.Normal: return UnityEngine.Rendering.VertexAttribute.Normal;
                case VertexAttributeType.Tangent: return UnityEngine.Rendering.VertexAttribute.Tangent;
                case VertexAttributeType.Color: return UnityEngine.Rendering.VertexAttribute.Color;
                case VertexAttributeType.UV0: return UnityEngine.Rendering.VertexAttribute.TexCoord0;
                case VertexAttributeType.UV1: return UnityEngine.Rendering.VertexAttribute.TexCoord1;
                case VertexAttributeType.UV2: return UnityEngine.Rendering.VertexAttribute.TexCoord2;
                case VertexAttributeType.UV3: return UnityEngine.Rendering.VertexAttribute.TexCoord3;
                case VertexAttributeType.UV4: return UnityEngine.Rendering.VertexAttribute.TexCoord4;
                case VertexAttributeType.UV5: return UnityEngine.Rendering.VertexAttribute.TexCoord5;
                case VertexAttributeType.UV6: return UnityEngine.Rendering.VertexAttribute.TexCoord6;
                case VertexAttributeType.UV7: return UnityEngine.Rendering.VertexAttribute.TexCoord7;
                case VertexAttributeType.BoneWeights: return UnityEngine.Rendering.VertexAttribute.BlendWeight;
                case VertexAttributeType.BoneIndicies: return UnityEngine.Rendering.VertexAttribute.BlendIndices;

                default:
                    throw new ArgumentOutOfRangeException("Invalid VertexAttributeType mode: " + attribute);
            }
        }

        public static UnityEngine.Rendering.VertexAttributeFormat ToUnity(this Renderite.Shared.VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32: return UnityEngine.Rendering.VertexAttributeFormat.Float32;
                case VertexAttributeFormat.Half16: return UnityEngine.Rendering.VertexAttributeFormat.Float16;

                case VertexAttributeFormat.UNorm8: return UnityEngine.Rendering.VertexAttributeFormat.UNorm8;
                case VertexAttributeFormat.UNorm16: return UnityEngine.Rendering.VertexAttributeFormat.UNorm16;

                case VertexAttributeFormat.SInt8: return UnityEngine.Rendering.VertexAttributeFormat.SInt8;
                case VertexAttributeFormat.SInt16: return UnityEngine.Rendering.VertexAttributeFormat.SInt16;
                case VertexAttributeFormat.SInt32: return UnityEngine.Rendering.VertexAttributeFormat.SInt32;

                case VertexAttributeFormat.UInt8: return UnityEngine.Rendering.VertexAttributeFormat.UInt8;
                case VertexAttributeFormat.UInt16: return UnityEngine.Rendering.VertexAttributeFormat.UInt16;
                case VertexAttributeFormat.UInt32: return UnityEngine.Rendering.VertexAttributeFormat.UInt32;

                default:
                    throw new ArgumentOutOfRangeException("Invalid VertexAttributeFormat mode: " + format);
            }
        }

        public static UnityEngine.Rendering.IndexFormat ToUnity(this Renderite.Shared.IndexBufferFormat format)
        {
            switch (format)
            {
                case IndexBufferFormat.UInt16: return UnityEngine.Rendering.IndexFormat.UInt16;
                case IndexBufferFormat.UInt32: return UnityEngine.Rendering.IndexFormat.UInt32;

                default:
                    throw new ArgumentOutOfRangeException("Invalid IndexBufferFormat mode: " + format);
            }
        }

        public static UnityEngine.MeshTopology ToUnity(this Renderite.Shared.SubmeshTopology format)
        {
            switch (format)
            {
                case SubmeshTopology.Points: return MeshTopology.Points;
                case SubmeshTopology.Triangles: return MeshTopology.Triangles;

                default:
                    throw new ArgumentOutOfRangeException("Invalid SubmeshTopology mode: " + format);
            }
        }

        public static UnityEngine.TextureFormat ToUnity(this Renderite.Shared.TextureFormat format, bool throwOnError = true)
        {
            switch (format)
            {
                case Renderite.Shared.TextureFormat.Alpha8:
                    return UnityEngine.TextureFormat.Alpha8;

                case Renderite.Shared.TextureFormat.R8:
                    return UnityEngine.TextureFormat.R8;

                case Renderite.Shared.TextureFormat.RGB565:
                case Shared.TextureFormat.BGR565:
                    return UnityEngine.TextureFormat.RGB565;

                case Renderite.Shared.TextureFormat.ARGB32:
                    return UnityEngine.TextureFormat.ARGB32;
                case Renderite.Shared.TextureFormat.RGB24:
                    return UnityEngine.TextureFormat.RGB24;
                case Renderite.Shared.TextureFormat.RGBA32:
                    return UnityEngine.TextureFormat.RGBA32;
                case Renderite.Shared.TextureFormat.BGRA32:
                    return UnityEngine.TextureFormat.BGRA32;

                case Renderite.Shared.TextureFormat.RGBAHalf:
                    return UnityEngine.TextureFormat.RGBAHalf;
                case Renderite.Shared.TextureFormat.RGBAFloat:
                    return UnityEngine.TextureFormat.RGBAFloat;

                case Renderite.Shared.TextureFormat.RHalf:
                    return UnityEngine.TextureFormat.RHalf;
                case Renderite.Shared.TextureFormat.RFloat:
                    return UnityEngine.TextureFormat.RFloat;

                case Renderite.Shared.TextureFormat.RGHalf:
                    return UnityEngine.TextureFormat.RGHalf;
                case Renderite.Shared.TextureFormat.RGFloat:
                    return UnityEngine.TextureFormat.RGFloat;

                case Renderite.Shared.TextureFormat.BC1:
                    return UnityEngine.TextureFormat.DXT1;

                case Renderite.Shared.TextureFormat.BC3:
                    return UnityEngine.TextureFormat.DXT5;

                case Renderite.Shared.TextureFormat.BC4:
                    return UnityEngine.TextureFormat.BC4;

                case Renderite.Shared.TextureFormat.BC5:
                    return UnityEngine.TextureFormat.BC5;

                case Renderite.Shared.TextureFormat.BC6H:
                    return UnityEngine.TextureFormat.BC6H;

                case Renderite.Shared.TextureFormat.BC7:
                    return UnityEngine.TextureFormat.BC7;

                case Renderite.Shared.TextureFormat.ETC2_RGB:
                    return UnityEngine.TextureFormat.ETC2_RGB;

                case Renderite.Shared.TextureFormat.ETC2_RGBA1:
                    return UnityEngine.TextureFormat.ETC2_RGBA1;

                case Renderite.Shared.TextureFormat.ETC2_RGBA8:
                    return UnityEngine.TextureFormat.ETC2_RGBA8;

                case Renderite.Shared.TextureFormat.ASTC_4x4:
                    return UnityEngine.TextureFormat.ASTC_4x4;

                case Renderite.Shared.TextureFormat.ASTC_5x5:
                    return UnityEngine.TextureFormat.ASTC_5x5;

                case Renderite.Shared.TextureFormat.ASTC_6x6:
                    return UnityEngine.TextureFormat.ASTC_6x6;

                case Renderite.Shared.TextureFormat.ASTC_8x8:
                    return UnityEngine.TextureFormat.ASTC_8x8;

                case Renderite.Shared.TextureFormat.ASTC_10x10:
                    return UnityEngine.TextureFormat.ASTC_10x10;

                case Renderite.Shared.TextureFormat.ASTC_12x12:
                    return UnityEngine.TextureFormat.ASTC_12x12;

                default:
                    if (throwOnError)
                        throw new Exception($"Invalid texture format {format}");
                    else
                        return (UnityEngine.TextureFormat)(-1);
            }
        }

        public static UnityEngine.Experimental.Rendering.GraphicsFormat ToUnityExperimental(this Renderite.Shared.TextureFormat format,
            ref Renderite.Shared.ColorProfile profile)
        {
            // This function is a bit weirder, because some of the formats, like SRGB variants are not supported for some reason
            // for texture creation. For this reason we check if it's supported and we change the expected color profile if
            // there is not matching format - expecting the outside to convert the texture
            switch (format)
            {
                case Renderite.Shared.TextureFormat.Alpha8:
                    // This is technically supported, but not exposed in the C# API of the Unity Version we have
                    return (UnityEngine.Experimental.Rendering.GraphicsFormat)54;

                case Renderite.Shared.TextureFormat.R8:
                    if (profile == ColorProfile.Linear ||
                        !UnityEngine.SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SRGB,
                        UnityEngine.Experimental.Rendering.FormatUsage.Sample))
                    {
                        profile = ColorProfile.Linear;
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm;
                    }
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGB24:
                    if (profile == ColorProfile.Linear ||
                        !UnityEngine.SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SRGB,
                        UnityEngine.Experimental.Rendering.FormatUsage.Sample))
                    {
                        profile = ColorProfile.Linear;
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UNorm;
                    }
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC1:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT1_UNorm;
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT1_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC2:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT3_UNorm;
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT3_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC3:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT5_UNorm;
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_DXT5_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC4:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R_BC4_UNorm;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC5:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RG_BC5_UNorm;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC6H:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGB_BC6H_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.BC7:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_BC7_UNorm;
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_BC7_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGB565:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R5G6B5_UNormPack16;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGBA32:
                    if (profile == ColorProfile.Linear ||
                        !UnityEngine.SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
                        UnityEngine.Experimental.Rendering.FormatUsage.Sample))
                    {
                        profile = ColorProfile.Linear;
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
                    }
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");


                case Shared.TextureFormat.BGRA32:
                    if (profile == ColorProfile.Linear ||
                        !UnityEngine.SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_SRGB,
                        UnityEngine.Experimental.Rendering.FormatUsage.Sample))
                    {
                        profile = ColorProfile.Linear;
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_UNorm;
                    }
                    else if (profile == ColorProfile.sRGB || profile == ColorProfile.sRGBAlpha)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_SRGB;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RHalf:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");


                case Renderite.Shared.TextureFormat.RFloat:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGHalf:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGFloat:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGBAHalf:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                case Renderite.Shared.TextureFormat.RGBAFloat:
                    if (profile == ColorProfile.Linear)
                        return UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
                    else
                        throw new NotImplementedException($"Invalid profile for {format}: {profile}");

                default:
                    throw new NotImplementedException("Invalid texture format: " + format);
            }
        }

        public static Renderite.Shared.TextureFormat ToEngine(this UnityEngine.TextureFormat format)
        {
            switch (format)
            {
                case UnityEngine.TextureFormat.Alpha8:
                    return Renderite.Shared.TextureFormat.Alpha8;

                case UnityEngine.TextureFormat.ARGB32:
                    return Renderite.Shared.TextureFormat.ARGB32;

                case UnityEngine.TextureFormat.RGB24:
                    return Renderite.Shared.TextureFormat.RGB24;

                case UnityEngine.TextureFormat.RGBA32:
                    return Renderite.Shared.TextureFormat.RGBA32;

                case UnityEngine.TextureFormat.RGBAHalf:
                    return Renderite.Shared.TextureFormat.RGBAHalf;

                case UnityEngine.TextureFormat.RGFloat:
                    return Renderite.Shared.TextureFormat.RGFloat;

                default:
                    return Renderite.Shared.TextureFormat.Unknown;
            }
        }

        public static Renderite.Shared.TextureFormat ToEngine(this UnityEngine.Experimental.Rendering.GraphicsFormat format)
        {
            switch (format)
            {
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SNorm:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SRGB:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UNorm:
                    return Renderite.Shared.TextureFormat.RGB24;

                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SNorm:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm:
                    return Renderite.Shared.TextureFormat.RGBA32;

                case UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat:
                    return Renderite.Shared.TextureFormat.RGBAHalf;

                case UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat:
                    return Renderite.Shared.TextureFormat.RGBAFloat;

                default:
                    throw new NotSupportedException("Unsupposted Unity GraphicsFormat: " + format);
            }
        }

        public static Renderite.Shared.ColorProfile ToEngineProfile(this UnityEngine.Experimental.Rendering.GraphicsFormat format)
        {
            switch (format)
            {
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SNorm:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UNorm:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SNorm:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UInt:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat:
                    return ColorProfile.Linear;

                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SRGB:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SRGB:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SRGB:
                case UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB:
                    return ColorProfile.sRGB;

                default:
                    throw new NotSupportedException("Unsupposted Unity GraphicsFormat: " + format);
            }
        }

        public static UnityEngine.TextureWrapMode ToUnity(this Renderite.Shared.TextureWrapMode wrap)
        {
            switch (wrap)
            {
                case Renderite.Shared.TextureWrapMode.Clamp: return UnityEngine.TextureWrapMode.Clamp;
                case Renderite.Shared.TextureWrapMode.Mirror: return UnityEngine.TextureWrapMode.Mirror;
                case Renderite.Shared.TextureWrapMode.MirrorOnce: return UnityEngine.TextureWrapMode.MirrorOnce;
                case Renderite.Shared.TextureWrapMode.Repeat: return UnityEngine.TextureWrapMode.Repeat;

                default:
                    return UnityEngine.TextureWrapMode.Repeat;
            }
        }

        public static UnityEngine.FilterMode ToUnity(this Renderite.Shared.TextureFilterMode filterMode)
        {
            switch (filterMode)
            {
                case TextureFilterMode.Point: return UnityEngine.FilterMode.Point;
                case TextureFilterMode.Bilinear: return UnityEngine.FilterMode.Bilinear;
                case TextureFilterMode.Trilinear: return UnityEngine.FilterMode.Trilinear;

                default:
                    throw new Exception("Invalid filter mode: " + filterMode);
            }
        }

        public static UnityEngine.LightType ToUnity(this Renderite.Shared.LightType lightType)
        {
            switch (lightType)
            {
                case Renderite.Shared.LightType.Point: return UnityEngine.LightType.Point;
                case Renderite.Shared.LightType.Spot: return UnityEngine.LightType.Spot;
                case Renderite.Shared.LightType.Directional: return UnityEngine.LightType.Directional;

                default:
                    throw new ArgumentOutOfRangeException("Invalid LightType: " + lightType);
            }
        }

        public static UnityEngine.LightShadows ToUnity(this Renderite.Shared.ShadowType shadowType)
        {
            switch (shadowType)
            {
                case Renderite.Shared.ShadowType.None: return UnityEngine.LightShadows.None;
                case Renderite.Shared.ShadowType.Soft: return UnityEngine.LightShadows.Soft;
                case Renderite.Shared.ShadowType.Hard: return UnityEngine.LightShadows.Hard;

                default:
                    throw new ArgumentOutOfRangeException("Invalid ShadowType: " + shadowType);
            }
        }

        public static UnityEngine.Rendering.ShadowCastingMode ToUnity(this ShadowCastMode mode)
        {
            switch (mode)
            {
                case ShadowCastMode.Off:
                    return UnityEngine.Rendering.ShadowCastingMode.Off;
                case ShadowCastMode.On:
                    return UnityEngine.Rendering.ShadowCastingMode.On;
                case ShadowCastMode.ShadowOnly:
                    return UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                case ShadowCastMode.DoubleSided:
                    return UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                default:
                    throw new Exception("Invalid shadow cast mode");
            }
        }

        public static ShadowCastMode ToEngine(this UnityEngine.Rendering.ShadowCastingMode mode)
        {
            switch (mode)
            {
                case UnityEngine.Rendering.ShadowCastingMode.Off:
                    return ShadowCastMode.Off;
                case UnityEngine.Rendering.ShadowCastingMode.On:
                    return ShadowCastMode.On;
                case UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:
                    return ShadowCastMode.ShadowOnly;
                case UnityEngine.Rendering.ShadowCastingMode.TwoSided:
                    return ShadowCastMode.DoubleSided;

                default:
                    throw new Exception("Invalid shadow cast mode");
            }
        }

        public static UnityEngine.MotionVectorGenerationMode ToUnity(this Renderite.Shared.MotionVectorMode mode) => mode switch
        {
            Renderite.Shared.MotionVectorMode.Camera => UnityEngine.MotionVectorGenerationMode.Camera,
            Renderite.Shared.MotionVectorMode.NoMotion => UnityEngine.MotionVectorGenerationMode.ForceNoMotion,
            Renderite.Shared.MotionVectorMode.Object => UnityEngine.MotionVectorGenerationMode.Object,
            _ => throw new Exception("Invalid MotionVectorMode: " + mode)
        };

        public static UnityEngine.CameraClearFlags ToUnity(this CameraClearMode mode)
        {
            switch (mode)
            {
                case CameraClearMode.Depth:
                    return UnityEngine.CameraClearFlags.Depth;

                case CameraClearMode.Nothing:
                    return UnityEngine.CameraClearFlags.Nothing;

                case CameraClearMode.Skybox:
                    return UnityEngine.CameraClearFlags.Skybox;

                case CameraClearMode.Color:
                    return UnityEngine.CameraClearFlags.Color;

                default:
                    throw new Exception("Invalid camera clear mode: " + mode);
            }
        }

        public static UnityEngine.Rendering.ReflectionProbeTimeSlicingMode ToUnity(this ReflectionProbeTimeSlicingMode mode) =>
            mode switch
            {
                ReflectionProbeTimeSlicingMode.AllFacesAtOnce => UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.AllFacesAtOnce,
                ReflectionProbeTimeSlicingMode.IndividualFaces => UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces,
                _ => UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing,
            };

        public static UnityEngine.Rendering.ReflectionProbeClearFlags ToUnity(this ReflectionProbeClear mode) =>
            mode switch
            {
                ReflectionProbeClear.Skybox => UnityEngine.Rendering.ReflectionProbeClearFlags.Skybox,
                ReflectionProbeClear.Color => UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor,
                _ => UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor
            };

        public static UnityEngine.ParticleSystemTrailTextureMode ToUnity(this TrailTextureMode textureMode)
        {
            switch (textureMode)
            {
                case TrailTextureMode.Stretch: return ParticleSystemTrailTextureMode.Stretch;
                case TrailTextureMode.Tile: return ParticleSystemTrailTextureMode.Tile;
                case TrailTextureMode.DistributePerSegment: return ParticleSystemTrailTextureMode.DistributePerSegment;
                case TrailTextureMode.RepeatPerSegment: return ParticleSystemTrailTextureMode.RepeatPerSegment;

                default:
                    throw new ArgumentOutOfRangeException("Invalid texture mode: " + textureMode);
            }
        }

        public static UnityEngine.ShadowResolution ToUnity(this ShadowResolutionMode resolution)
        {
            switch (resolution)
            {
                case ShadowResolutionMode.Low: return UnityEngine.ShadowResolution.Low;
                case ShadowResolutionMode.Medium: return UnityEngine.ShadowResolution.Medium;
                case ShadowResolutionMode.High: return UnityEngine.ShadowResolution.High;
                case ShadowResolutionMode.Ultra: return UnityEngine.ShadowResolution.VeryHigh;

                default:
                    throw new ArgumentOutOfRangeException("Invalid shadow resolution mode: " + resolution);
            }
        }

        public static int ToUnity(this ShadowCascadeMode shadowCascades)
        {
            switch (shadowCascades)
            {
                case ShadowCascadeMode.None:
                    return 1;

                case ShadowCascadeMode.TwoCascades:
                    return 2;

                case ShadowCascadeMode.FourCascades:
                    return 4;

                default:
                    throw new ArgumentOutOfRangeException("Invalid shadow cascades: " + shadowCascades);
            }
        }

        public static SkinWeights ToUnity(this SkinWeightMode mode)
        {
            switch (mode)
            {
                case SkinWeightMode.OneBone:
                    return SkinWeights.OneBone;

                case SkinWeightMode.TwoBones:
                    return SkinWeights.TwoBones;

                case SkinWeightMode.FourBones:
                    return SkinWeights.FourBones;

                case SkinWeightMode.Unlimited:
                    return SkinWeights.Unlimited;

                default:
                    throw new ArgumentOutOfRangeException("Invalid skin weight mode: " + mode);
            }
        }

        //public static UnityEngine.TouchScreenKeyboardType ToUnity(this KeyboardType keyboardType)
        //{
        //    switch (keyboardType)
        //    {
        //        case KeyboardType.Default:
        //        default:
        //            return UnityEngine.TouchScreenKeyboardType.Default;

        //        case KeyboardType.ASCIICapable:
        //            return UnityEngine.TouchScreenKeyboardType.ASCIICapable;

        //        case KeyboardType.EmailAddress:
        //            return UnityEngine.TouchScreenKeyboardType.EmailAddress;

        //        case KeyboardType.NamePhonePad:
        //            return UnityEngine.TouchScreenKeyboardType.NamePhonePad;

        //        case KeyboardType.NumberPad:
        //            return UnityEngine.TouchScreenKeyboardType.NumberPad;

        //        case KeyboardType.NumbersAndPunctuation:
        //            return UnityEngine.TouchScreenKeyboardType.NumbersAndPunctuation;

        //        case KeyboardType.PhonePad:
        //            return UnityEngine.TouchScreenKeyboardType.PhonePad;

        //        case KeyboardType.Search:
        //            return UnityEngine.TouchScreenKeyboardType.Search;

        //        case KeyboardType.Social:
        //            return UnityEngine.TouchScreenKeyboardType.Social;

        //        case KeyboardType.URL:
        //            return UnityEngine.TouchScreenKeyboardType.URL;
        //    }
        //}

        public static string ToMillisecondTimeString(this DateTime datetime)
        {
            return datetime.ToLongTimeString() + "." + datetime.Millisecond.ToString("D3");
        }

        public static string ToDebugString(this UnityEngine.Rendering.SphericalHarmonicsL2 sh)
        {
            var str = new StringBuilder();

            for (int c = 0; c < 9; c++)
                str.AppendLine($"SH{c} - [{sh[0, c]}; {sh[1, c]}; {sh[2, c]}]");

            return str.ToString();
        }
    }
}
