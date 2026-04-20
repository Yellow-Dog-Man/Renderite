using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class LightRenderable : Renderable
    {
        public Light Light { get; private set; }

        public int? LastCookieAssetId;

        protected override void Cleanup()
        {
            UnityEngine.Object.Destroy(Light);
            Light = null;
        }

        protected override void Setup(Transform root)
        {
            var go = root.gameObject;
            Light = go.AddComponent<Light>();
        }
    }
}
