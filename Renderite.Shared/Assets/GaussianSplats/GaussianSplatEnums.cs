using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    // The numbers on these enums are assigned explicitly so they will match what the compute shader expects
    // However these numbers CAN be updated! Serialization should be using the string names, which must stay the same
    // Only consideration is that the shader needs to be updated.
    [DataModelType]
    [OldTypeName("Elements.Assets.GaussianVectorFormat", "Elements.Assets")]
    public enum GaussianVectorFormat
    {
        Float32 = 0, // 12 bytes: 32F.32F.32F
        Norm16 = 1, // 6 bytes: 16.16.16
        Norm11 = 2, // 4 bytes: 11.10.11
        Norm6 = 3,   // 2 bytes: 6.5.5
    }

    [DataModelType]
    [OldTypeName("Elements.Assets.GaussianRotationFormat", "Elements.Assets")]
    public enum GaussianRotationFormat
    {
        PackedNorm10 = 0,
    }

    [DataModelType]
    [OldTypeName("Elements.Assets.GaussianColorFormat", "Elements.Assets")]
    public enum GaussianColorFormat
    {
        Float32x4 = 0,
        Float16x4 = 1,
        Norm8x4 = 2,
        BC7 = 3,
    }

    [DataModelType]
    [OldTypeName("Elements.Assets.GaussianSHFormat", "Elements.Assets")]
    public enum GaussianSHFormat
    {
        Float16 = 0,

        Norm11 = 1,
        Norm6 = 2,

        Cluster64k = 3,
        Cluster32k = 4,
        Cluster16k = 5,
        Cluster8k = 6,
        Cluster4k = 7,
    }
}
