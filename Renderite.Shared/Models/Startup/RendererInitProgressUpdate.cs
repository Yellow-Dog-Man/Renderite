using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    /// <summary>
    /// This lets the renderer to know to update the init progress displayed to the user
    /// </summary>
    public class RendererInitProgressUpdate : RendererCommand
    {
        /// <summary>
        /// Init progress percentage from 0.0 to 1.0
        /// </summary>
        public float progress;

        /// <summary>
        /// Name of the init phase
        /// </summary>
        public string phase;

        /// <summary>
        /// Subphase message
        /// </summary>
        public string subPhase;

        /// <summary>
        /// Whether to force show the phase name
        /// </summary>
        public bool forceShow;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(progress);
            packer.Write(phase);
            packer.Write(subPhase);
            packer.Write(forceShow);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref progress);
            packer.Read(ref phase);
            packer.Read(ref subPhase);
            packer.Read(ref forceShow);
        }
    }
}
