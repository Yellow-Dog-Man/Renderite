using System;
using System.Collections.Generic;
using System.Text;



namespace Renderite.Shared
{
    /// <summary>
    /// This indicates that the renderer is ready to render another frame and provides the state of the renderer-bound inputs
    /// and other data that the main engine might need in order to operate.
    /// </summary>
    public class FrameStartData : RendererCommand
    {
        /// <summary>
        /// Index of the last frame that was submitted and rendered
        /// </summary>
        
        public int lastFrameIndex;

        /// <summary>
        /// Current state of performance collected by the renderer
        /// </summary>
        
        public PerformanceState performance;

        /// <summary>
        /// Contains the state of the inputs that need to be tracked and managed by the renderer
        /// </summary>
        
        public InputState inputs;

        /// <summary>
        /// List of ReflectionProbe renderable ID's that have finished rendering at the beginning of this frame.
        /// We pass these as part of frame start data, because probes can take a variable amount of time to finish
        /// rendering, so we cannot pre-allocate buffers to indicate which ones have finished from the renderer side.
        /// However generally the amount of realtime reflection probes in a scene is pretty low, so a buffer could be
        /// an overkill and providing the results this way should be easier and more efficient overall.
        /// </summary>
        
        public List<ReflectionProbeChangeRenderResult> renderedReflectionProbes;

        public List<VideoTextureClockErrorState> videoClockErrors;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(lastFrameIndex);
            packer.WriteObject(performance);
            packer.WriteObject(inputs);
            packer.WriteValueList(renderedReflectionProbes);
            packer.WriteValueList(videoClockErrors);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref lastFrameIndex);
            packer.ReadObject(ref performance);
            packer.ReadObject(ref inputs);
            packer.ReadValueList(ref renderedReflectionProbes);
            packer.ReadValueList(ref videoClockErrors);
        }
    }
}
