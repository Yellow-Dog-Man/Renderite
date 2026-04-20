using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Renderite.Unity
{
    public class VideoTextureAsset : Asset
    {
        public UnityEngine.Texture Texture => _playbackInstance?.Texture;

        static DateTime _lastLoad;

        VideoPlaybackManager Manager => RenderingManager.Instance.VideoPlaybackManager;

        IVideoPlaybackInstance _playbackInstance;
        CancellationTokenSource _cancellationToken;

        VideoTextureProperties _stagedProperties;

        bool _unloaded;

        public void TextureChanged()
        {
            var changed = new VideoTextureChanged();
            changed.assetId = AssetId;

            RenderingManager.Instance.SendAssetUpdate(changed);
        }

        public void Handle(VideoTextureLoad load)
        {
            if (_unloaded)
                throw new InvalidOperationException("Cannot load already unloaded video texture asset");

            if(_cancellationToken != null)
                throw new InvalidOperationException("This instance is already trying to load a video texture");

            _cancellationToken = new CancellationTokenSource();

            AssetIntegrator.EnqueueTask(() => Manager.StartCoroutine(Load(load)));
        }

        public void Handle(VideoTextureUpdate update)
        {
            _playbackInstance?.HandleUpdate(update);
        }

        public void Handle(VideoTextureProperties properties)
        {
            if (_playbackInstance != null)
                _playbackInstance.HandleProperties(properties);
            else
                _stagedProperties = properties;
        }

        public void Handle(VideoTextureStartAudioTrack audioTrack)
        {
            if (_playbackInstance == null)
                throw new InvalidOperationException("Audio can be started only after playback instance is initialized");

            _playbackInstance.StartAudio(audioTrack);
        }

        public void Unload()
        {
            _unloaded = true;

            RenderingManager.Instance.VideoTextures.RemoveAsset(this);

            // Make sure to cancel any ongoing load
            // It's possible this is null if the video texture is unloaded before load is handled
            _cancellationToken?.Cancel();

            if(_playbackInstance != null)
                UnityEngine.Object.Destroy((UnityEngine.Object)_playbackInstance);

            _playbackInstance = null;
        }

        IEnumerator Load(VideoTextureLoad load)
        {
            // IMPORTANT!!! There are bugs with the video playback engines where loading too many videos at once will crash things
            // so we add a bit of a delay to space them out and prevent crashes and issues.
            while ((DateTime.UtcNow - _lastLoad).TotalSeconds < 4)
                yield return new WaitForEndOfFrame();

            if (_cancellationToken.IsCancellationRequested)
                yield break;

            _lastLoad = DateTime.UtcNow;

            bool forceEngine;

            var enginesToTry = new List<VideoPlaybackEngine>();

            var overridenEngine = Manager.FindPlaybackEngine(load.overrideEngine);

            if (overridenEngine != null)
                enginesToTry.Add(overridenEngine);
            else
                enginesToTry.AddRange(Manager.AvailablePlaybackEngines);

            foreach(var engine in enginesToTry)
            {
                int attemptsLeft = engine.InitializationAttempts;

                for(int attempt = 0; attempt < attemptsLeft; attempt++)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        yield break;

                    var instance = engine.Instantiate(Manager.gameObject);

                    yield return instance.Setup(this, load.source, load.audioSystemSampleRate);

                    if (instance.IsLoaded && !_cancellationToken.IsCancellationRequested)
                    {
                        // We've been successful!
                        _playbackInstance = instance;

                        if (_stagedProperties != null)
                        {
                            _playbackInstance.HandleProperties(_stagedProperties);
                            _stagedProperties = null;
                        }

                        SendOnLoaded(engine.Name);

                        yield break;
                    }
                    else
                    {
                        // Cleanup before another attempt
                        UnityEngine.Object.Destroy((UnityEngine.Object)instance);
                    }
                }
            }
        }

        void SendOnLoaded(string playbackEngine)
        {
            var loaded = new VideoTextureReady();

            loaded.assetId = AssetId;

            loaded.length = _playbackInstance.Length;
            loaded.size = _playbackInstance.Size.ToRender();
            loaded.hasAlpha = _playbackInstance.HasAlpha;

            loaded.audioTracks = _playbackInstance.GetTracks();

            loaded.playbackEngine = playbackEngine;

            loaded.instanceChanged = true;

            RenderingManager.Instance.SendAssetUpdate(loaded);
        }
    }
}
