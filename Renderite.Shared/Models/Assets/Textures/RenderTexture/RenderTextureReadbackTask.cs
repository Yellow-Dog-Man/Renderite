using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    /// <summary>
    /// Request to read back texture data from a given render texture. 
    /// This should use async GPU readbacks for efficiency on the renderer.
    /// It will give whatever data is currently in the render texture
    /// </summary>
    public class RenderTextureReadbackTask : AssetCommand
    {
        /// <summary>
        /// Unique ID for the readback task. Since this process is asynchronous and can take several frames
        /// and there can be multiple concurrent requests (even for the same render texture), we need to
        /// identify each one uqiquely when the read back data is returned
        /// </summary>
        public int readbackTaskId;

        /// <summary>
        /// Buffer to fill the rendered data. This represents raw bitmap data. The size MUST match
        /// the given render texture dimensions and requested readbackFormat.
        /// </summary>
        public SharedMemoryBufferDescriptor<byte> resultData;

        /// <summary>
        /// Texture format of the read back data. This can be used to read back textures as RGB24 for example
        /// when alpha channel is not needed, reducing the overall amount of bandwidth.
        /// </summary>
        public TextureFormat readbackFormat;

        public override void Pack(ref MemoryPacker packer)
        {
            base.Pack(ref packer);

            packer.Write(readbackTaskId);
            packer.Write(resultData);
            packer.Write(readbackFormat);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            base.Unpack(ref packer);

            packer.Read(ref readbackTaskId);
            packer.Read(ref resultData);
            packer.Read(ref readbackFormat);
        }
    }
}
