using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    /// <summary>
    /// This is sent by the engine to tell the renderer to render another frame. It contains references on what
    /// buffers need to be updated before the frame renders and any specific render jobs that need to happen
    /// within this frame - this includes the primary one, that's for the user's output, but also any sub-jobs
    /// like rendering to textures, which will then have to be sent back. These are included as part of the frame
    /// render data, because we want to make sure they're synchronized with the frame.
    /// </summary>
    public class FrameSubmitData : RendererCommand
    {
        public int frameIndex;
        
        public bool debugLog;
        
        public bool vrActive;
        
        public float nearClip;
        public float farClip;
        public float desktopFOV;

        public OutputState outputState;

        public List<RenderSpaceUpdate> renderSpaces = new List<RenderSpaceUpdate>();
        
        public List<CameraRenderTask> renderTasks;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(frameIndex);

            packer.Write(debugLog);

            packer.Write(vrActive);

            packer.Write(nearClip);
            packer.Write(farClip);
            packer.Write(desktopFOV);

            packer.WriteObject(outputState);

            packer.WriteObjectList(renderSpaces);
            packer.WriteObjectList(renderTasks);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref frameIndex);
            packer.Read(ref debugLog);

            packer.Read(ref vrActive);

            packer.Read(ref nearClip);
            packer.Read(ref farClip);
            packer.Read(ref desktopFOV);

            packer.ReadObject(ref outputState);

            packer.ReadObjectList(ref renderSpaces);
            packer.ReadObjectList(ref renderTasks);
        }
    }
}
