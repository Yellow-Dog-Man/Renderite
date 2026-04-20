using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class SkinnedMeshRenderable : Renderable, IMeshRenderable
    {
        public SkinnedMeshRenderer Renderer { get; private set; }
        public Mesh SharedMesh
        {
            set
            {
                Renderer.sharedMesh = value;

                var meshBones = value?.bindposes?.Length ?? 0;

                if (meshBones != (Renderer.bones?.Length ?? 0))
                    Renderer.bones = new Transform[meshBones];
            }
        }

        public int LastPropertyBlockCount { get; set; }

        Renderer IMeshRenderable.Renderer => Renderer;

        HashSet<Renderable> _forceRecalcRequests;

        protected override void Cleanup()
        {
            UnityEngine.Object.Destroy(Renderer);

            Renderer = null;
        }

        protected override void Setup(Transform root)
        {
            var go = root.gameObject;

            Renderer = go.AddComponent<SkinnedMeshRenderer>();

            // Assign null material initially
            Renderer.sharedMaterial = RenderingManager.Instance.NullMaterial;
        }

        public void RequestForceRecalcPerRender(Renderable requester)
        {
            if (Renderer == null)
                return;

            if (_forceRecalcRequests == null)
                _forceRecalcRequests = new HashSet<Renderable>();

            if (_forceRecalcRequests.Count == 0)
                Renderer.forceMatrixRecalculationPerRender = true;

            _forceRecalcRequests.Add(requester);
        }

        public void RemoveRequestForceRecalcPerRender(Renderable requester)
        {
            _forceRecalcRequests.Remove(requester);

            // If we already cleaned up, we can ignore this
            if (Renderer == null)
                return;

            if (_forceRecalcRequests.Count == 0)
                Renderer.forceMatrixRecalculationPerRender = false;
        }
    }
}
