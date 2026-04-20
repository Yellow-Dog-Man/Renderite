using Renderite.Shared;
using SharpDX.Direct3D11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using NativeGraphics.NET;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class TextureAssetBase : Asset
    {
        public const int TIMESLICE_RESOLUTION = 256 * 256;

        protected abstract UnityEngine.Texture UnityTexture { get; }

        protected SharpDX.Direct3D11.Texture2D _dx11Tex;
        protected SharpDX.Direct3D11.ShaderResourceView _dx11Resource;

        protected ColorProfile? _targetProfile;

        int _totalMips;

        protected TextureFilterMode _filterMode;
        protected int _anisoLevel;

        protected Renderite.Shared.TextureFormat _format;

        protected float _mipmapBias;

        int _lastLoadedMip;

        bool _texturePropertiesDirty;

        bool _destroyed;

        protected void CheckDestroyed()
        {
            if (_destroyed)
                throw new InvalidOperationException($"Texture asset {AssetId} is destroyed.");
        }

        protected void MarkTexturePropertiesDirty() => _texturePropertiesDirty = true;

        protected abstract void SendResult(TextureUpdateResultType result, bool instanceChanged);

        protected void SetTextureFormat(TextureFormatData format)
        {
            CheckDestroyed();

            _format = format.format;
            _targetProfile = format.profile;

            void SetFormatUnity() => AssetIntegrator.EnqueueProcessing(() => SetTextureFormatUnity(format), true);

            // Currently native uploads are not supported for 3D textures
            if (format.type == TextureType.Texture3D)
            {
                SetFormatUnity();
                return;
            }

            switch (AssetIntegrator.GraphicsDeviceType)
            {
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:

                    // Skip in editor, because this causes it to freeze due to unity bug
                    if (RenderingManager.IsDebug)
                        goto default;

                    AssetIntegrator.EnqueueRenderThreadProcessing(SetTextureFormatDX11Native(format));
                    break;

                //case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                //case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                //case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                //    AssetIntegrator.EnqueueRenderThreadProcessing(SetTextureFormatOpenGLNative(format));
                //    break;

                default:
                    SetFormatUnity();
                    break;
            }
        }

        protected void SetTextureData(TextureUploadData data, bool highPriority)
        {
            CheckDestroyed();

            void EnqueueUnityUpload() => AssetIntegrator.EnqueueProcessing(() => UploadTextureDataUnity(data), highPriority);

            // Bitmap3D does not support DX11/OpenGL uploads
            if (this is Texture3DAsset)
            {
                EnqueueUnityUpload();
                return;
            }

            Renderite.Shared.TextureFormat convertToFormat;

            switch (AssetIntegrator.GraphicsDeviceType)
            {
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:

                    // Skip in editor, because this causes it to freeze due to unity bug
                    if (AssetIntegrator.IsDebugBuild)
                        goto default;

                    var dx11Format = _format.ToDX11(_targetProfile.Value, AssetIntegrator.IsUsingLinearSpace);

                    AssetIntegrator.EnqueueRenderThreadProcessing(UploadTextureDataDX11Native(data));
                    break;

                //case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                //case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                //case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                //    var openGLformat = _format.ToOpenGL(_targetProfile.Value, AssetIntegrator.IsUsingLinearSpace);

                //    AssetIntegrator.EnqueueRenderThreadProcessing(UploadTextureDataOpenGLNative(data));
                //    break;

                default:
                    // Default Unity Texture upload
                    EnqueueUnityUpload();
                    break;
            }
        }

        public void Unload()
        {
            AssetIntegrator.EnqueueProcessing(Destroy, true);

            // Remove it from the manager right away
            RemoveFromManager();
        }

        protected abstract void DoDestroy();
        protected abstract void RemoveFromManager();

        protected void Destroy()
        {
            DoDestroy();

            if (_dx11Resource != null)
            {
                var resource = _dx11Resource;
                var tex = _dx11Tex;

                AssetIntegrator.EnqueueDelayedRemoval(() =>
                    AssetIntegrator.EnqueueRenderThreadProcessing(DestroyDX11(resource, tex)));

                _dx11Tex = null;
                _dx11Resource = null;
            }

            _targetProfile = null;
        }

        IEnumerator DestroyDX11(ShaderResourceView resource, SharpDX.Direct3D11.Texture2D tex)
        {
            resource?.Dispose();
            tex?.Dispose();

            yield break;
        }

        protected abstract void DoAssignTextureProperties();

        void AssignTextureProperties()
        {
            CheckDestroyed();

            if (!_texturePropertiesDirty)
                return;

            _texturePropertiesDirty = false;

            DoAssignTextureProperties();

            //if (AssetIntegrator.GraphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 ||
            //    AssetIntegrator.GraphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
            //{
            //    AssetIntegrator.EnqueueRenderThreadProcessing(UpdateOpenGLTextureParams);
            //}
        }

        protected void UpdateTextureProperties()
        {
            CheckDestroyed();

            AssignTextureProperties();

            SendResult(TextureUpdateResultType.PropertiesSet, false);
        }

        protected abstract void DoSetTextureFormatUnity(TextureFormatData format, ref bool instanceChanged);

        void SetTextureFormatUnity(TextureFormatData format)
        {
            CheckDestroyed();

            bool instanceChanged = false;

            DoSetTextureFormatUnity(format, ref instanceChanged);

            AssignTextureProperties();

            SendResult(TextureUpdateResultType.FormatSet, instanceChanged);
        }

        protected abstract void DoUploadTextureDataUnity(TextureUploadData data);

        void UploadTextureDataUnity(TextureUploadData data)
        {
            CheckDestroyed();

            DoUploadTextureDataUnity(data);

            SendResult(TextureUpdateResultType.DataUpload, false);
        }

        #region DIRECTX 11

        protected abstract void DoGenerateUnityTextureFromDX11(TextureFormatData format);

        void GenerateUnityTextureFromDX11(TextureFormatData format)
        {
            CheckDestroyed();

            DoGenerateUnityTextureFromDX11(format);

            AssignTextureProperties();

            CompleteFormatUpdate(format, true);
        }


        IEnumerator SetTextureFormatDX11Native(TextureFormatData format)
        {
            CheckDestroyed();

            var dx11Format = format.format.ToDX11(format.profile, AssetIntegrator.IsUsingLinearSpace);
            Texture2DDescription desc = _dx11Tex?.Description ?? default(Texture2DDescription);

            bool updateUnityTexture = false;

            //var s = System.Diagnostics.Stopwatch.StartNew();

            if (_dx11Tex == null ||
                desc.Width != format.width ||
                desc.Height != format.height ||
                desc.ArraySize != format.ArraySize ||
                desc.Format != dx11Format ||
                desc.MipLevels != format.mips)
            {
                // Schedule the destruction of the old one
                if (_dx11Tex != null)
                {
                    var oldUnityTex = UnityTexture;
                    var oldDX11tex = _dx11Tex;
                    var oldDX11res = _dx11Resource;

                    format.oldCleanup = () =>
                    {
                        UnityEngine.Object.Destroy(oldUnityTex);

                        AssetIntegrator.EnqueueDelayedRemoval(() =>
                        {
                            oldDX11res?.Dispose();
                            oldDX11tex?.Dispose();
                        });
                    };
                }

                // create new texture
                desc.Width = format.width;
                desc.Height = format.height;
                desc.MipLevels = format.mips;
                desc.ArraySize = format.ArraySize;
                desc.Format = dx11Format;
                desc.SampleDescription.Count = 1;
                desc.Usage = ResourceUsage.Default;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CpuAccessFlags = CpuAccessFlags.None;
                desc.OptionFlags = format.type == TextureType.Texture2D ? ResourceOptionFlags.ResourceClamp :
                    (ResourceOptionFlags.TextureCube | ResourceOptionFlags.ResourceClamp);

                ShaderResourceViewDescription shaderDesc = default;
                shaderDesc.Format = desc.Format;
                shaderDesc.Dimension = format.type == TextureType.Texture2D ? SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D :
                    SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;

                switch (format.type)
                {
                    case TextureType.Texture2D:
                        shaderDesc.Texture2D.MipLevels = format.mips;
                        shaderDesc.Texture2D.MostDetailedMip = 0;
                        break;

                    case TextureType.Cubemap:
                        shaderDesc.TextureCube.MipLevels = format.mips;
                        shaderDesc.TextureCube.MostDetailedMip = 0;
                        break;
                }

                try
                {
                    _dx11Tex = new SharpDX.Direct3D11.Texture2D(AssetIntegrator._dx11device, desc);
                    _dx11Resource = new ShaderResourceView(AssetIntegrator._dx11device, _dx11Tex, shaderDesc);

                    _totalMips = format.mips;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Exception creating texture: Width: {desc.Width}, Height: {desc.Height}, Mips: {desc.MipLevels}, format: {dx11Format}.");
                    throw ex;
                }

                _lastLoadedMip = format.mips;

                updateUnityTexture = true;
            }

            // Check if we still need to generate an unity texture first
            if (updateUnityTexture)
                AssetIntegrator.EnqueueProcessing(() => GenerateUnityTextureFromDX11(format), true);
            else if (_texturePropertiesDirty)
                AssetIntegrator.EnqueueProcessing(() =>
                {
                    AssignTextureProperties();
                    CompleteFormatUpdate(format);
                }, true);
            else
                CompleteFormatUpdate(format); // just updated the data, we are done now

            yield break;
        }

        void CompleteFormatUpdate(TextureFormatData format, bool instanceChanged = false)
        {
            CheckDestroyed();

            format.oldCleanup?.Invoke();

            SendResult(TextureUpdateResultType.FormatSet, format.oldCleanup != null || instanceChanged);
        }

        IEnumerator UploadTextureDataDX11Native(TextureUploadData data)
        {
            CheckDestroyed();

            int elements;

            switch(data.type)
            {
                case TextureType.Texture2D:
                    elements = 1;
                    break;

                case TextureType.Cubemap:
                    elements = 6;
                    break;

                case TextureType.Texture3D:
                    throw new NotSupportedException("Texture3D upload via DX11 isn't currently supported");

                default:
                    throw new ArgumentException("Invalid texture type");
            }

            var hint = data.hint2D;
            var faceSize = data.FaceSize;
            var format = _format;
            var totalMipMaps = _totalMips;

            var width = hint.region?.width ?? faceSize.x;
            var height = hint.region?.height ?? faceSize.y;
            var startX = hint.region?.x ?? 0;
            var startY = hint.region?.y ?? 0;

            var blockSize = format.BlockSize();
            var bitsPerPixel = format.GetBitsPerPixel();

            if (width > 0 && height > 0)
            {
                // Upload texture data
                for (int mip = 0; mip < data.MipMapCount; mip++)
                {
                    for (int face = 0; face < elements; face++)
                    {
                        var levelSize = data.MipMapSize(mip);
                        var targetMip = data.startMip + mip;

                        width = Math.Min(width, levelSize.x - startX);
                        height = Math.Min(height, levelSize.y - startY);

                        var mipWidth = MathHelper.AlignSize(levelSize.x, blockSize.x);
                        var mipHeight = MathHelper.AlignSize(levelSize.y, blockSize.y);

                        width = MathHelper.AlignSize(width, blockSize.x);
                        height = MathHelper.AlignSize(height, blockSize.y);

                        //UnityEngine.Debug.Log($"[{AssetId}]. Uploading Mip: {mip}. Width: {width}, Height: {height}, " +
                        //    $"MipWidth: {mipWidth}, MipHeight: {mipHeight}, StartX: {startX}, StartY: {startY}, LevelSize: {levelSize}, Format: {format}");

                        //mipSize = Bitmap2D.AlignSize(mipSize, data.Format);

                        // Compute how many rows to upload per slice, should be multiple of 4
                        var rowGranularity = TIMESLICE_RESOLUTION / width;
                        rowGranularity -= rowGranularity % 4;
                        rowGranularity = Math.Max(4, rowGranularity);

                        int row = 0;

                        var rowPitch = (int)(MathHelper.BitsToBytes((long)mipWidth * bitsPerPixel) * blockSize.y);

                        while (row < height)
                        {
                            // if it's not the first row, wait before the next upload, giving the engine a chance to skip
                            // and wait for the next frame before continuing
                            // This will also implicitly upload all mip levels with number of pixels smaller than the timeslice
                            // amount without yielding, which is desirable
                            if (row > 0)
                            {
                                /*s.Stop();
                                UniLog.Log("Time-slice uploaded in: " + s.GetElapsedMilliseconds());*/
                                yield return null;

                                CheckDestroyed();
                                //s.Restart();
                            }

                            ResourceRegion? region = null;

                            region = new ResourceRegion(startX, startY + row, 0, startX + width, Math.Min(startY + row + rowGranularity, startY + height), 1);

                            // exclude region if it's not necessary
                            // IMPORTANT!!! The GPU often hangs at small mipmaps for block compressed textures when passed region
                            // so make sure it's never passed for those small ones
                            if (region.Value.Left == 0 && region.Value.Top == 0 && region.Value.Right == mipWidth && region.Value.Bottom == mipHeight)
                                region = null;

                            int rowYstart = startY + row;

                            if (data.type == TextureType.Texture2D)
                                rowYstart = levelSize.y - rowYstart - 1;

                            var byteStart = (int)MathHelper.BitsToBytes((long)data.PixelStart(startX, rowYstart, mip, face) * bitsPerPixel);

                            var rawData = data.data.RawData;

                            /*UnityEngine.Debug.Log($"[{AssetId}]. ByteStart: {byteStart}, TargetMip: {targetMip}, " +
                                $"Face: {face}, RowPitch: {rowPitch}, Region. Left: {region?.Left}, " +
                                $"Right: {region?.Right}, Bottom: {region?.Bottom}, Front: {region?.Front}, " +
                                $"Top: {region?.Top}");*/

                            AssetIntegrator._dx11device.ImmediateContext.UpdateSubresource(
                                ref rawData[byteStart], _dx11Tex, targetMip + face * totalMipMaps, rowPitch: rowPitch, region: region);

                            row += rowGranularity;

                            RenderingManager.Instance.Stats.TextureSliceUpdated();
                        }
                    }

                    // Divide by 2
                    width >>= 1;
                    height >>= 1;
                    startX >>= 1;
                    startY >>= 1;

                    width = Math.Max(width, 1);
                    height = Math.Max(height, 1);
                }

                _lastLoadedMip = Math.Min(_lastLoadedMip, data.startMip);

                _dx11Tex.Device.ImmediateContext.SetMinimumLod(_dx11Tex, _lastLoadedMip);
            }

            RenderingManager.Instance.Stats.TextureUpdated();

            SendResult(TextureUpdateResultType.DataUpload, false);
        }

        #endregion

        //#region OpenGL

        //int _GL_width;
        //int _GL_height;
        //int _GL_mips;
        //OpenGL_TextureFormat _GL_format;

        //uint _GL_texId;

        //protected abstract void DoGenerateUnityTextureFromOpenGL(uint texId, TextureFormatData format);

        //void GenerateUnityTextureFromOpenGL(uint texId, TextureFormatData format)
        //{
        //    DoGenerateUnityTextureFromOpenGL(texId, format);

        //    AssignTextureProperties();

        //    format.onDone(true);
        //}

        //protected abstract void UpdateOpenGLTextureParams();

        //protected void UpdateOpenGLTextureParams(OpenGL.TextureTarget bindTarget,
        //    Renderite.Shared.TextureWrapMode wrapU,
        //    Renderite.Shared.TextureWrapMode wrapV)
        //{
        //    OpenGL.GL_BindTexture(bindTarget, _GL_texId);
        //    OpenGL_PrintError($"BindTexture {bindTarget} - {_GL_texId} - UpdateTexParams");

        //    OpenGL.ParameterName wrapS = wrapU.ToOpenGL();
        //    OpenGL.ParameterName wrapT = wrapV.ToOpenGL();
        //    OpenGL.ParameterName minFilter;
        //    OpenGL.ParameterName magFilter;

        //    switch (_filterMode)
        //    {
        //        case TextureFilterMode.Anisotropic:
        //        case TextureFilterMode.Trilinear:
        //            minFilter = OpenGL.ParameterName.GL_LINEAR_MIPMAP_LINEAR;
        //            magFilter = OpenGL.ParameterName.GL_LINEAR;
        //            break;

        //        case TextureFilterMode.Bilinear:
        //            minFilter = OpenGL.ParameterName.GL_LINEAR_MIPMAP_NEAREST;
        //            magFilter = OpenGL.ParameterName.GL_LINEAR;
        //            break;

        //        case TextureFilterMode.Point:
        //        default:
        //            minFilter = OpenGL.ParameterName.GL_NEAREST_MIPMAP_NEAREST;
        //            magFilter = OpenGL.ParameterName.GL_NEAREST;
        //            break;
        //    }

        //    OpenGL.GL_TexParameterInt(bindTarget, OpenGL.TextureParameterName.GL_TEXTURE_WRAP_S, (int)wrapS);
        //    OpenGL_PrintError($"Set {bindTarget} WrapS Format: {_GL_format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}");

        //    OpenGL.GL_TexParameterInt(bindTarget, OpenGL.TextureParameterName.GL_TEXTURE_WRAP_T, (int)wrapT);
        //    OpenGL_PrintError($"Set {bindTarget} WrapT Format: {_GL_format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}");

        //    OpenGL.GL_TexParameterInt(bindTarget, OpenGL.TextureParameterName.GL_TEXTURE_MIN_FILTER, (int)minFilter);
        //    OpenGL_PrintError($"Set {bindTarget} MinFilter Format: {_GL_format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}");

        //    OpenGL.GL_TexParameterInt(bindTarget, OpenGL.TextureParameterName.GL_TEXTURE_MAG_FILTER, (int)magFilter);
        //    OpenGL_PrintError($"Set {bindTarget} MagFilter Format: {_GL_format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}");

        //    OpenGL.GL_TexParameterInt(bindTarget, OpenGL.TextureParameterName.GL_TEXTURE_MIN_LOD, _lastLoadedMip);
        //    OpenGL_PrintError($"Set {bindTarget} SetMindLOD {_lastLoadedMip}. Format: {_GL_format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}");
        //}

        //IEnumerator SetTextureFormatOpenGLNative(TextureFormatData format)
        //{
        //    var elements = format.ArraySize;
        //    var isCubemap = format.type == TextureType.Cubemap;

        //    var glFormat = format.format.ToOpenGL(format.profile, AssetIntegrator.IsUsingLinearSpace);

        //    //UniLog.Log($"Texture format: {glFormat.storageFormat}, source format: {glFormat.sourceFormat}, pixel: {glFormat.pixelType}, baseFormat: {glFormat.baseFormat}, Size: {width}x{height}");

        //    bool updateUnityTexture = false;

        //    if (_GL_width != format.width ||
        //        _GL_height != format.height ||
        //        _GL_mips != format.mips ||
        //        _GL_format != glFormat)
        //    {
        //        if (_GL_texId != 0)
        //        {
        //            var oldUnityTex = _unityTexture2D;
        //            var old_GL_texId = _GL_texId;
        //            var oldOnDone = format.onDone;

        //            format.onDone = assetInstanceChanged =>
        //            {
        //                if (oldUnityTex)
        //                    UnityEngine.Object.DestroyImmediate(oldUnityTex);

        //                OpenGL.GL_DeleteTexture(old_GL_texId);

        //                oldOnDone(true);
        //            };
        //        }

        //        // create new texture
        //        _GL_texId = OpenGL.GL_GenTexture();
        //        OpenGL_PrintError($"GenTexture - Format: {glFormat.storageFormat}, Width: {format.width}, Height: {format.height}, Mips: {format.mips}");

        //        _GL_width = format.width;
        //        _GL_height = format.height;
        //        _GL_mips = format.mips;
        //        _GL_format = glFormat;

        //        var bindTarget = isCubemap ? OpenGL.TextureTarget.GL_TEXTURE_CUBE_MAP : OpenGL.TextureTarget.GL_TEXTURE_2D;
        //        OpenGL.GL_BindTexture(bindTarget, _GL_texId);
        //        OpenGL_PrintError($"BindTexture {bindTarget} - Format: {glFormat.storageFormat}, Width: {format.width}, Height: {format.height}, Mips: {format.mips}");

        //        var bitsPerPixel = format.format.GetBitsPerPixel();

        //        //for (int face = 0; face < elements; face++)
        //        //{
        //        //    OpenGL.TextureTarget texTarget;

        //        //    // make sure to always bind the texture in case it has gotten unbound in the meanwhile
        //        //    if (isCubemap)
        //        //        texTarget = OpenGL.TextureTarget.GL_TEXTURE_CUBE_MAP_POSITIVE_X + face;
        //        //    else
        //        //        texTarget = OpenGL.TextureTarget.GL_TEXTURE_2D;

        //        //    OpenGL.GL_TexStorage2D(texTarget, format.mips, glFormat.storageFormat, format.width, format.height);
        //        //    OpenGL_PrintError($"Storage2D {texTarget} - Format: {glFormat.storageFormat}, Width: {format.width}, Height: {format.height}, Mips: {format.mips}");
        //        //}

        //        OpenGL.GL_TexStorage2D(bindTarget, format.mips, glFormat.storageFormat, format.width, format.height);
        //        OpenGL_PrintError($"Storage2D {bindTarget} - Format: {glFormat.storageFormat}, Width: {format.width}, Height: {format.height}, Mips: {format.mips}");

        //        UpdateOpenGLTextureParams(bindTarget);

        //        _lastLoadedMip = format.mips;

        //        updateUnityTexture = true;
        //    }

        //    // Check if we still need to generate an unity texture first
        //    if (updateUnityTexture)
        //        AssetIntegrator.EnqueueProcessing(() => GenerateUnityTextureFromOpenGL(_GL_texId, format), true);
        //    else if (_texturePropertiesDirty)
        //        AssetIntegrator.EnqueueProcessing(() =>
        //        {
        //            AssignTextureProperties();
        //            format.onDone(false);
        //        }, true);
        //    else
        //        format.onDone(false); // just updated the data, we are done now

        //    yield break;
        //}

        //IEnumerator UploadTextureDataOpenGLNative(TextureUploadData data)
        //{
        //    var elements = data.ElementCount;
        //    var hint = data.hint2D;
        //    var bitmap = data.Bitmap;
        //    var faceSize = data.FaceSize;

        //    var width = hint.region?.width ?? faceSize.x;
        //    var height = hint.region?.height ?? faceSize.y;
        //    var startX = hint.region?.x ?? 0;
        //    var startY = hint.region?.y ?? 0;

        //    var blockSize = data.Format.BlockSize();

        //    var bitsPerPixel = (long)data.Format.GetBitsPerPixel();

        //    var bindTarget = data.bitmapCube != null ? OpenGL.TextureTarget.GL_TEXTURE_CUBE_MAP : OpenGL.TextureTarget.GL_TEXTURE_2D;

        //    if (width > 0 || height > 0)
        //    {
        //        var handle = GCHandle.Alloc(bitmap.RawData, GCHandleType.Pinned);
        //        var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bitmap.RawData, 0);

        //        // Upload texture data
        //        for (int mip = 0; mip < bitmap.MipMapLevels; mip++)
        //        {
        //            for (int face = 0; face < elements; face++)
        //            {
        //                var levelSize = data.MipMapSize(mip);
        //                var targetMip = data.startMip + mip;

        //                width = Math.Min(width, levelSize.x - startX);
        //                height = Math.Min(height, levelSize.y - startY);

        //                var mipSize = Bitmap2D.AlignSize(levelSize, data.Format);

        //                var size = new int2(width, height);
        //                var alignedSize = Bitmap2D.AlignSize(size, data.Format);

        //                // Compute how many rows to upload per slice, should be multiple of 4
        //                var rowGranularity = TIMESLICE_RESOLUTION / width;
        //                rowGranularity -= rowGranularity % 4;
        //                rowGranularity = Math.Max(4, rowGranularity);

        //                var rowPitch = (int)MathHelper.BitsToBytes((long)mipSize.x * (long)bitsPerPixel) * blockSize.y;

        //                int row = 0;

        //                while (row < height)
        //                {
        //                    if (row > 0)
        //                        yield return null;

        //                    OpenGL.TextureTarget texTarget;

        //                    // make sure to always bind the texture in case it has gotten unbound in the meanwhile
        //                    if (data.bitmapCube != null)
        //                        texTarget = OpenGL.TextureTarget.GL_TEXTURE_CUBE_MAP_POSITIVE_X + face;
        //                    else
        //                        texTarget = OpenGL.TextureTarget.GL_TEXTURE_2D;

        //                    OpenGL.GL_BindTexture(bindTarget, _GL_texId);
        //                    OpenGL_PrintError($"BindTexture {bindTarget} - ID: {_GL_texId}");

        //                    OpenGL.GL_PixelStoreInt((int)OpenGL.ParameterName.GL_PACK_ALIGNMENT, 1);
        //                    OpenGL.GL_PixelStoreInt((int)OpenGL.ParameterName.GL_UNPACK_ALIGNMENT, 1);

        //                    var uploadY = startY + row;

        //                    var rowYstart = uploadY;

        //                    if (data.bitmap2D != null)
        //                        rowYstart = levelSize.y - rowYstart - 1;

        //                    var byteStart = (int)MathHelper.BitsToBytes((long)data.PixelStart(startX, rowYstart, mip, face) * (long)bitsPerPixel);

        //                    var uploadHeight = rowGranularity;
        //                    var excess = (uploadY + uploadHeight) - size.y;

        //                    if (excess > 0)
        //                        uploadHeight -= excess;

        //                    var alignedUploadHeight = Bitmap2D.AlignSize(new int2(0, uploadHeight), data.Format).y;

        //                    var bytes = (int)MathHelper.BitsToBytes(alignedSize.x * alignedUploadHeight * (long)bitsPerPixel);

        //                    // sub-region upload
        //                    if (_GL_format.isCompressed)
        //                    {
        //                        OpenGL.GL_CompressedTexSubImage2D(texTarget, targetMip,
        //                            startX, uploadY, size.x, uploadHeight, _GL_format.storageFormat, bytes, dataPtr + byteStart);

        //                        OpenGL_PrintError($"Set GL_CompressedTexSubImage2D {texTarget}. Format: {_GL_format} - {data.Format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}. StartX: {startX}, StartY: {startY}, UploadY: {uploadY}, Size: {size}, UploadHeight: {uploadHeight}, Mip: {targetMip}, Height: {height}, Excess: {excess}");
        //                    }
        //                    else
        //                    {
        //                        OpenGL.GL_TexSubImage2D(texTarget, targetMip,
        //                            startX, uploadY, size.x, uploadHeight, _GL_format.baseFormat, _GL_format.pixelType, dataPtr + byteStart);

        //                        OpenGL_PrintError($"Set GL_TexSubImage2D {texTarget}. Format: {_GL_format} - {data.Format}, Width: {_GL_width}, Height: {_GL_height}, Mips: {_GL_mips}. StartX: {startX}, StartY: {startY}, UploadY: {uploadY}, Size: {size}, UploadHeight: {uploadHeight}, Mip: {targetMip}, Height: {height}, Excess: {excess}");
        //                    }

        //                    row += rowGranularity;

        //                    Engine.TextureSliceUpdated();
        //                }
        //            }

        //            width /= 2;
        //            height /= 2;
        //            startX /= 2;
        //            startY /= 2;

        //            width = Math.Max(width, 1);
        //            height = Math.Max(height, 1);
        //        }

        //        _lastLoadedMip = Math.Min(_lastLoadedMip, data.startMip);

        //        UpdateOpenGLTextureParams(bindTarget);

        //        handle.Free();
        //    }

        //    Engine.TextureUpdated();

        //    data.onDone(false);
        //}

        //[Conditional("DEBUG_OPENGL")]
        //void OpenGL_PrintError(string message)
        //{
        //    OpenGL.Error error;

        //    while ((error = OpenGL.GL_GetError()) != OpenGL.Error.GL_NO_ERROR)
        //        UnityEngine.Debug.Log(message + ": " + error);
        //}

        //#endregion
    }
}
