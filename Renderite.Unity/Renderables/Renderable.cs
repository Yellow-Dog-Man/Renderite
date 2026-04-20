using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class Renderable
    {
        public bool IsDirect => SubTransform != null;
        public int Index { get; internal set; }

        public virtual bool DirectOnly => false;

        public RenderSpace Space { get; private set; }
        public Transform Transform { get; private set; }
        public Transform SubTransform { get; private set; }

        public Transform ActualTransform => SubTransform ?? Transform;

        public void Setup(RenderSpace space, Transform transform, bool direct)
        {
            Space = space;
            Transform = transform;

            if(!direct && !DirectOnly)
            {
                var go = new GameObject("");
                go.transform.SetParent(transform, false);
                go.layer = transform.gameObject.layer;

                SubTransform = go.transform;
            }

            Setup(SubTransform ?? Transform);
        }

        public void Remove(bool removingSpace = false)
        {
            Cleanup();

            // We can skip the rest if we're removing the whole space, because the entire hierarchy will be destroyed
            if (removingSpace)
                return;

            if (SubTransform != null)
            {
                UnityEngine.Object.Destroy(SubTransform.gameObject);
                SubTransform = null;
            }
        }

        protected abstract void Setup(Transform root);
        protected abstract void Cleanup();
    }
}
