using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class LayerRenderable : Renderable
    {
        public override bool DirectOnly => true;

        OverlayRootPositioner _positioner;

        protected override void Cleanup()
        {
            // Restore the layer of the parent if it's not destroyed
            if (Transform != null)
            {
                Transform.tag = "Untagged";

                if(Transform.parent != null)
                    SetLayerRecursively(Transform, Transform.parent.gameObject.layer);
            }

            if(_positioner != null)
            {
                UnityEngine.GameObject.Destroy(_positioner);
                _positioner = null;
            }
        }

        protected override void Setup(Transform root)
        {
            // Mark it to indicate that the layer is forced
            Transform.tag = TransformManager.FORCE_LAYER;

            if (RenderingManager.IsDebug)
                Debug.Log($"Forcing layer on {Transform.name}");
        }

        public void AssignLayer(LayerType type)
        {
            SetLayerRecursively(Transform, GetLayer(type));

            // TODO!!! Re-engineer this? It would be better to just separate the hierarchy out, so it doesn't need
            // to be constantly repositioned globally and compensated by the parent transform
            // This is however more work and stuff needs to be figured out (like how to make sure transform updates don't
            // screw this and move it elsewhere)
            if(type == LayerType.Overlay)
                _positioner = ActualTransform.gameObject.AddComponent<OverlayRootPositioner>();
        }

        public static int GetLayer(LayerType type)
        {
            switch(type)
            {
                case LayerType.Overlay:
                    return LayerMask.NameToLayer(RenderHelper.OVERLAY_LAYER);

                case LayerType.Hidden:
                    return LayerMask.NameToLayer(RenderHelper.HIDDEN_LAYER);

                default:
                    throw new InvalidOperationException("Invalid layer type: " + type);
            }
        }

        public static void SetLayerRecursively(Transform root, int layer, bool isStart = true)
        {
            // This is another forced layer, leave it be
            if (root.tag == TransformManager.FORCE_LAYER && !isStart)
                return;

            root.gameObject.layer = layer;

            for (int i = 0; i < root.childCount; i++)
                SetLayerRecursively(root.GetChild(i), layer, false);
        }
    }
}
