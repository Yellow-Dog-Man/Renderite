using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;
using UnityEngine;

namespace Renderite.Unity
{
    public interface IMeshRenderable
    {
        Renderer Renderer { get; }
        Mesh SharedMesh { set; }
        int LastPropertyBlockCount { get; set; }
    }

    public class MeshRendererManager : MeshRendererManager<MeshRenderable, MeshRenderablesUpdate>
    {
        public MeshRendererManager(RenderSpace space) : base(space)
        {
        }

        protected override MeshRenderable AllocateRenderable(Transform rootTransform, bool isInUse)
        {
            var mesh = new MeshRenderable();

            mesh.Setup(Space, rootTransform, !isInUse);

            return mesh;
        }
    }

    public abstract class MeshRendererManager<TRenderable, TUpdate> : RenderableManager<TRenderable, TUpdate>
        where TRenderable : Renderable, IMeshRenderable
        where TUpdate : MeshRenderablesUpdate
    {
        public MeshRendererManager(RenderSpace space) : base(space)
        {
        }

        public int LastPropertyBlockCount { get; set; }

        protected override void ApplyUpdate(TUpdate updateBatch)
        {
            var meshes = RenderingManager.Instance.Meshes;
            var materialAssets = RenderingManager.Instance.Materials.Materials;
            var propertyBlockAssets = RenderingManager.Instance.Materials.PropertyBlocks;

            if (!updateBatch.meshStates.IsEmpty)
            {
                // Apply mesh states
                var updates = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.meshStates);
                var materialIndexes = RenderingManager.Instance.SharedMemory.AccessData(updateBatch.meshMaterialsAndPropertyBlocks);

                int materialIndex = 0;

                for(int i = 0; i < updates.Length; i++)
                {
                    ref var update = ref updates[i];

                    if (update.renderableIndex < 0)
                        break;

                    var meshRenderer = this[update.renderableIndex];

                    if (update.meshAssetId < 0)
                        meshRenderer.SharedMesh = null;
                    else
                        meshRenderer.SharedMesh = meshes.GetAsset(update.meshAssetId).Mesh;

                    meshRenderer.Renderer.shadowCastingMode = update.shadowCastMode.ToUnity();
                    meshRenderer.Renderer.motionVectorGenerationMode = update.motionVectorMode.ToUnity();
                    meshRenderer.Renderer.sortingOrder = update.sortingOrder;

                    // If the material count is negative, it means we should not change it in any way and keep the existing state
                    // because the materials haven't been updated. This will save some performance in such cases
                    if(update.materialCount >= 0)
                    {
                        var materials = new Material[update.materialCount];

                        for (int m = 0; m < materials.Length; m++)
                        {
                            var materialAssetId = materialIndexes[materialIndex++];

                            var asset = materialAssets.GetAsset(materialAssetId);
                            materials[m] = asset?.Material ?? RenderingManager.Instance.NullMaterial;
                        }

                        meshRenderer.Renderer.sharedMaterials = materials;

                        if (update.materialPropertyBlockCount >= 0)
                        {
                            for (int m = 0; m < update.materialPropertyBlockCount; m++)
                            {
                                var blockAssetId = materialIndexes[materialIndex++];

                                var asset = propertyBlockAssets.GetAsset(blockAssetId)?.PropertyBlock;

                                meshRenderer.Renderer.SetPropertyBlock(asset, m);
                            }

                            // Make sure we don't try to clear invalid blocks
                            meshRenderer.LastPropertyBlockCount = Math.Min(meshRenderer.LastPropertyBlockCount, update.materialCount);

                            for (int m = update.materialPropertyBlockCount; m < meshRenderer.LastPropertyBlockCount; m++)
                                meshRenderer.Renderer.SetPropertyBlock(null, m);

                            meshRenderer.LastPropertyBlockCount = update.materialPropertyBlockCount;
                        }
                    }
                }
            }
        }
    }
}
