using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class LayerManager : RenderableManager<LayerRenderable, LayerUpdate>
    {
        List<LayerRenderable> newLayers = new List<LayerRenderable>();

        public LayerManager(RenderSpace space) : base(space)
        {
        }

        protected override LayerRenderable AllocateRenderable(Transform rootTransform, bool isInUse)
        {
            var layerRenderable = new LayerRenderable();

            layerRenderable.Setup(Space, rootTransform, !isInUse);

            newLayers.Add(layerRenderable);

            return layerRenderable;
        }

        protected override void ApplyUpdate(LayerUpdate updateBatch)
        {
            var layerData = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.layerAssignments);

            for (int i = 0; i < newLayers.Count; i++)
            {
                var layer = newLayers[i];
                layer.AssignLayer(layerData[i]);
            }

            newLayers.Clear();
        }
    }
}
