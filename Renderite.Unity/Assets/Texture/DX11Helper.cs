using Renderite.Shared;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public static class DX11Helper
    {
        public static Format ToDX11(this Renderite.Shared.TextureFormat format,
         Renderite.Shared.ColorProfile profile, bool usingLinearSpace)
        {
            var converted = format.TryToDX11(profile, usingLinearSpace);

            if (converted != null)
                return converted.Value;

            throw new NotSupportedException($"Cannot convert {format} with color profile {profile} to DX11");
        }

        public static Format? TryToDX11(this Renderite.Shared.TextureFormat format,
            Renderite.Shared.ColorProfile profile, bool usingLinearSpace)
        {
            switch (format)
            {
                case Renderite.Shared.TextureFormat.Alpha8:
                    return Format.A8_UNorm;

                case TextureFormat.R8:
                    return Format.R8_UNorm;

                case Renderite.Shared.TextureFormat.RGBA32:
                    switch (profile)
                    {
                        case ColorProfile.Linear:
                            return Format.R8G8B8A8_UNorm;
                        case ColorProfile.sRGB:
                        case ColorProfile.sRGBAlpha:
                            if (usingLinearSpace)
                                return Format.R8G8B8A8_UNorm_SRgb;
                            else
                                return Format.R8G8B8A8_UNorm;
                        default:
                            return null;
                    }

                case Renderite.Shared.TextureFormat.RGBAHalf:
                    return Format.R16G16B16A16_Float;

                case Renderite.Shared.TextureFormat.RGBAFloat:
                    return Format.R32G32B32A32_Float;

                case TextureFormat.RHalf:
                    return Format.R16_Float;

                case TextureFormat.RGHalf:
                    return Format.R16G16_Float;

                case TextureFormat.RFloat:
                    return Format.R32_Float;

                case TextureFormat.RGFloat:
                    return Format.R32G32_Float;

                case Renderite.Shared.TextureFormat.BC1:
                    switch (profile)
                    {
                        case ColorProfile.Linear:
                            return Format.BC1_UNorm;

                        case ColorProfile.sRGB:
                        case ColorProfile.sRGBAlpha:
                            if (usingLinearSpace)
                                return Format.BC1_UNorm_SRgb;
                            else
                                return Format.BC1_UNorm;
                        default:
                            return null;
                    }

                case Renderite.Shared.TextureFormat.BC2:
                    switch (profile)
                    {
                        case ColorProfile.Linear:
                            return Format.BC2_UNorm;

                        case ColorProfile.sRGB:
                        case ColorProfile.sRGBAlpha:
                            if (usingLinearSpace)
                                return Format.BC2_UNorm_SRgb;
                            else
                                return Format.BC2_UNorm;
                        default:
                            return null;
                    }

                case Renderite.Shared.TextureFormat.BC3:
                    switch (profile)
                    {
                        case ColorProfile.Linear:
                            return Format.BC3_UNorm;

                        case ColorProfile.sRGB:
                        case ColorProfile.sRGBAlpha:
                            if (usingLinearSpace)
                                return Format.BC3_UNorm_SRgb;
                            else
                                return Format.BC3_UNorm;
                        default:
                            return null;
                    }

                case Renderite.Shared.TextureFormat.BC4:
                    return Format.BC4_UNorm;

                case Renderite.Shared.TextureFormat.BC5:
                    return Format.BC5_UNorm;

                case Renderite.Shared.TextureFormat.BC6H:
                    return Format.BC6H_Uf16;

                case Renderite.Shared.TextureFormat.BC7:
                    switch (profile)
                    {
                        case ColorProfile.Linear:
                            return Format.BC7_UNorm;

                        case ColorProfile.sRGB:
                        case ColorProfile.sRGBAlpha:
                            if (usingLinearSpace)
                                return Format.BC7_UNorm_SRgb;
                            else
                                return Format.BC7_UNorm;
                        default:
                            return null;
                    }

                case TextureFormat.BGR565:
                    return Format.B5G6R5_UNorm;

                case Renderite.Shared.TextureFormat.RGB565:
                case Renderite.Shared.TextureFormat.BGRA32:
                case Renderite.Shared.TextureFormat.ARGB32:
                case Renderite.Shared.TextureFormat.RGB24:
                    return null;

                default:
                    return null;
            }
        }

    }
}
