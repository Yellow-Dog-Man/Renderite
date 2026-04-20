using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    /// <summary>
    /// This is sent back by the renderer after it finishes initialization.
    /// This both indicates that it has completed, while also providing any environment details
    /// </summary>
    public class RendererInitResult : RendererCommand
    {
        /// <summary>
        /// The actual output device used. This is notably important for autodetect mode.
        /// </summary>
        
        public HeadOutputDevice actualOutputDevice;

        /// <summary>
        /// Identifier of the renderer - its name + version
        /// </summary>
        public string rendererIdentifier;

        /// <summary>
        /// Pointer to the main window handle. Assumes 64-bit pointers, since we don't use 32-bit
        /// </summary>
        public long mainWindowHandlePtr;

        /// <summary>
        /// The mode used to render stereo image when VR rendering is active
        /// </summary>
        
        public string stereoRenderingMode;

        /// <summary>
        /// Maximum texture size supported by the renderer and the hardware it runs on.
        /// The engine should not send textures bigger than this.
        /// </summary>
        
        public int maxTextureSize;

        /// <summary>
        /// 
        /// </summary>
        
        public bool isGPUTexturePOTByteAligned;

        /// <summary>
        /// List of supported texture formats. The engine should not send any formats that are not on this list.
        /// </summary>
        
        public List<TextureFormat> supportedTextureFormats;

        public override string ToString()
        {
            return $"Renderer: {rendererIdentifier} (WindowPtr: 0x{mainWindowHandlePtr:X}, ActualOutputDevice: {actualOutputDevice}, StereoRenderingMode: {stereoRenderingMode}, " +
                $"MaxTextureSize: {maxTextureSize}, IsGPUTexturePOTByteAligned: {isGPUTexturePOTByteAligned}\n"
                + "Supported texture formats: " + string.Join(", ", supportedTextureFormats);
        }

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(actualOutputDevice);
            packer.Write(rendererIdentifier);
            packer.Write(mainWindowHandlePtr);
            packer.Write(stereoRenderingMode);
            packer.Write(maxTextureSize);
            packer.Write(isGPUTexturePOTByteAligned);
            packer.WriteValueList(supportedTextureFormats);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref actualOutputDevice);
            packer.Read(ref rendererIdentifier);
            packer.Read(ref mainWindowHandlePtr);
            packer.Read(ref stereoRenderingMode);
            packer.Read(ref maxTextureSize);
            packer.Read(ref isGPUTexturePOTByteAligned);
            packer.ReadValueList(ref supportedTextureFormats);
        }
    }
}
