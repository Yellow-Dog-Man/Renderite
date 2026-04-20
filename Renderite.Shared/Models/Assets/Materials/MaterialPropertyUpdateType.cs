using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public enum MaterialPropertyUpdateType : byte
    {
        /// <summary>
        /// This is a "meta" property - it tells the system which material is the next set of updates for,
        /// in order to avoid specifying the material assetID with every single property individually.
        /// The material ID is taken from the Int buffer
        /// </summary>
        SelectTarget,

        /// <summary>
        /// Assigns a specific shader to this material. The ShaderID is specified in the property ID and matches
        /// the asset ID of the given shader
        /// </summary>
        SetShader,

        /// <summary>
        /// Sets the render-queue of the material. The value is stored in the actual propertyID, because there's no
        /// property ID for RenderQueue, allowing us to use the field for the value itself to save data.
        /// </summary>
        SetRenderQueue,

        /// <summary>
        /// Set whether the material should use instancing or not. The value is stored in the propertyID, since there
        /// is no actual property ID matching this. 0 means false, 1 means true
        /// </summary>
        SetInstancing,

        /// <summary>
        /// Render type of given material. The int value is stored in the propertyID since there's no associated
        /// propertyID with this. The value is the integer value of the enum MaterialRenderType
        /// </summary>
        SetRenderType,

        /// <summary>
        /// Sets a single float property
        /// </summary>
        SetFloat,

        /// <summary>
        /// Set a single float4 property
        /// </summary>
        SetFloat4,

        /// <summary>
        /// Set a single float4x4 (matrix) property
        /// </summary>
        SetFloat4x4,

        /// <summary>
        /// Set a float array. The length of the array is stored in the Int buffer and then
        /// appropriate amount of the float values is taken from the float buffer.
        /// </summary>
        SetFloatArray,

        /// <summary>
        /// Set float4 array. The length of the array is stored in the Int buffer and then
        /// appropriate amount of the float4 values is taken from the float4 buffer.
        /// </summary>
        SetFloat4Array,

        /// <summary>
        /// Assigns a texture property. Each texture assignment uses two values from Int buffer.
        /// - First Int - texture type (2D, 3D, Cube, Render...)
        /// - Second Int - asset ID of that texture type
        /// </summary>
        SetTexture,

        /// <summary>
        /// This indicates that the update batch has finished
        /// </summary>
        UpdateBatchEnd,
    }
}
