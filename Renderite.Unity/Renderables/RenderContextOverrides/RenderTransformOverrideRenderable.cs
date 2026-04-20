using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class RenderTransformOverrideRenderable : RenderContextOverride
    {
        static HashSet<SkinnedMeshRenderable> _existing = new HashSet<SkinnedMeshRenderable>();

        Vector3? _targetPosition;
        Quaternion? _targetRotation;
        Vector3? _targetScale;

        Vector3? _originalPosition;
        Quaternion? _originalRotation;
        Vector3? _originalScale;

        HashSet<SkinnedMeshRenderable> _registeredSkinnedRenderables;

        bool renderersDirty;

        List<SkinnedMeshRenderable> _skinnedMeshesToRegister;

        public void ApplyState(ref RenderTransformOverrideState state, UnmanagedSpan<int> skinnedMeshRenderers)
        {
            BeginUpdateSetup(state.context);

            if(state.skinnedMeshRendererCount >= 0)
            {
                if (_skinnedMeshesToRegister == null && state.skinnedMeshRendererCount > 0)
                    _skinnedMeshesToRegister = new List<SkinnedMeshRenderable>();

                _skinnedMeshesToRegister?.Clear();

                for(int i = 0; i < state.skinnedMeshRendererCount; i++)
                {
                    var renderableIndex = skinnedMeshRenderers[i];

                    if (renderableIndex < 0)
                        continue;

                    var skin = Space.SkinnedMeshes[renderableIndex];

                    _skinnedMeshesToRegister.Add(skin);
                }

                renderersDirty = true;
            }

            if (_registeredContext == null)
            {
                // clear all registered recalc requests if we are inactive
                ClearRecalcRequests();

                // Remark the renderers as dirty, because we'll want them to be processed next time
                renderersDirty = true;
            }

            _targetPosition = state.PositionOverride?.ToUnity();
            _targetRotation = state.RotationOverride?.ToUnity();
            _targetScale = state.ScaleOverride?.ToUnity();

            FinishUpdateSetup();
        }

        protected override void Override()
        {
            if (renderersDirty)
            {
                if(_registeredSkinnedRenderables != null)
                    foreach (var skinned in _registeredSkinnedRenderables)
                        _existing.Add(skinned);

                if(_skinnedMeshesToRegister != null)
                    foreach (var renderer in _skinnedMeshesToRegister)
                    {
                        if (renderer == null)
                            continue;

                        if (_existing.Remove(renderer))
                            continue; // it's already flagged
                        else
                        {
                            if (_registeredSkinnedRenderables == null)
                                _registeredSkinnedRenderables = new HashSet<SkinnedMeshRenderable>();

                            renderer.RequestForceRecalcPerRender(this);
                            _registeredSkinnedRenderables.Add(renderer);
                        }
                    }

                // Remove any that are missing
                foreach (var toRemove in _existing)
                {
                    toRemove.RemoveRequestForceRecalcPerRender(this);
                    _registeredSkinnedRenderables.Remove(toRemove);
                }

                _existing.Clear();

                // Only unflag it if all of them were processed
                renderersDirty = false;
            }

            var t = Transform;

            // store originals
            if (_targetPosition != null)
            {
                _originalPosition = t.localPosition;
                t.localPosition = _targetPosition.Value;
            }
            else
                _originalPosition = null;

            if (_targetRotation != null)
            {
                _originalRotation = t.localRotation;
                t.localRotation = _targetRotation.Value;
            }
            else
                _originalRotation = null;

            if (_targetScale != null)
            {
                _originalScale = t.localScale;
                t.localScale = _targetScale.Value;
            }
            else
                _originalScale = null;
        }

        protected override void Restore()
        {
            var t = Transform;

            // Restore original
            if (_originalPosition != null)
                t.localPosition = _originalPosition.Value;

            if (_originalRotation != null)
                t.localRotation = _originalRotation.Value;

            if (_originalScale != null)
                t.localScale = _originalScale.Value;
        }

        protected override void Cleanup()
        {
            base.Cleanup();

            ClearRecalcRequests();
        }

        void ClearRecalcRequests()
        {
            // Remove all the registered requests
            if (_registeredSkinnedRenderables != null)
            {
                foreach (var skinned in _registeredSkinnedRenderables)
                    skinned.RemoveRequestForceRecalcPerRender(this);

                _registeredSkinnedRenderables.Clear();
            }
        }
    }
}
