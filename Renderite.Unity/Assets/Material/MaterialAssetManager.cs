using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Renderite.Unity
{
    public class MaterialAssetManager
    {
        const int UNLOAD_GROUP_GRANULARITY = 100;

        public readonly AssetManager<MaterialAsset> Materials;
        public readonly AssetManager<MaterialPropertyBlockAsset> PropertyBlocks;

        List<float> _floatArray = new List<float>();
        List<Vector4> _vectorArray = new List<Vector4>();

        bool HasUnloadsToProcess => _materialsIdsToUnload.Count > 0 || _propertyBlockIdsToUnload.Count > 0;

        ConcurrentQueue<int> _materialsIdsToUnload = new ConcurrentQueue<int>();
        ConcurrentQueue<int> _propertyBlockIdsToUnload = new ConcurrentQueue<int>();

        int _unloadScheduled;

        public MaterialAssetManager()
        {
            Materials = new AssetManager<MaterialAsset>();
            PropertyBlocks = new AssetManager<MaterialPropertyBlockAsset>();
        }

        public void Handle(MaterialsUpdateBatch batch)
        {
            RenderingManager.Instance.AssetIntegrator.EnqueueProcessing(ApplyUpdate, batch, true);
        }

        public void Handle(UnloadMaterial material)
        {
            _materialsIdsToUnload.Enqueue(material.assetId);

            PackerMemoryPool.Instance.Return(material);

            TryScheduleUnload();
        }

        public void Handle(UnloadMaterialPropertyBlock propertyBlock)
        {
            _propertyBlockIdsToUnload.Enqueue(propertyBlock.assetId);

            PackerMemoryPool.Instance.Return(propertyBlock);

            TryScheduleUnload();
        }

        void TryScheduleUnload()
        {
            var previousValue = Interlocked.Exchange(ref _unloadScheduled, 1);

            if (previousValue == 0)
                RenderingManager.Instance.AssetIntegrator.EnqueueProcessing(UnloadBatch(), false);
        }

        IEnumerator UnloadBatch()
        {
            int maxToProcess = _materialsIdsToUnload.Count + _propertyBlockIdsToUnload.Count;

            int processed = 0;

            while(_materialsIdsToUnload.TryDequeue(out var materialId))
            {
                var asset = Materials.GetAsset(materialId);

                asset.Destroy();

                Materials.RemoveAsset(asset);

                if (processed++ % UNLOAD_GROUP_GRANULARITY == 0)
                    yield return null;

                if (processed > maxToProcess)
                    break;
            }

            while(_propertyBlockIdsToUnload.TryDequeue(out var propertyBlockId))
            {
                var asset = PropertyBlocks.GetAsset(propertyBlockId);
                asset.Free();

                PropertyBlocks.RemoveAsset(asset);

                if (processed++ % UNLOAD_GROUP_GRANULARITY == 0)
                    yield return null;

                if (processed > maxToProcess)
                    break;
            }

            if(HasUnloadsToProcess)
            {
                // Just enqueue it again!
                RenderingManager.Instance.AssetIntegrator.EnqueueProcessing(UnloadBatch(), false);
                yield break;
            }
            else
            {
                // Unflag it! We'll wait for more unload jobs to arrive to flag it again
                _unloadScheduled = 0;

                // If it has more to process, try schedule again in case it was missed
                if (HasUnloadsToProcess)
                    TryScheduleUnload();
            }
        }

        void ApplyUpdate(object untypedBatch)
        {
            var batch = (MaterialsUpdateBatch)untypedBatch;

            var instanceChangedRawBuffer = RenderingManager.Instance.SharedMemory.AccessData(batch.instanceChangedBuffer);
            var instanceChangedBuffer = new BitSpan(instanceChangedRawBuffer);

            // Initiate reader
            var reader = new MaterialUpdateReader(batch, instanceChangedBuffer);

            MaterialAsset targetMaterial = null;
            MaterialPropertyBlockAsset targetPropertyBlock = null;
            bool? instanceChanged = null;

            bool updatingPropertyBlocks = false;

            int updatedMaterials = 0;

            try
            {
                while (reader.HasNextUpdate)
                {
                    // Fetch the update
                    var update = reader.ReadUpdate();

                    if (update.updateType == MaterialPropertyUpdateType.SelectTarget)
                    {
                        // Switch to property blocks
                        if (updatedMaterials == batch.materialUpdateCount)
                            updatingPropertyBlocks = true;

                        updatedMaterials++;

                        // If the instance has changed, write it to the result buffer
                        if (instanceChanged != null)
                            reader.WriteInstanceChanged(instanceChanged.Value);

                        // Clear it for the next one
                        instanceChanged = false;

                        if (updatingPropertyBlocks)
                        {
                            targetPropertyBlock = PropertyBlocks.GetAsset(update.propertyID);
                            instanceChanged = targetPropertyBlock.EnsureInstance();

                            if (RenderingManager.IsDebug)
                                Debug.Log($"Targetting MaterialPropertyBlock: {targetPropertyBlock.AssetId}");
                        }
                        else
                        {
                            targetMaterial = Materials.GetAsset(update.propertyID);

                            if (RenderingManager.IsDebug)
                                Debug.Log($"Targetting Material: {targetMaterial.AssetId}. IsAllocated: {targetMaterial.Material != null}");
                        }
                    }
                    else
                    {
                        if (updatingPropertyBlocks)
                            instanceChanged |= HandlePropertyBlockUpdate(ref reader, ref update, targetPropertyBlock);
                        else
                            instanceChanged |= HandleMaterialUpdate(ref reader, ref update, targetMaterial);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogError($"Exception when applying material update.\n" +
                    $"UpdatedMaterials: {updatedMaterials}, MaterialUpdateCount: {batch.materialUpdateCount}\n" +
                    $"ReadState:\n{reader.ToString()}");

                Debug.LogError($"Material update diagnostic:\n{GenerateMaterialUpdateDiagnostic(batch)}");

                throw;
            }

            // Write the last instance changed
            if (instanceChanged != null)
                reader.WriteInstanceChanged(instanceChanged.Value);

            // Send message that the update was completed
            var updateResult = new MaterialsUpdateBatchResult();
            updateResult.updateBatchId = batch.updateBatchId;

            RenderingManager.Instance.SendMaterialUpdateResult(updateResult);

            PackerMemoryPool.Instance.Return(batch);
        }

        string GenerateMaterialUpdateDiagnostic(MaterialsUpdateBatch batch)
        {
            var str = new StringBuilder();

            try
            {
                var instanceChangedRawBuffer = RenderingManager.Instance.SharedMemory.AccessData(batch.instanceChangedBuffer);
                var instanceChangedBuffer = new BitSpan(instanceChangedRawBuffer);

                // Initiate reader
                var reader = new MaterialUpdateReader(batch, instanceChangedBuffer);

                int updatedMaterials = 0;
                bool updatingPropertyBlocks = false;

                while (reader.HasNextUpdate)
                {
                    var update = reader.ReadUpdate();

                    if(update.updateType == MaterialPropertyUpdateType.SelectTarget)
                    {
                        // Switch to property blocks
                        if (updatedMaterials == batch.materialUpdateCount)
                            updatingPropertyBlocks = true;

                        str.AppendLine($"SelectTarget. IsPropertyBlock: {updatingPropertyBlocks}, AssetID: {update.propertyID}");

                        updatedMaterials++;
                    }
                    else
                    {
                        str.AppendLine(update.ToString());

                        int length;

                        switch(update.updateType)
                        {
                            case MaterialPropertyUpdateType.SetFloat:
                                str.AppendLine("Float: " + reader.ReadFloat());
                                break;

                            case MaterialPropertyUpdateType.SetFloat4:
                                str.AppendLine("Float4: " + reader.ReadVector());
                                break;

                            case MaterialPropertyUpdateType.SetFloat4x4:
                                str.AppendLine("Float4x4: " + reader.ReadMatrix());
                                break;

                            case MaterialPropertyUpdateType.SetFloatArray:
                                length = reader.PeekInt();
                                str.AppendLine("Float array length: " + length);

                                var floatArray = reader.AccessFloatArray();

                                str.AppendLine("Float array: " + string.Join(", ", floatArray.ToArray()));
                                break;

                            case MaterialPropertyUpdateType.SetFloat4Array:
                                length = reader.PeekInt();
                                str.AppendLine("Float4 array length: " + length);

                                var float4Array = reader.AccessVectorArray();

                                str.AppendLine("Float4 array: " + string.Join(", ", float4Array.ToArray()));
                                break;

                            case MaterialPropertyUpdateType.SetTexture:
                                var packedId = reader.ReadInt();
                                str.AppendLine("PackedTextureID: " + packedId);

                                IdPacker<TextureAssetType>.Unpack(packedId, out var textureAssetId, out var textureAssetType);

                                str.AppendLine($"TextureAssetId: {textureAssetId}, AssetType: {textureAssetType}");
                                break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                str.AppendLine($"EXCEPTION: " + ex);
            }

            return str.ToString();
        }

        bool HandleMaterialUpdate(ref MaterialUpdateReader reader, ref MaterialPropertyUpdate update, MaterialAsset target)
        {
            if (update.updateType != MaterialPropertyUpdateType.SetShader && target.Material == null)
                throw new InvalidOperationException($"Material {target.AssetId} is not allocated and the first operation is not SetShader.\n" +
                    $"Operation: {update.updateType}");

            switch (update.updateType)
            {
                case MaterialPropertyUpdateType.SetShader:
                    var shaderId = update.propertyID;

                    Shader shader;

                    if (shaderId < 0)
                        shader = null;
                    else
                        shader = RenderingManager.Instance.Shaders.GetAsset(shaderId).UnityShader;

                    return target.SetShader(shader);

                case MaterialPropertyUpdateType.SetRenderQueue:
                    target.Material.renderQueue = update.propertyID;
                    return false;

                case MaterialPropertyUpdateType.SetInstancing:
                    target.Material.enableInstancing = update.propertyID > 0;
                    return false;

                case MaterialPropertyUpdateType.SetRenderType:
                    target.Material.SetOverrideTag("RenderType", ((MaterialRenderType)update.propertyID).ToString());
                    return false;

                case MaterialPropertyUpdateType.SetFloat:
                    target.Material.SetFloat(update.propertyID, reader.ReadFloat());
                    return false;

                case MaterialPropertyUpdateType.SetFloat4:
                    target.Material.SetVector(update.propertyID, reader.ReadVector());
                    return false;

                case MaterialPropertyUpdateType.SetFloat4x4:
                    target.Material.SetMatrix(update.propertyID, reader.ReadMatrix());
                    return false;

                case MaterialPropertyUpdateType.SetFloatArray:
                    var floatArray = reader.AccessFloatArray();

                    for (int i = 0; i < floatArray.Length; i++)
                        _floatArray.Add(floatArray[i]);

                    target.Material.SetFloatArray(update.propertyID, _floatArray);

                    _floatArray.Clear();
                    return false;

                case MaterialPropertyUpdateType.SetFloat4Array:
                    var vectorArray = reader.AccessVectorArray();

                    for (int i = 0; i < vectorArray.Length; i++)
                        _vectorArray.Add(vectorArray[i]);

                    target.Material.SetVectorArray(update.propertyID, _vectorArray);

                    _vectorArray.Clear();
                    return false;

                case MaterialPropertyUpdateType.SetTexture:
                    target.Material.SetTexture(update.propertyID, TextureHelper.GetTexture(reader.ReadInt()));
                    return false;

                default:
                    throw new InvalidOperationException("Invalid update type: " + update);
            }
        }

        bool HandlePropertyBlockUpdate(ref MaterialUpdateReader reader, ref MaterialPropertyUpdate update, MaterialPropertyBlockAsset target)
        {
            // IMPORTANT!!! We always trigger instance changed, because just changing the values doesn't seem to notify any of the mesh
            // renderers of this change - it can't be changed on the instance itself.
            // TODO - this should probably be reworked a bit, so we actually track which meshes use which material property blocks on each
            // one and then when we update it on the renderer, we manually re-submit just these property blocks to those specific mesh
            // renderers. This should be more efficient and avoid a roundtrip to the main process.
            // However for the initial implementation I'm leaving this pattern same as in original implementation, just to avoid
            // adding extra complexity in this.

            switch (update.updateType)
            {
                case MaterialPropertyUpdateType.SetShader:
                case MaterialPropertyUpdateType.SetInstancing:
                case MaterialPropertyUpdateType.SetRenderQueue:
                case MaterialPropertyUpdateType.SetRenderType:
                    throw new InvalidOperationException("Invalid operation for material property block: " + update.updateType);

                case MaterialPropertyUpdateType.SetFloat:
                    target.PropertyBlock.SetFloat(update.propertyID, reader.ReadFloat());
                    return true;

                case MaterialPropertyUpdateType.SetFloat4:
                    target.PropertyBlock.SetVector(update.propertyID, reader.ReadVector());
                    return true;

                case MaterialPropertyUpdateType.SetFloat4x4:
                    target.PropertyBlock.SetMatrix(update.propertyID, reader.ReadMatrix());
                    return true;

                case MaterialPropertyUpdateType.SetFloatArray:
                    var floatArray = reader.AccessFloatArray();

                    for (int i = 0; i < floatArray.Length; i++)
                        _floatArray.Add(floatArray[i]);

                    target.PropertyBlock.SetFloatArray(update.propertyID, _floatArray);

                    _floatArray.Clear();
                    return true;

                case MaterialPropertyUpdateType.SetFloat4Array:
                    var vectorArray = reader.AccessVectorArray();

                    for (int i = 0; i < vectorArray.Length; i++)
                        _vectorArray.Add(vectorArray[i]);

                    target.PropertyBlock.SetVectorArray(update.propertyID, _vectorArray);

                    _vectorArray.Clear();
                    return true;

                case MaterialPropertyUpdateType.SetTexture:
                    // For some reason, you can't SetTexture to null when it wants to be cleared, so we set it to white texture instead
                    // It's an odd limitation of Unity and we'd probably have to clear the whole property block and redo it, but we don't
                    // really have the data for that (we'd need to keep it around, which would add complexity, so this is easiest solution
                    target.PropertyBlock.SetTexture(update.propertyID, TextureHelper.GetTexture(reader.ReadInt()) ?? Texture2D.whiteTexture);
                    return true;

                default:
                    throw new InvalidOperationException("Invalid update type: " + update);
            }
        }
    }
}
