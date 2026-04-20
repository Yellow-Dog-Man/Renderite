using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    /// <summary>
    /// This initializes the renderer after startup. This must be sent only once and must be the first
    /// message sent to the renderer.
    /// </summary>
    public class RendererInitData : RendererCommand
    {
        /// <summary>
        /// This is the prefix used for shared memory names. This makes sure that the names will not conflict
        /// with any other instances.
        /// </summary>
        public string sharedMemoryPrefix;

        /// <summary>
        /// Unique GUID identifying the application session. This is same for the whole run of the main process.
        /// It is used to link certain things, like WASAPI audio sessions for example
        /// </summary>
        public Guid uniqueSessionId;

        /// <summary>
        /// The ID of the main process. The renderer will use this to monitor if the main process has crashed.
        /// </summary>
        public int mainProcessId;

        /// <summary>
        /// Whether the renderer should log frame pacing information into the log for diagnostic purposes.
        /// </summary>
        public bool debugFramePacing;

        /// <summary>
        /// This is the head output device that we want to use for rendering.
        /// </summary>
        public HeadOutputDevice outputDevice;

        /// <summary>
        /// What Window title to use for the renderer
        /// </summary>
        public string windowTitle;

        /// <summary>
        /// What icon should be used for the renderer
        /// </summary>
        public SetWindowIcon setWindowIcon;

        /// <summary>
        /// Override for the splash screen. This is typically used for private universes
        /// </summary>
        public RendererSplashScreenOverride splashScreenOverride;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(sharedMemoryPrefix);
            packer.Write(uniqueSessionId);
            packer.Write(mainProcessId);
            packer.Write(debugFramePacing);
            packer.Write(outputDevice);

            packer.Write(windowTitle);

            packer.WriteObject(setWindowIcon);
            packer.WriteObject(splashScreenOverride);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref sharedMemoryPrefix);
            packer.Read(ref uniqueSessionId);
            packer.Read(ref mainProcessId);
            packer.Read(ref debugFramePacing);
            packer.Read(ref outputDevice);

            packer.Read(ref windowTitle);

            packer.ReadObject(ref setWindowIcon);
            packer.ReadObject(ref splashScreenOverride);
        }
    }
}
