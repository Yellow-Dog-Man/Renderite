using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Renderite.Unity
{
    public class RenderTextureAsset : Asset
    {
        public RenderTexture Texture { get; private set; }

        public void Handle(SetRenderTextureFormat format)
        {
            AssetIntegrator.EnqueueProcessing(ApplyUpdate, format, false);
        }

        public void Handle(UnloadRenderTexture unload)
        {
            AssetIntegrator.EnqueueProcessing(Destroy, false);

            // Remove it from the manager
            RenderingManager.Instance.RenderTextures.RemoveAsset(this);

            PackerMemoryPool.Instance.Return(unload);
        }

        public void Handle(RenderTextureReadbackTask task)
        {
            // Do the readback directly into the buffer to avoid unecessary copies
            var buffer = RenderingManager.Instance.SharedMemory.AccessAsNativeArray(task.resultData);

            // Capture the task data. Since this is async processing, the task will be disposed before this completes
            // so we need to capture it here
            var result = new RenderTextureReadbackResult();
            result.assetId = AssetId;
            result.readbackTaskId = task.readbackTaskId;

            AsyncGPUReadback.RequestIntoNativeArray(ref buffer, Texture, 0, task.readbackFormat.ToUnity(), readbackResult =>
            {
                // Indicate if this succeeded or not
                result.success = !readbackResult.hasError;

                // Inform the engine that this has completed and they can process the read back data
                RenderingManager.Instance.SendAssetUpdate(result);
            });
        }

        void ApplyUpdate(object untypedFormat)
        {
            var format = (SetRenderTextureFormat)untypedFormat;

            // Destroy any previous render texture
            Destroy();

            var width = Mathf.Clamp(format.size.x, 4, 8192);
            var height = Mathf.Clamp(format.size.y, 4, 8192);
            var depth = Mathf.Max(format.depth, 0);

            Texture = new RenderTexture(width, height, depth, RenderTextureFormat.ARGBHalf);
            Texture.Create();

            if (format.filterMode == TextureFilterMode.Anisotropic)
            {
                Texture.filterMode = FilterMode.Trilinear;
                Texture.anisoLevel = format.anisoLevel;
            }
            else
            {
                Texture.filterMode = format.filterMode.ToUnity();
                Texture.anisoLevel = 0;
            }

            Texture.wrapModeU = format.wrapU.ToUnity();
            Texture.wrapModeV = format.wrapV.ToUnity();

            // Send message that update was completed
            var result = new RenderTextureResult();
            result.assetId = AssetId;
            result.instanceChanged = true;

            RenderingManager.Instance.SendAssetUpdate(result);

            PackerMemoryPool.Instance.Return(format);
        }

        void Destroy()
        {
            if (Texture == null)
                return;

            UnityEngine.Object.Destroy(Texture);
        }
    }
}
