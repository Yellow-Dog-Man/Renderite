using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class CameraManager : RenderableStateChangeManager<CameraRenderable, CameraRenderablesUpdate,
        CameraState, CameraManager.UpdateState>
    {
        public struct UpdateState
        {
            public int transformIndex;
            public UnmanagedSpan<int> transformIds;

            public int ReadTransformId() => transformIds[transformIndex++];
        }

        // Cache the properties
        static int _layerMask;
        static int _privateLayerMask;

        public CameraManager(RenderSpace space) : base(space)
        {
            if (_layerMask == 0)
                _layerMask = RenderHelper.PUBLIC_RENDER_MASK;

            if (_privateLayerMask == 0)
                _privateLayerMask = RenderHelper.PRIVATE_RENDER_MASK;
        }

        protected override int GetRenderableIndex(ref CameraState state) => state.renderableIndex;

        protected override void ApplyState(ref CameraState update, CameraRenderable cameraHandler, ref UpdateState updateData, CameraRenderablesUpdate batch)
        {
            var camera = cameraHandler.Camera;
            var helper = cameraHandler.Helper;

            camera.orthographic = update.projection == CameraProjection.Orthographic;
            camera.fieldOfView = update.fieldOfView;
            camera.orthographicSize = update.orthographicSize;

            helper.OrthographicSize = update.orthographicSize;
            helper.UseTransformScale = update.useTransformScale;

            helper.NearClip = update.nearClip;
            helper.FarClip = update.farClip;

            camera.clearFlags = update.clearMode.ToUnity();
            camera.backgroundColor = update.backgroundColor.ToUnity();
            camera.rect = update.viewport.ToUnity();
            camera.depth = update.depth;

            camera.renderingPath = update.forwardOnly ? RenderingPath.Forward : RenderingPath.UsePlayerSettings;

            helper.RenderShadows = update.renderShadows;

            if (update.postprocessing != cameraHandler.PostprocessingSetup ||
                update.screenSpaceReflections != cameraHandler.ScreenspaceReflectionsSetup ||
                update.motionBlur != cameraHandler.MotionBlurSetup)
            {
                camera.targetTexture = null;

                cameraHandler.PostprocessingSetup = update.postprocessing;
                cameraHandler.ScreenspaceReflectionsSetup = update.screenSpaceReflections;
                cameraHandler.MotionBlurSetup = update.motionBlur;

                RenderingManager.Instance.CameraInitializer.SetupPostprocessing(camera, new CameraSettings()
                {
                    IsPrimary = false,
                    IsSingleCapture = false,
                    IsVR = false,
                    SetupPostProcessing = update.postprocessing,
                    ScreenSpaceReflection = update.screenSpaceReflections,
                    MotionBlur = update.motionBlur
                });
            }

            if (update.renderTextureAssetId < 0)
                helper.Texture = null;
            else
                helper.Texture = RenderingManager.Instance.RenderTextures.GetAsset(update.renderTextureAssetId).Texture;

            // Don't use double buffering when postprocessing filter is active, because that already takes care of this
            // and the double buffering would break the filters
            helper.DoubleBuffer = update.doubleBuffered && !update.postprocessing;

            // Setup selective render
            helper.SelectiveRender.Clear();

            for (int n = 0; n < update.selectiveRenderCount; n++)
            {
                var transform = Space.Transforms[updateData.ReadTransformId()];

                helper.SelectiveRender.Add(transform.gameObject);
            }

            helper.ExcludeRender.Clear();

            for (int n = 0; n < update.excludeRenderCount; n++)
            {
                var transform = Space.Transforms[updateData.ReadTransformId()];

                helper.ExcludeRender.Add(transform.gameObject);
            }

            if (helper.SelectiveRender.Count > 0)
                camera.cullingMask = 1 << UnityEngine.LayerMask.NameToLayer(RenderHelper.TEMP_LAYER);
            else
                camera.cullingMask = update.renderPrivateUI ? _privateLayerMask : _layerMask;

            camera.targetTexture = helper.Texture;
            camera.enabled = camera.targetTexture != null && update.enabled;
        }

        protected override UpdateState InitUpdateData(CameraRenderablesUpdate batch)
        {
            var state = new UpdateState();

            state.transformIds = RenderingManager.Instance.SharedMemory.AccessDataUnmanaged(batch.transformIds);
            state.transformIndex = 0;

            return state;
        }
    }
}
