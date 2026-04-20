using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class TrailsRenderBufferAsset : RenderBufferAssetBase<TrailsRenderBufferAsset, TrailRenderBufferUpload, TrailRenderBufferConsumed>
    {
        public void HandleUnload(TrailRenderBufferUnload unload)
        {
            Unload();

            RenderingManager.Instance.TrailsRenderBuffers.RemoveAsset(this);

            PackerMemoryPool.Instance.Return(unload);
        }
    }
}
