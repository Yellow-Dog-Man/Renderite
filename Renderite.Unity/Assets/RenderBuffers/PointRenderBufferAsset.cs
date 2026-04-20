using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class PointRenderBufferAsset : RenderBufferAssetBase<PointRenderBufferAsset, PointRenderBufferUpload, PointRenderBufferConsumed>
    {
        public void HandleUnload(PointRenderBufferUnload unload)
        {
            Unload();

            RenderingManager.Instance.PointRenderBuffers.RemoveAsset(this);

            PackerMemoryPool.Instance.Return(unload);
        }
    }
}
