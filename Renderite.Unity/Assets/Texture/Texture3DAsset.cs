using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Renderite.Unity
{
    public class Texture3DAsset : TextureAssetBase
    {
        public Texture3D Texture { get; private set; }

        protected override Texture UnityTexture => Texture;

        Renderite.Shared.TextureWrapMode _wrapU;
        Renderite.Shared.TextureWrapMode _wrapV;
        Renderite.Shared.TextureWrapMode _wrapW;

        public void SetFormat(SetTexture3DFormat format)
        {
            var options = new TextureFormatData();

            options.type = TextureType.Texture3D;
            options.width = format.width;
            options.height = format.height;
            options.depth = format.depth;
            options.mips = format.mipmapCount;
            options.format = format.format;
            options.profile = format.profile;

            SetTextureFormat(options);
        }

        public void SetProperties(SetTexture3DProperties properties)
        {
            MarkTexturePropertiesDirty();

            _filterMode = properties.filterMode;
            _wrapU = properties.wrapU;
            _wrapV = properties.wrapV;
            _wrapW = properties.wrapW;

            if (properties.applyImmediatelly)
                AssetIntegrator.EnqueueProcessing(UpdateTextureProperties, properties.highPriority);
        }

        public void SetData(SetTexture3DData data)
        {
            var uploadData = new TextureUploadData();

            uploadData.type = TextureType.Texture3D;
            uploadData.data = RenderingManager.Instance.SharedMemory.AccessSlice(data.data);
            uploadData.startMip = 0;
            uploadData.hint3D = data.hint;

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
            Texture.wrapModeU = _wrapU.ToUnity();
            Texture.wrapModeV = _wrapV.ToUnity();
            Texture.wrapModeW = _wrapW.ToUnity();
        }

        protected override void DoGenerateUnityTextureFromDX11(TextureFormatData format)
        {
            // 3D textures are not supported through the native DX11 upload right now
            throw new NotSupportedException();
        }

        protected override unsafe void DoUploadTextureDataUnity(TextureUploadData data)
        {
            var rawData = data.data.RawData;

            fixed(void* dataPtr = rawData)
            {
                NativeArray<byte> nativeData;

                if (RenderingManager.IsDebug)
                    nativeData = new NativeArray<byte>(rawData.ToArray(), Allocator.Persistent);
                else
                    nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(dataPtr, rawData.Length, Allocator.None);

                Texture.SetPixelData<byte>(nativeData, data.startMip);

                nativeData.Dispose();
            }

            if (data.startMip == 0)
                Texture.Apply(false, !data.hint3D.readable);
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
            var actualProfile = format.profile;

            var unityFormat = format.format.ToUnityExperimental(ref actualProfile);

            if (actualProfile != format.profile)
                _targetProfile = actualProfile;

            if (Texture == null ||
                Texture.width != format.width ||
                Texture.height != format.height ||
                Texture.depth != format.depth ||
                Texture.graphicsFormat != unityFormat ||
                (Texture.mipmapCount > 1) != (format.mips > 1) ||
                actualProfile != _targetProfile)
            {
                Destroy();

                _targetProfile = actualProfile;

                Texture = new UnityEngine.Texture3D(format.width, format.height, format.depth,
                    unityFormat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

                //UniLog.Log($"Created 3D texture with format: {unityFormat}, size: {format.width}x{format.height}x{format.depth}\n" +
                //    $"Supported: {UnityEngine.SystemInfo.IsFormatSupported(unityFormat, UnityEngine.Experimental.Rendering.FormatUsage.Sample)}");

                instanceChanged = true;
            }
        }

        protected override void SendResult(TextureUpdateResultType type, bool instanceChanged)
        {
            var result = new SetTexture3DResult();

            result.assetId = AssetId;
            result.type = type;
            result.instanceChanged = instanceChanged;

            RenderingManager.Instance.SendAssetUpdate(result);
        }

        protected override void RemoveFromManager()
        {
            RenderingManager.Instance.Texture3Ds.RemoveAsset(this);
        }

        //protected override void DoGenerateUnityTextureFromOpenGL(uint texId, TextureFormatData format)
        //{
        //    throw new NotSupportedException("Texture3D does not support native OpenGL upload");
        //}

        //protected override void UpdateOpenGLTextureParams()
        //{
        //    throw new NotSupportedException("Texture3D does not support native OpenGL upload");
        //}
    }
}
