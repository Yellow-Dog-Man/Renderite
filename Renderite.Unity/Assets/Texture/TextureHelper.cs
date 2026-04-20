using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public static class TextureHelper
    {
        public static Texture GetTexture(int packedId)
        {
            if (packedId == ~0)
                return null;

            IdPacker<TextureAssetType>.Unpack(packedId, out var textureAssetId, out var textureAssetType);

            switch (textureAssetType)
            {
                case TextureAssetType.Texture2D: return RenderingManager.Instance.Texture2Ds.GetAsset(textureAssetId).Texture;
                case TextureAssetType.Texture3D: return RenderingManager.Instance.Texture3Ds.GetAsset(textureAssetId).Texture;
                case TextureAssetType.Cubemap: return RenderingManager.Instance.Cubemaps.GetAsset(textureAssetId).Texture;
                case TextureAssetType.RenderTexture: return RenderingManager.Instance.RenderTextures.GetAsset(textureAssetId).Texture;
                case TextureAssetType.VideoTexture: return RenderingManager.Instance.VideoTextures.GetAsset(textureAssetId).Texture;
                case TextureAssetType.Desktop: return RenderingManager.Instance.DesktopTextures.GetAsset(textureAssetId).Texture;

                default:
                    throw new NotImplementedException($"Unsupported texture type: {textureAssetType}");
            }
        }
    }
}
