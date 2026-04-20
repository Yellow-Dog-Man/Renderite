using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class TransformManager
    {
        public const string FORCE_LAYER = "FORCE_LAYER";

        public struct TransformData
        {
            public TransformData(Transform transform)
            {
                this.transform = transform;
                this.inUse = false;  // initially not in use
            }

            public Transform transform;
            public bool inUse;
        }

        public RenderSpace Space { get; private set; }
        public Transform Root { get; private set; }

        public Transform this[int transformId] => transforms[transformId].transform;

        public TransformData GetTransformData(int transformId) => transforms[transformId];

        public void MarkInUse(int transformId)
        {
            var data = transforms[transformId];

            if(data.inUse)
                throw new InvalidOperationException("This transform is already in use.");

            data.inUse = true;

            transforms[transformId] = data;
        }

        public void ClearInUse(int transformId)
        {
            var data = transforms[transformId];

            if(!data.inUse)
                throw new InvalidOperationException("This transform is not in use.");

            data.inUse = false;

            transforms[transformId] = data;
        }

        List<TransformData> transforms = new List<TransformData>();

        public TransformManager(RenderSpace space, Transform root)
        {
            this.Space = space;
            this.Root = root;
        }

        public void HandleUpdate(TransformsUpdate update)
        {
            if(!update.removals.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(update.removals);

                for (int i = 0; i < updates.Length; i++)
                {
                    var removedIndex = updates[i];

                    // Check if we reached the end. It's possible that the whole buffer is not filled
                    if (removedIndex < 0)
                        break;

                    if (RenderingManager.IsDebug)
                    {
                        for (int ch = 0; ch < transforms[i].transform.childCount; ch++)
                            transforms[i].transform.GetChild(ch).name += $"-D:{removedIndex}";
                    }

                    var toRemove = transforms[removedIndex];

                    toRemove.transform.DetachChildren();

                    UnityEngine.Object.Destroy(toRemove.transform.gameObject);

                    transforms[removedIndex] = transforms[transforms.Count - 1];
                    transforms.RemoveAt(transforms.Count - 1);
                }
            }

            while (transforms.Count < update.targetTransformCount)
                transforms.Add(new TransformData(AlocateTransform(Space.Id, transforms.Count)));

            if(!update.parentUpdates.IsEmpty)
            {
                // Update parents
                var updates = RenderingManager.Instance.SharedMemory.AccessData(update.parentUpdates);

                // First reset the parents for all of them
                // This is important, because it's possible that the parents end up being swapped
                for (int i = 0; i < updates.Length; i++)
                {
                    var u = updates[i];

                    if (u.transformId < 0)
                        break;

                    transforms[u.transformId].transform.SetParent(null, false);

                    if (RenderingManager.IsDebug)
                        transforms[u.transformId].transform.name += "-P:null";
                }

                for(int i = 0; i < updates.Length; i++)
                {
                    var u = updates[i];

                    if (u.transformId < 0)
                        break;

                    var t = transforms[u.transformId];
                    var p = transforms[u.newParentId];

                    t.transform.SetParent(p.transform);

                    // Unless the layer is being forced on the transform, inherit the parent one
                    if (t.transform.gameObject.layer != p.transform.gameObject.layer)
                        LayerRenderable.SetLayerRecursively(t.transform, p.transform.gameObject.layer);

                    if (RenderingManager.IsDebug)
                        transforms[u.transformId].transform.name += $"-P:{u.newParentId}";
                }
            }

            if(!update.poseUpdates.IsEmpty)
            {
                var updates = RenderingManager.Instance.SharedMemory.AccessData(update.poseUpdates.As<UnityTransformPoseUpdate>());

                for(int i = 0; i < updates.Length; i++)
                {
                    var u = updates[i];

                    if (u.transformId < 0)
                        break;

                    var t = transforms[u.transformId].transform;

                    t.localPosition = u.pose.position;
                    t.localRotation = u.pose.rotation;
                    t.localScale = u.pose.scale;
                }
            }
        }

        Transform AlocateTransform(int renderSpaceId, int transformId)
        {
            var go = new GameObject(RenderingManager.IsDebug ? $"{renderSpaceId}:{transformId}" : "");
            var transform = go.transform;

            go.layer = Space.DefaultLayer;

            transform.SetParent(Root, false);

            return transform;
        }
    }
}
