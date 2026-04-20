using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class SkinnedMeshRendererManager : MeshRendererManager<SkinnedMeshRenderable, SkinnedMeshRenderablesUpdate>
    {
        public SkinnedMeshRendererManager(RenderSpace space) : base(space)
        {
        }

        protected override SkinnedMeshRenderable AllocateRenderable(Transform rootTransform, bool isInUse)
        {
            var mesh = new SkinnedMeshRenderable();

            mesh.Setup(Space, rootTransform, !isInUse);

            return mesh;
        }

        protected override void ApplyUpdate(SkinnedMeshRenderablesUpdate updateBatch)
        {
            base.ApplyUpdate(updateBatch);

            if (RenderingManager.IsDebug)
                Debug.Log($"Skinned mesh update. Bounds: {updateBatch.boundsUpdates.length}, " +
                    $"Bones: {updateBatch.boneAssignments.length}, Blendshape: {updateBatch.blendshapeUpdates.length}");

            if (!updateBatch.boundsUpdates.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.boundsUpdates.As<UnitySkinnedMeshBoundsUpdate>());

                if (RenderingManager.IsDebug)
                    Debug.Log("Actual update count: " + updates.Length);

                for (int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    if (update.renderableIndex < 0)
                        break;

                    var skin = this[update.renderableIndex];
                    skin.Renderer.updateWhenOffscreen = false;
                    skin.Renderer.localBounds = update.localBounds;

                    if (RenderingManager.IsDebug)
                        Debug.Log($"[{i}] ({update.renderableIndex} - {skin.Transform.name} - {update.localBounds}");
                }
            }

            if(!updateBatch.realtimeBoundsUpdates.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.realtimeBoundsUpdates);

                for (int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    if (update.renderableIndex < 0)
                        break;

                    var skin = this[update.renderableIndex];
                    skin.Renderer.updateWhenOffscreen = true;
                    update.computedGlobalBounds = skin.Renderer.bounds.ToRender();
                }
            }

            if (!updateBatch.boneAssignments.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.boneAssignments);
                var boneIndexes = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.boneTransformIndexes);

                var transforms = Space.Transforms;

                Transform GetBone(int index)
                {
                    if (index < 0)
                        return null;

                    return transforms[index];
                }

                int boneIndex = 0;

                for (int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    if (update.renderableIndex < 0)
                        break;

                    var skin = this[update.renderableIndex];

                    var bones = new Transform[update.boneCount];

                    for (int b = 0; b < update.boneCount; b++)
                        bones[b] = GetBone(boneIndexes[boneIndex++]);

                    skin.Renderer.bones = bones;
                    skin.Renderer.rootBone = GetBone(update.rootBoneTransformId);

                    if (RenderingManager.IsDebug)
                        Debug.Log($"Assigning bones to {update.renderableIndex} - {skin.Transform.name}: {bones.Length}. Mesh bone count: {skin.Renderer.sharedMesh?.bindposes.Length}." +
                            $"\nBones: {string.Join(", ", bones.Select(b => b?.name))}");
                }
            }

            if (!updateBatch.blendshapeUpdateBatches.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.blendshapeUpdateBatches);
                var blendshapes = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.blendshapeUpdates);

                int blendshapeIndex = 0;

                for (int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    if (update.renderableIndex < 0)
                        break;

                    var skin = this[update.renderableIndex];

                    for (int b = 0; b < update.blendshapeUpdateCount; b++)
                    {
                        var blendshape = blendshapes[blendshapeIndex++];

                        skin.Renderer.SetBlendShapeWeight(blendshape.blendshapeIndex, blendshape.weight);
                    }
                }
            }
        }
    }
}
