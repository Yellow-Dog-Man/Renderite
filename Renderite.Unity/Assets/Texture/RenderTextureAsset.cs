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

        RenderTexture _sRGB_Readback;

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

            var source = Texture;

            // We currently always create render textures as HDR which are in linear space
            // If the readback format is not HDR, we want to perform sRGB conversion on the GPU
            if(!task.readbackFormat.IsHDR())
            {
                var requestedFormat = RenderTextureFormat.Default;

                switch (task.readbackFormat)
                {
                    case Shared.TextureFormat.RGB565:
                        requestedFormat = RenderTextureFormat.RGB565;
                        break;
                }

                if(_sRGB_Readback == null ||
                    _sRGB_Readback.format != requestedFormat ||
                    _sRGB_Readback.width != Texture.width ||
                    _sRGB_Readback.height != Texture.height)
                {
                    FreeReadback();

                    _sRGB_Readback = RenderTexture.GetTemporary(new RenderTextureDescriptor()
                    {
                        width = Texture.width,
                        height = Texture.height,
                        depthBufferBits = 0,
                        autoGenerateMips = false,
                        dimension = TextureDimension.Tex2D,
                        bindMS = false,
                        vrUsage = VRTextureUsage.None,
                        stencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
                        volumeDepth = 1,
                        shadowSamplingMode = ShadowSamplingMode.None,
                        msaaSamples = 1,

                        colorFormat = requestedFormat,
                        sRGB = true,
                    });
                }

                Graphics.Blit(Texture, _sRGB_Readback);

                source = _sRGB_Readback;
            }

            AsyncGPUReadback.RequestIntoNativeArray(ref buffer, source, 0, task.readbackFormat.ToUnity(), readbackResult =>
            {
                // Indicate if this succeeded or not
                result.success = !readbackResult.hasError;

                // Inform the engine that this has completed and they can process the read back data
                RenderingManager.Instance.SendAssetUpdate(result);
            });
        }

        void FreeReadback()
        {
            if (_sRGB_Readback != null)
                RenderTexture.ReleaseTemporary(_sRGB_Readback);
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
            FreeReadback();

            if (Texture == null)
                return;

            UnityEngine.Object.Destroy(Texture);
        }
    }
}
