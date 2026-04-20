using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class DesktopTextureAsset : Asset
    {
        public Texture Texture => _source?.UnityTexture;

        IDisplayTextureSource _source;

        public void Handle(SetDesktopTextureProperties properties)
        {
            AssetIntegrator.EnqueueProcessing(Update, properties, false);
        }

        void Update(object untyped)
        {
            var properties = (SetDesktopTextureProperties)untyped;

            var source = RenderingManager.Instance.Display.TryGetDisplayTexture(properties.displayIndex);

            if(source != _source)
            {
                // Fee the old one
                FreeSource();

                if(source != null)
                {
                    _source = source;
                    _source.RegisterRequest(TextureUpdated);
                }

                TextureUpdated();
            }

            PackerMemoryPool.Instance.Return(properties);
        }

        void FreeSource()
        {
            var source = _source;
            _source = null;

            if (source != null)
                AssetIntegrator.EnqueueProcessing(() => source.UnregisterRequest(TextureUpdated), true);
        }

        void TextureUpdated()
        {
            var message = new DesktopTexturePropertiesUpdate();

            message.assetId = AssetId;
            message.size = new RenderVector2i(Texture?.width ?? 0, Texture?.height ?? 0);

            RenderingManager.Instance.SendAssetUpdate(message);
        }

        public void Unload()
        {
            RenderingManager.Instance.DesktopTextures.RemoveAsset(this);

            AssetIntegrator.EnqueueProcessing(FreeSource, true);
        }
    }
}
