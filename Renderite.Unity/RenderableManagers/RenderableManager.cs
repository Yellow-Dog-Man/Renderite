using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class RenderableManager<TRenderable, TUpdate>
        where TRenderable : Renderable
        where TUpdate : RenderablesUpdate
    {
        public RenderSpace Space { get; private set; }

        public RenderableManager(RenderSpace space)
        {
            Space = space;
        }

        public int RenderableCount => renderables.Count;
        public TRenderable this[int renderableIndex] => renderables[renderableIndex];

        List<TRenderable> renderables = new List<TRenderable>();

        public void HandleUpdate(TUpdate update)
        {
            // Handle additions and removals first
            if(!update.removals.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(update.removals);

                for(int i = 0; i < updates.Length; i++)
                {
                    var removedIndex = updates[i];

                    if (removedIndex < 0)
                        break;

                    var renderable = renderables[removedIndex];
                    renderable.Index = -1;

                    // TODO!!! Mark the transform to not be in use
                    // This is a bit trickier, because its index might have changed in the meanwhile and we'll need to remap it
                    // Maybe actually just add a reference to "In-use" reference to the struct? So it can then assign new index
                    // when it gets remapped?
                    // It's okay to keep it marked as in-use, it just means subsequent additions will be a bit less efficient

                    // Let the renderable handle its own cleanup
                    renderable.Remove();

                    // Swap the last one there
                    renderables[removedIndex] = renderables[renderables.Count - 1];
                    renderables[removedIndex].Index = removedIndex;

                    // And remove the last one
                    renderables.RemoveAt(renderables.Count - 1);
                }
            }

            if(!update.additions.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(update.additions);

                for(int i = 0; i < updates.Length; i++)
                {
                    var transformId = updates[i];

                    if (transformId < 0)
                        break;

                    // Get the transform it's supposed to be attached to
                    var data = Space.Transforms.GetTransformData(transformId);

                    // Mark it in use
                    if (!data.inUse)
                        Space.Transforms.MarkInUse(transformId);

                    if (data.transform == null)
                        throw new Exception($"TransformId: {transformId} is null! InUse: {data.inUse}. Renderable type handler: {GetType().FullName}");

                    var renderable = AllocateRenderable(data.transform, data.inUse);
                    renderable.Index = renderables.Count;

                    renderables.Add(renderable);
                }
            }

            // Now that all the allocations are done, we can apply the actual updates
            ApplyUpdate(update);
        }

        protected abstract void ApplyUpdate(TUpdate updateBatch);

        protected abstract TRenderable AllocateRenderable(Transform rootTransform, bool isInUse);
    }
}
