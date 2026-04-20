using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class MeshRenderable : Renderable, IMeshRenderable
    {
        public MeshRenderer Renderer { get; private set; }
        public MeshFilter Filter { get; private set; }
        public Mesh SharedMesh { set => Filter.sharedMesh = value; }
        public int LastPropertyBlockCount { get; set; }

        Renderer IMeshRenderable.Renderer => Renderer;

        protected override void Cleanup()
        {
            UnityEngine.Object.Destroy(Renderer);
            UnityEngine.Object.Destroy(Filter);

            Renderer = null;
            Filter = null;
        }

        protected override void Setup(Transform root)
        {
            var go = root.gameObject;

            Filter = go.AddComponent<MeshFilter>();
            Renderer = go.AddComponent<MeshRenderer>();

            // Assign null material initially
            Renderer.sharedMaterial = RenderingManager.Instance.NullMaterial;
        }
    }
}
