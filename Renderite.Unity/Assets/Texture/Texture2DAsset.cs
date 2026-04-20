using NativeGraphics.NET;
using Renderite.Shared;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Renderite.Unity
{
    public class Texture2DAsset : TextureAssetBase
    {
        public Texture2D Texture { get; private set; }

        protected override Texture UnityTexture => Texture;

        Renderite.Shared.TextureWrapMode _wrapU;
        Renderite.Shared.TextureWrapMode _wrapV;

        public void SetFormat(SetTexture2DFormat format)
        {
            var options = new TextureFormatData();

            options.type = TextureType.Texture2D;
            options.width = format.width;
            options.height = format.height;
            options.depth = 1;
            options.mips = format.mipmapCount;
            options.format = format.format;
            options.profile = format.profile;

            PackerMemoryPool.Instance.Return(format);

            SetTextureFormat(options);
        }

        public void SetProperties(SetTexture2DProperties properties)
        {
            MarkTexturePropertiesDirty();

            _filterMode = properties.filterMode;
            _anisoLevel = properties.anisoLevel;
            _wrapU = properties.wrapU;
            _wrapV = properties.wrapV;
            _mipmapBias = properties.mipmapBias;

            // only update in place if this is update for already created texture
            if (properties.applyImmediatelly)
                AssetIntegrator.EnqueueProcessing(UpdateTextureProperties, properties.highPriority);

            PackerMemoryPool.Instance.Return(properties);
        }

        public void SetData(SetTexture2DData data)
        {
            var uploadData = new TextureUploadData();

            uploadData.type = TextureType.Texture2D;
            uploadData.data = RenderingManager.Instance.SharedMemory.AccessSlice(data.data);
            uploadData.startMip = data.startMipLevel;
            uploadData.hint2D = data.hint;
            uploadData.mipMapSizes = data.mipMapSizes;
            uploadData.flipY = data.flipY;

            // It only has one face, so we just make the list implicitly
            uploadData.mipStarts = new List<List<int>>();
            uploadData.mipStarts.Add(data.mipStarts);

            SetTextureData(uploadData, data.highPriority);
        }

        protected override void DoAssignTextureProperties()
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Assigning Texture Properties for: {AssetId}, Texture: {Texture}");

            if (_filterMode == TextureFilterMode.Anisotropic)
            {
                Texture.filterMode = UnityEngine.FilterMode.Trilinear;
                Texture.anisoLevel = _anisoLevel;
            }
            else
            {
                Texture.filterMode = _filterMode.ToUnity();
                Texture.anisoLevel = 0;
            }

            Texture.wrapModeU = _wrapU.ToUnity();
            Texture.wrapModeV = _wrapV.ToUnity();
            Texture.mipMapBias = _mipmapBias;
        }

        protected override void DoGenerateUnityTextureFromDX11(TextureFormatData format)
        {
            if(_dx11Resource == null)
                throw new InvalidOperationException($"DX11 resource is null on texture {AssetId}");

            if (_dx11Resource.IsDisposed)
                throw new InvalidOperationException($"DX11 resource was disposed on texture {AssetId}");

            if(_dx11Resource.NativePointer == IntPtr.Zero)
                throw new InvalidOperationException($"DX11 resource native pointer is zero on texture {AssetId}");

            /*Debug.Log($"Uploading Texture2D {AssetId}. {format.width}x{format.height}, Mips: {format.mips}, " +
                $"Format: {format.format}, Pointer: {_dx11Resource.NativePointer}");*/

            Texture = UnityEngine.Texture2D.CreateExternalTexture(format.width, format.height,
                format.format.ToUnity(), format.mips > 1, false, _dx11Resource.NativePointer);

            if (Texture == null)
                Debug.LogWarning($"Failed to create Unity texture from native.\n" +
                    $"Size: {format.width} x {format.height}, Format: {format.format}, Mips: {format.mips}, Pointer: {_dx11Resource.NativePointer}");
        }

        protected override void DoUploadTextureDataUnity(TextureUploadData data)
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Uploading Texture Data for: {AssetId}. Texture: {Texture}");

            var width = Texture.width;
            var height = Texture.height;

            int offset = 0;

            var blockSize = _format.BlockSize();

            for (int i = 0; i < data.startMip; i++)
            {
                var alignedWidth = MathHelper.AlignSize(width, blockSize.x);
                var alignedHeight = MathHelper.AlignSize(height, blockSize.y);

                offset += alignedWidth * alignedHeight;

                width = Math.Max(1, (width >> 1));
                height = Math.Max(1, (height >> 1));
            }

            offset = MathHelper.PixelsToBytes(offset, _format);

            var texData = Texture.GetRawTextureData<byte>();
            var rawData = data.data.RawData;

            if (RenderingManager.IsDebug)
                Debug.Log($"Texture: {AssetId} ({width}x{height}), Mips: {Texture.mipmapCount}, Offset: {offset}. Format: {_format}. RawData.Length: {rawData.Length}. TexData.Length: {texData.Length}");

            for (int i = 0; i < rawData.Length; i++)
                texData[i + offset] = rawData[i];

            if (data.startMip == 0)
                Texture.Apply(false, !data.hint2D.readable);
        }

        protected override void DoSetTextureFormatUnity(TextureFormatData format, ref bool instanceChanged)
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Setting texture format for: {AssetId}, Texture: {Texture}");

            var unityFormat = format.format.ToUnity();

            // first create an empty texture with appropriate format and size
            // try to reuse the existing one if there's one
            if (Texture == null ||
                Texture.width != format.width ||
                Texture.height != format.height ||
                Texture.format != unityFormat ||
                (Texture.mipmapCount > 1) != (format.mips > 1))
            {
                // destroy the old one first if it exists
                Destroy();

                Texture = new UnityEngine.Texture2D(format.width, format.height, unityFormat, format.mips > 1);

                instanceChanged = true;
            }
        }

        protected override void DoDestroy()
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Destroying Texture: {AssetId}, Texture: {Texture}");

            if (Texture != null)
            {
                UnityEngine.Object.Destroy(Texture);
                Texture = null;
            }
        }

        protected override void SendResult(TextureUpdateResultType type, bool instanceChanged)
        {
            var result = new SetTexture2DResult();

            result.assetId = AssetId;
            result.type = type;
            result.instanceChanged = instanceChanged;

            RenderingManager.Instance.SendAssetUpdate(result);
        }

        protected override void RemoveFromManager()
        {
            RenderingManager.Instance.Texture2Ds.RemoveAsset(this);
        }

        //protected override void DoGenerateUnityTextureFromOpenGL(uint texId, TextureFormatData format)
        //{
        //    Texture = UnityEngine.Texture2D.CreateExternalTexture(format.width, format.height,
        //            format.format.ToUnity(), format.mips > 1, false, new IntPtr(texId));
        //}

        //protected override void UpdateOpenGLTextureParams()
        //{
        //    UpdateOpenGLTextureParams(OpenGL.TextureTarget.GL_TEXTURE_2D, _wrapU, _wrapV);
        //}
    }
}
