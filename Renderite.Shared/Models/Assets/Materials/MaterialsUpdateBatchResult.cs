using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MaterialsUpdateBatchResult : RendererCommand
    {
        /// <summary>
        /// ID uniquely identifying the batch that this result is for. This will let the main process match it up
        /// with its own data and finalize.
        /// </summary>
        
        public int updateBatchId = -1;

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(updateBatchId);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref updateBatchId);
        }
    }
}
