using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public struct EmptyUpdateData
    {

    }

    public abstract class RenderableStateChangeManager<TRenderable, TUpdate, TState, TUpdateData> : RenderableManager<TRenderable, TUpdate>
        where TRenderable : Renderable, new()
        where TUpdate : RenderablesStateUpdate<TState>
        where TState : unmanaged
        where TUpdateData : unmanaged
    {
        protected RenderableStateChangeManager(RenderSpace space) : base(space)
        {
        }

        protected override TRenderable AllocateRenderable(Transform rootTransform, bool isInUse)
        {
            var renderable = new TRenderable();
            renderable.Setup(Space, rootTransform, !isInUse);
            return renderable;
        }

        protected override void ApplyUpdate(TUpdate updateBatch)
        {
            if(!updateBatch.states.IsEmpty)
            {
                var updateData = InitUpdateData(updateBatch);

                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.states);

                for(int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    var index = GetRenderableIndex(ref update);

                    if (index < 0)
                        break;

                    var renderable = this[index];

                    if (renderable == null)
                        throw new Exception($"Renderable at index {index} is null! Update batch states length: {updateBatch.states.length}. " +
                            $"Manager: {GetType().FullName}");

                    ApplyState(ref update, renderable, ref updateData, updateBatch);
                }
            }
        }

        protected abstract TUpdateData InitUpdateData(TUpdate batch);
        protected abstract int GetRenderableIndex(ref TState state);
        protected abstract void ApplyState(ref TState update, TRenderable handler, ref TUpdateData updateData, TUpdate batch);
    }
}
