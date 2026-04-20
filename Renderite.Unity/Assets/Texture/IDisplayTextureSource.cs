using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public interface IDisplayTextureSource
    {
        UnityEngine.Texture UnityTexture { get; }

        void RegisterRequest(Action onTextureChanged);
        void UnregisterRequest(Action onTextureChanged);
    }
}
