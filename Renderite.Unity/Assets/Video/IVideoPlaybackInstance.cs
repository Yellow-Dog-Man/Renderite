using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Renderite.Unity
{
    public interface IVideoPlaybackInstance
    {
        bool IsLoaded { get; }
        IEnumerator Setup(VideoTextureAsset asset, string dataSource, int audioSystemSampleRate);

        double Length { get; }
        Vector2Int Size { get; }
        bool HasAlpha { get; }

        List<VideoAudioTrack> GetTracks();

        UnityEngine.Texture Texture { get; }

        void HandleUpdate(VideoTextureUpdate update);
        void HandleProperties(VideoTextureProperties properties);
        void StartAudio(VideoTextureStartAudioTrack audioTrack);
    }
}
