using Elements.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    [DataModelType]
    [OldTypeName("FrooxEngine.ReflectionProbe+Clear", "FrooxEngine")]
    public enum ReflectionProbeClear : byte
    {
        Skybox,
        Color
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.ReflectionProbe+Type", "FrooxEngine")]
    public enum ReflectionProbeType : byte
    {
        Baked,
        OnChanges,
        Realtime
    }

    [DataModelType]
    [OldTypeName("FrooxEngine.ReflectionProbe+TimeSlicingMode", "FrooxEngine")]
    public enum ReflectionProbeTimeSlicingMode : byte
    {
        AllFacesAtOnce,
        IndividualFaces,
        NoTimeSlicing
    }
}
