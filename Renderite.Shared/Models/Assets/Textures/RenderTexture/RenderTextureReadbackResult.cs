using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Indicates that readback from a render texture has completed and the buffer data can be
    /// processed and disposed.
    /// </summary>
    public class RenderTextureReadbackResult : AssetCommand
    {
        /// <summary>
        /// Unique ID for the readback task. This will match the same value of the readback task
        /// and is used to match the result to specific task.
        /// </summary>
        public int readbackTaskId;

        /// <summary>
        /// Indicates if the readback was success. If false, then the buffer data is likely invalid
        /// and should not be processed
        /// </summary>
        public bool success;
    }
}
