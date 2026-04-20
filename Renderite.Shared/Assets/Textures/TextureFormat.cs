using Elements.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("Elements.Assets.TextureFormat", "Elements.Assets")]
    public enum TextureFormat
    {
        Unknown = 0,

        Alpha8 = 1,
        R8 = 2,

        RGB24 = 16,

        // Alpha
        ARGB32 = 17,
        RGBA32 = 18,
        BGRA32 = 19,

        // Non 8-bit per pixel
        RGB565 = 24,
        BGR565 = 25,

        // Floating point
        RGBAHalf = 32,
        ARGBHalf = 33,
        RHalf = 34,
        RGHalf = 35,

        RGBAFloat = 48,
        ARGBFloat = 49,
        RFloat = 50,
        RGFloat = 51,

        // Block compressed
        BC1 = 64, // DXT1
        BC2 = 65, // DXT2/DXT3
        BC3 = 66, // DXT4/DXT5
        BC4 = 67,
        BC5 = 68,
        BC6H = 69,
        BC7 = 70,

        ETC2_RGB = 96,
        ETC2_RGBA1 = 97,
        ETC2_RGBA8 = 98,

        // Not enabled currently see: https://github.com/Yellow-Dog-Man/InternalDiscussion/issues/863
        ASTC_4x4 = 128,
        ASTC_5x5 = 129,
        ASTC_6x6 = 130,
        ASTC_8x8 = 131,
        ASTC_10x10 = 132,
        ASTC_12x12 = 133,
    }

    public static class TextureFormatExtensions
    {
        public static bool SupportsRead(this TextureFormat format)
        {
            if (format == TextureFormat.Unknown)
                return false;

            if (format.IsBlockCompressed())
                return false;

            return true;
        }

        public static bool SupportsWrite(this TextureFormat format)
        {
            if (format == TextureFormat.Unknown)
                return false;

            if (format.IsBlockCompressed())
                return false;

            return true;
        }

        public static bool IsBlockCompressed(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.BC1:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;

                default:
                    return false;
            }
        }

        public static RenderVector2i BlockSize(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.BC1:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                    return new RenderVector2i(4);

                case TextureFormat.ASTC_5x5:
                    return new RenderVector2i(5);
                case TextureFormat.ASTC_6x6:
                    return new RenderVector2i(6);
                case TextureFormat.ASTC_8x8:
                    return new RenderVector2i(8);
                case TextureFormat.ASTC_10x10:
                    return new RenderVector2i(10);
                case TextureFormat.ASTC_12x12:
                    return new RenderVector2i(12);

                default:
                    return new RenderVector2i(1);
            }
        }

        public static RenderVector3i BlockSize3D(this TextureFormat format)
        {
            switch (format)
            {
                default:
                    // Fallback to the 2D block size, assuming that the volume is compressed as slices of 2D layers
                    // If a particular texture format compresses 3D blocks, we override them here
                    var blockSize = format.BlockSize();
                    return new RenderVector3i(blockSize.x, blockSize.y, 1);
            }
        }

        public static bool IsHDR(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.RGBAHalf:
                case TextureFormat.ARGBHalf:
                case TextureFormat.RGBAFloat:
                case TextureFormat.ARGBFloat:
                case TextureFormat.BC6H:
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                    return true;

                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                case TextureFormat.RGB24:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                case TextureFormat.BC1:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC7:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return false;

                case TextureFormat.Unknown:
                    return false;

                default:
                    throw new ArgumentException("Invalid texture format: " + format);
            }
        }

        public static double GetBitsPerPixel(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                    return 1 * 8;

                case TextureFormat.RGB24:
                    return 3 * 8;

                case TextureFormat.RGB565:
                case TextureFormat.BGR565:
                    return 16;

                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                    return 4 * 8;

                case TextureFormat.RGBAHalf:
                case TextureFormat.ARGBHalf:
                    return 4 * 16;

                case TextureFormat.RHalf:
                    return 16;

                case TextureFormat.RGHalf:
                    return 2 * 16;

                case TextureFormat.RGBAFloat:
                case TextureFormat.ARGBFloat:
                    return 4 * 32;

                case TextureFormat.RGFloat:
                    return 2 * 32;

                case TextureFormat.RFloat:
                    return 32;

                case TextureFormat.BC1:
                case TextureFormat.BC4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                    return 4;

                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ETC2_RGBA8:
                    return 1 * 8;

                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    var blockSize = format.BlockSize();
                    return 128 / (double)(blockSize.x * blockSize.y);

                case TextureFormat.Unknown:
                    return 0;

                default:
                    throw new ArgumentException("Invalid texture format: " + format);
            }
        }

        public static int GetBytesPerPixel(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                    return 1;

                case TextureFormat.RGB565:
                case TextureFormat.BGR565:
                    return 2;

                case TextureFormat.RGB24:
                    return 3;

                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                    return 4;

                case TextureFormat.RGBAHalf:
                case TextureFormat.ARGBHalf:
                    return 2 * 4;

                case TextureFormat.RGBAFloat:
                case TextureFormat.ARGBFloat:
                    return 4 * 4;

                case TextureFormat.RHalf:
                    return 2;

                case TextureFormat.RGHalf:
                    return 2 * 2;

                case TextureFormat.RFloat:
                    return 4;

                case TextureFormat.RGFloat:
                    return 2 * 4;

                case TextureFormat.BC1:
                    throw new Exception("Bytes per pixel is less than 1, use GetBitsPerPixel");

                case TextureFormat.BC3:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.BC7:
                    return 1;

                default:
                    throw new ArgumentException("Invalid texture format: " + format);
            }
        }

        public static int GetBytesPerChannel(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                case TextureFormat.RGB24:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                    return 1;

                case TextureFormat.RGBAHalf:
                case TextureFormat.ARGBHalf:
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                    return 2;

                case TextureFormat.RGBAFloat:
                case TextureFormat.ARGBFloat:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                    return 4;

                default:
                    throw new ArgumentException("Invalid texture format: " + format);
            }
        }

        public static int GetChannels(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                case TextureFormat.BC4:
                case TextureFormat.RHalf:
                case TextureFormat.RFloat:
                    return 1;

                case TextureFormat.RGHalf:
                case TextureFormat.RGFloat:
                case TextureFormat.BC5:
                    return 2;

                case TextureFormat.RGB24:
                case TextureFormat.BC6H:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.RGB565:
                case TextureFormat.BGR565:
                    return 3;

                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RGBAFloat:
                case TextureFormat.ARGBHalf:
                case TextureFormat.ARGBFloat:
                case TextureFormat.BC1:
                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC7:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return 4;

                default:
                    throw new ArgumentException("Invalid texture format: " + format);
            }
        }

        public static bool SupportsAlpha(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8:
                    return true;

                case TextureFormat.R8:
                case TextureFormat.RGB24:
                case TextureFormat.RGB565:
                case TextureFormat.BGR565:
                case TextureFormat.RHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGHalf:
                case TextureFormat.RGFloat:
                    return false;

                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGBA32:
                case TextureFormat.RGBAHalf:
                case TextureFormat.ARGBHalf:
                case TextureFormat.RGBAFloat:
                case TextureFormat.ARGBFloat:
                    return true;

                case TextureFormat.BC1:
                case TextureFormat.BC4:
                case TextureFormat.BC6H:
                    return false;

                case TextureFormat.BC2:
                case TextureFormat.BC3:
                case TextureFormat.BC7:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;

                case TextureFormat.ETC2_RGB:
                    return false;

                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                    return true;

                default:
                    throw new ArgumentException("Invalid texture format: " + format);
            }
        }

        public static bool IsDisabled(this TextureFormat format)
        {
            switch (format)
            {
                // Newer Versions of Compressonator Native do not support ASTC.
                //see: https://github.com/Yellow-Dog-Man/InternalDiscussion/issues/863 for discussion
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;
                default:
                    return false;
            }
        }

        public readonly struct AlternateFormat
        {
            public readonly TextureFormat format;
            public readonly bool upgradeOnly;

            public AlternateFormat(TextureFormat format, bool upgradeOnly)
            {
                this.format = format;
                this.upgradeOnly = upgradeOnly;
            }

            public static implicit operator AlternateFormat(TextureFormat format) => new AlternateFormat(format, false);
        }

        static List<List<AlternateFormat>> _compatibleFormatGroups = new List<List<AlternateFormat>>()
        {
            new List<AlternateFormat>
            {
                TextureFormat.ARGB32,
                TextureFormat.RGBA32,
                TextureFormat.BGRA32,
                new AlternateFormat(TextureFormat.RGB24, true)
            },

            new List<AlternateFormat>
            {
                TextureFormat.RGB565,
                TextureFormat.BGR565
            },
        };

        public static IReadOnlyList<AlternateFormat> GetCompatibleFormatGroup(this TextureFormat format)
        {
            foreach (var group in _compatibleFormatGroups)
                if (group.Any(f => f.format == format))
                    return group;

            return null;
        }

        public static TextureFormat? FindCompatibleFormat(this TextureFormat format, Predicate<TextureFormat> filter)
        {
            var group = format.GetCompatibleFormatGroup();

            if (group == null)
                return null;

            foreach(var alternate in group)
            {
                // These can only be upgraded, but are not allowed to be used
                if (alternate.upgradeOnly)
                    continue;

                if (filter(alternate.format))
                    return alternate.format;
            }

            return null;
        }

        /// <summary>
        /// Is this texture format expensive to compute?
        /// </summary>
        /// <param name="f">target formate</param>
        /// <returns><see langword="true"/>, if this format is expensive, <see langword="false"/> otherwise.</returns>
        /// <remarks>We determine manually if a format is "expensive" through observations of its performance during testing.</remarks>
        public static bool IsExpensiveToCompute(this TextureFormat f)
        {
            switch (f)
            {
                // These have been proven to be very expensive through extensive testing
                // See: https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/1983
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                    return true;

                default:
                    return false;
            }
        }
    }
}
