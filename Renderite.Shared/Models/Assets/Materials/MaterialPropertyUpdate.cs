using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Describes an update for a single material property. The material that this targets must be specified
    /// by a SelectMaterial property in the buffer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct MaterialPropertyUpdate
    {
        /// <summary>
        /// The ID of the property that's being assigned
        /// </summary>
        [FieldOffset(0)]
        public int propertyID;

        /// <summary>
        /// The type of the property update
        /// </summary>
        [FieldOffset(4)]
        public MaterialPropertyUpdateType updateType;

        public MaterialPropertyUpdate(int propertyId, MaterialPropertyUpdateType updateType)
        {
            this.propertyID = propertyId;
            this.updateType = updateType;
        }

        public override string ToString() => $"{updateType} (PropertyID: {propertyID})";
    }
}
