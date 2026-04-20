using NativeGraphics.NET;
using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class CubemapAsset : TextureAssetBase
    {
        public Cubemap Texture { get; private set; }

        protected override Texture UnityTexture => Texture;

        public void SetFormat(SetCubemapFormat format)
        {
            var options = new TextureFormatData();

            options.type = TextureType.Cubemap;
            options.width = format.size;
            options.height = format.size;
            options.depth = 1;
            options.mips = format.mipmapCount;
            options.format = format.format;
            options.profile = format.profile;

            SetTextureFormat(options);

            PackerMemoryPool.Instance.Return(format);
        }

        public void SetProperties(SetCubemapProperties properties)
        {
            MarkTexturePropertiesDirty();

            _filterMode = properties.filterMode;
            _anisoLevel = properties.anisoLevel;
            _mipmapBias = properties.mipmapBias;

            // only update in place if this is update for already created texture
            if (properties.applyImmediatelly)
                AssetIntegrator.EnqueueProcessing(UpdateTextureProperties, properties.highPriority);

            PackerMemoryPool.Instance.Return(properties);
        }

        public void SetData(SetCubemapData data)
        {
            var uploadData = new TextureUploadData();

            uploadData.type = TextureType.Cubemap;
            uploadData.data = RenderingManager.Instance.SharedMemory.AccessSlice(data.data);
            uploadData.startMip = data.startMipLevel;
            uploadData.mipMapSizes = data.mipMapSizes;
            uploadData.mipStarts = data.mipStarts;
            uploadData.flipY = data.flipY;

            SetTextureData(uploadData, data.highPriority);
        }

        protected override void DoAssignTextureProperties()
        {
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

            Texture.mipMapBias = _mipmapBias;
        }

        protected override void DoGenerateUnityTextureFromDX11(TextureFormatData format)
        {
            /*Debug.Log($"Uploading TextureCube {AssetId}. {format.width}x{format.height}, Mips: {format.mips}, " +
                $"Format: {format.format}, Pointer: {_dx11Resource.NativePointer}");*/

            Texture = UnityEngine.Cubemap.CreateExternalTexture(format.width,
                format.format.ToUnity(), format.mips > 1, _dx11Resource.NativePointer);

            if (Texture == null)
                Debug.LogWarning($"Failed to create Unity texture from native.\n" +
                    $"Size: {format.width} x {format.height}, Format: {format.format}, Mips: {format.mips}, Pointer: {_dx11Resource.NativePointer}");
        }

        protected override void DoDestroy()
        {
            if (Texture != null)
            {
                UnityEngine.Object.Destroy(Texture);
                Texture = null;
            }
        }

        protected override void DoSetTextureFormatUnity(TextureFormatData format, ref bool instanceChanged)
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Setting cubemap format for: {AssetId}, Texture: {Texture}");

            var unityFormat = format.format.ToUnity();

            // first create an empty texture with appropriate format and size
            // try to reuse the existing one if there's one
            if (Texture == null ||
                Texture.width != format.width ||
                Texture.format != unityFormat ||
                (Texture.mipmapCount > 1) != (format.mips > 1))
            {
                // destroy the old one first if it exists
                Destroy();

                Texture = new UnityEngine.Cubemap(format.width, unityFormat, format.mips > 1);

                instanceChanged = true;
            }
        }

        protected override void DoUploadTextureDataUnity(TextureUploadData data)
        {
            if (RenderingManager.IsDebug)
                Debug.Log($"Uploading Cubemap Data for: {AssetId}. Texture: {Texture}");

            var buffer = data.data.RawData;
            var blockSize = _format.BlockSize();

            for (int face = 0; face < 6; face++)
            {
                var mipStarts = data.mipStarts[face];

                for(int mip = 0; mip < data.MipMapCount; mip++)
                {
                    var size = MathHelper.AlignSize(data.MipMapSize(mip), blockSize);
                    var totalPixels = size.x * size.y;

                    var start = MathHelper.PixelsToBytes(mipStarts[mip], _format);
                    var length = MathHelper.PixelsToBytes(totalPixels, _format);

                    var mipData = buffer.Slice(start, length);

                    if (RenderingManager.IsDebug)
                        Debug.Log($"Uploading Cubemap {AssetId}, Face: {face}, Mip: {mip}, Format: {_format}, Start: {start}, Length: {length}, Size: {size}");

                    // Not bothering to optimize this with native arrays since it's only used in the editor for debugging
                    Texture.SetPixelData(mipData.ToArray(), data.startMip + mip, (CubemapFace)face);
                }
            }

            if (data.startMip == 0)
                Texture.Apply(false, true);
        }

        protected override void SendResult(TextureUpdateResultType type, bool instanceChanged)
        {
            var result = new SetCubemapResult();

            result.assetId = AssetId;
            result.type = type;
            result.instanceChanged = instanceChanged;

            RenderingManager.Instance.SendAssetUpdate(result);
        }

        protected override void RemoveFromManager()
        {
            RenderingManager.Instance.Cubemaps.RemoveAsset(this);
        }

        //protected override void DoGenerateUnityTextureFromOpenGL(uint texId, TextureFormatData format)
        //{
        //    Texture = UnityEngine.Cubemap.CreateExternalTexture(format.width, format.format.ToUnity(),
        //            format.mips > 1, new IntPtr(texId));
        //}

        //protected override void UpdateOpenGLTextureParams()
        //{
        //    UpdateOpenGLTextureParams(OpenGL.TextureTarget.GL_TEXTURE_CUBE_MAP, Shared.TextureWrapMode.Clamp, Shared.TextureWrapMode.Clamp);
        //}
    }
}
