using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class BlitToDisplayRenderable : Renderable
    {
        public TextureDisplayBlitter Blitter { get; private set; }

        protected override void Cleanup()
        {
            Blitter?.Deinitialize();

            UnityEngine.Object.Destroy(Blitter);
        }

        protected override void Setup(Transform root)
        {
            Blitter = root.gameObject.AddComponent<TextureDisplayBlitter>();
        }
    }
}
