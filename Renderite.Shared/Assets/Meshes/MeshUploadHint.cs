using EnumsNET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct MeshUploadHint
    {
        [Flags]
        public enum Flag
        {
            VertexLayout = 1 << 0,  // 1
            SubmeshLayout = 1 << 1,  // 2

            Geometry = 1 << 2,  // 4

            Positions = 1 << 3,  // 8

            Normals = 1 << 4,  // 16
            Tangents = 1 << 5,  // 32
            Colors = 1 << 6,  // 64

            UV0s = 1 << 7,  // 128
            UV1s = 1 << 8,  // 256
            UV2s = 1 << 9,  // 512
            UV3s = 1 << 10, // 1024
            UV4s = 1 << 11, // 2048
            UV5s = 1 << 12, // 4096
            UV6s = 1 << 13, // 8192
            UV7s = 1 << 14, // 16384

            BindPoses = 1 << 15, // 32768
            BoneWeights = 1 << 16, // 65536

            Blendshapes = 1 << 17, // 131072

            Dynamic = 1 << 18, // 262144
            Readable = 1 << 19, // 524288

            Debug = 1 << 20  // 1048576
        }

        public static Flag GetUVFlag(int uvChannel)
        {
            return uvChannel switch
            {
                0 => Flag.UV0s,
                1 => Flag.UV1s,
                2 => Flag.UV2s,
                3 => Flag.UV3s,
                4 => Flag.UV4s,
                5 => Flag.UV5s,
                6 => Flag.UV6s,
                7 => Flag.UV7s,
                _ => throw new ArgumentOutOfRangeException(nameof(uvChannel), "Invalid UV channel: " + uvChannel),
            };
        }

        [FieldOffset(0)]
        Flag _flags;

        public Flag Flags => _flags;

        public MeshUploadHint(Flag flags)
        {
            _flags = flags;
        }

        public bool AnyVertexStreams =>
            this[Flag.Positions] ||
            this[Flag.Normals] ||
            this[Flag.Tangents] ||
            this[Flag.Colors] ||
            this[Flag.UV0s] ||
            this[Flag.UV1s] ||
            this[Flag.UV2s] ||
            this[Flag.UV3s] ||
            this[Flag.UV4s] ||
            this[Flag.UV5s] ||
            this[Flag.UV6s] ||
            this[Flag.UV7s];

        public bool this[Flag channel]
        {
            get => _flags.HasAnyFlags(channel);
            set
            {
                if (value)
                    _flags |= channel;
                else
                    _flags &= ~channel;
            }
        }

        public bool GetUVChannel(int uv)
        {
            switch (uv)
            {
                case 0: return this[Flag.UV0s];
                case 1: return this[Flag.UV1s];
                case 2: return this[Flag.UV2s];
                case 3: return this[Flag.UV3s];
                case 4: return this[Flag.UV4s];
                case 5: return this[Flag.UV5s];
                case 6: return this[Flag.UV6s];
                case 7: return this[Flag.UV7s];

                default:
                    throw new Exception("Invalid UV channel: " + uv);
            }
        }        

        public void ResetAll() => _flags = 0;
        public void SetAll(bool debug = false)
        {
            _flags = unchecked((Flag)~0);

            // Handle debug separately. We want this to be set very explicitly
            this[Flag.Debug] = debug;
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            foreach (var flag in Enums.GetValues<Flag>())
                str.Append($"{flag}: {this[flag]}, ");

            str.Length -= 2;

            return str.ToString();
        }
    }
}
