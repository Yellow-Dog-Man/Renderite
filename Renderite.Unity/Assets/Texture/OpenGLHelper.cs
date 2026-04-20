//using NativeGraphics.NET;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Renderite.Shared;

//namespace Renderite.Unity
//{
//    public struct OpenGL_TextureFormat : IEquatable<OpenGL_TextureFormat>
//    {
//        public OpenGL.TextureFormat storageFormat;
//        public OpenGL.TextureFormat baseFormat;
//        public OpenGL.PixelType pixelType;
//        public bool isCompressed;

//        public static bool operator ==(OpenGL_TextureFormat a, OpenGL_TextureFormat b)
//        {
//            return a.storageFormat == b.storageFormat &&
//                a.baseFormat == b.baseFormat &&
//                a.pixelType == b.pixelType;
//        }

//        public static bool operator !=(OpenGL_TextureFormat a, OpenGL_TextureFormat b)
//        {
//            return !(a == b);
//        }

//        public override string ToString()
//        {
//            return $"StorageFormat: {storageFormat}, Base: {baseFormat}, Pixel: {pixelType}";
//        }

//        public override bool Equals(object obj)
//        {
//            if (obj is OpenGL_TextureFormat other)
//                return Equals(other);

//            return false;
//        }

//        public override int GetHashCode()
//        {
//            int hashCode = 1325013550;
//            hashCode = hashCode * -1521134295 + storageFormat.GetHashCode();
//            hashCode = hashCode * -1521134295 + baseFormat.GetHashCode();
//            hashCode = hashCode * -1521134295 + pixelType.GetHashCode();
//            hashCode = hashCode * -1521134295 + isCompressed.GetHashCode();
//            return hashCode;
//        }

//        public bool Equals(OpenGL_TextureFormat other)
//        {
//            return storageFormat == other.storageFormat &&
//                   baseFormat == other.baseFormat &&
//                   pixelType == other.pixelType &&
//                   isCompressed == other.isCompressed;
//        }
//    }

//    public static class OpenGL_Helper
//    {
//        public static OpenGL_TextureFormat ToOpenGL(this Renderite.Shared.TextureFormat format, Renderite.Shared.ColorProfile profile,
//            bool isUsingLinearSpace)
//        {
//            var openGL = new OpenGL_TextureFormat();

//            openGL.isCompressed = format.IsBlockCompressed();
//            openGL.pixelType = OpenGL.PixelType.GL_UNSIGNED_BYTE;

//            switch (format)
//            {
//                case Renderite.Shared.TextureFormat.Alpha8:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_ALPHA;
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_ALPHA8;
//                    break;

//                case Renderite.Shared.TextureFormat.RGB565:
//                    throw new NotSupportedException($"Format {format} is not supported by OpenGL natively");

//                case Renderite.Shared.TextureFormat.RGB24:
//                    // You generally want to have your base format align with the channels of the texture itself.
//                    // The driver takes this as a hint relative to the storage format used.
//                    // I know this is cumbersome, but it's just how OpenGL is.
//                    // This should never deviate regardless of the storage format. If it's RGBA, it's always RGBA. Not sRGB_ALPHA.
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGB;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_RGB8;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_SRGB8;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_RGB8;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.BC1:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGB;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB_S3TC_DXT1_EXT;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.BC2:
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.BC3:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.BC4:
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RED_RGTC1;
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RED;
//                    break;

//                case TextureFormat.BC5:
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RG_RGTC2;
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RG;
//                    break;

//                case TextureFormat.BC6H:
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT_ARB;
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA16F;
//                    openGL.pixelType = OpenGL.PixelType.GL_HALF_FLOAT;
//                    break;

//                case TextureFormat.BC7:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_BPTC_UNORM_ARB;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM_ARB;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_BPTC_UNORM_ARB;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.BGRA32:
//                case TextureFormat.ARGB32:
//                    throw new NotSupportedException($"Format {format} is not supported by OpenGL natively");

//                case TextureFormat.RGBA32:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_RGBA8;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_SRGB8_ALPHA8;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_RGBA8;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.RGBAHalf:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_RGBA16F;
//                    openGL.pixelType = OpenGL.PixelType.GL_HALF_FLOAT;
//                    break;

//                case TextureFormat.RGBAFloat:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    openGL.storageFormat = OpenGL.TextureFormat.GL_RGBA32F;
//                    openGL.pixelType = OpenGL.PixelType.GL_FLOAT;
//                    break;

//                case TextureFormat.ETC2_RGB:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGB;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB8_ETC2;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ETC2;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB8_ETC2;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ETC2_RGBA1:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ETC2_RGBA8:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA8_ETC2_EAC;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA8_ETC2_EAC;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ASTC_4x4:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_4x4_KHR;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ASTC_5x5:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x5_KHR;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ASTC_6x6:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x6_KHR;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ASTC_8x8:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x8_KHR;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ASTC_10x10:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_10x10_KHR;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x10_KHR;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_10x10_KHR;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                case TextureFormat.ASTC_12x12:
//                    openGL.baseFormat = OpenGL.TextureFormat.GL_RGBA;
//                    switch (profile)
//                    {
//                        case ColorProfile.Linear:
//                            openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_12x12_KHR;
//                            break;

//                        case ColorProfile.sRGB:
//                        case ColorProfile.sRGBAlpha:
//                            if (isUsingLinearSpace)
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x12_KHR;
//                            }
//                            else
//                            {
//                                openGL.storageFormat = OpenGL.TextureFormat.GL_COMPRESSED_RGBA_ASTC_12x12_KHR;
//                            }

//                            break;
//                        default:
//                            throw new Exception($"Invalid color profile {profile}");
//                    }
//                    break;

//                default:
//                    throw new Exception($"Invalid texture format {format}");
//            }

//            return openGL;
//        }

//        public static OpenGL.ParameterName ToOpenGL(this Renderite.Shared.TextureWrapMode mode)
//        {
//            // TODO!!! Actual params?!

//            switch (mode)
//            {
//                case TextureWrapMode.Repeat:
//                    return OpenGL.ParameterName.GL_REPEAT;

//                case TextureWrapMode.Clamp:
//                    return OpenGL.ParameterName.GL_CLAMP_TO_EDGE;

//                case TextureWrapMode.Mirror:
//                case TextureWrapMode.MirrorOnce:
//                    return OpenGL.ParameterName.GL_MIRRORED_REPEAT;

//                default:
//                    throw new Exception("Invalid TextureWrapMode: " + mode);
//            }
//        }
//    }
//}
