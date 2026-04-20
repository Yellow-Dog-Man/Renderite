using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class WindowState : IMemoryPackable
    {
        
        public bool isWindowFocused;

        
        public bool isFullscreen;

        
        public RenderVector2i windowResolution;

        
        public bool resolutionSettingsApplied;

        
        public DragAndDropEvent dragAndDropEvent;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(isWindowFocused);
            packer.Write(isFullscreen);
            packer.Write(windowResolution);
            packer.Write(resolutionSettingsApplied);

            packer.WriteObject(dragAndDropEvent);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref isWindowFocused);
            packer.Read(ref isFullscreen);
            packer.Read(ref windowResolution);
            packer.Read(ref resolutionSettingsApplied);

            packer.ReadObject(ref dragAndDropEvent);
        }
    }
}
