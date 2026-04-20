using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class CameraPortalManager : RenderableStateChangeManager<
        CameraPortalRenderable, CameraPortalsRenderablesUpdate, CameraPortalState, EmptyUpdateData>
    {
        public static int LayerMask { get; private set; }

        public CameraPortalManager(RenderSpace space) : base(space)
        {
            LayerMask = ~UnityEngine.LayerMask.GetMask("Private", "Overlay");
        }

        protected override EmptyUpdateData InitUpdateData(CameraPortalsRenderablesUpdate batch) => default;
        protected override int GetRenderableIndex(ref CameraPortalState state) => state.renderableIndex;

        protected override void ApplyState(ref CameraPortalState update, CameraPortalRenderable handler, ref EmptyUpdateData updateData, CameraPortalsRenderablesUpdate batch)
        {
            // Get the mesh renderer this needs first
            MeshRenderable renderer;

            if (update.meshRendererIndex < 0)
                renderer = null;
            else
                renderer = Space.Meshes[update.meshRendererIndex];

            var rendererGo = renderer?.Renderer?.gameObject;

            GameObject currentGo = null;

            if (handler.CurrentPortal != null)
                currentGo = handler.CurrentPortal.gameObject;

            if (!object.ReferenceEquals(rendererGo, currentGo))
            {
                // Cleanup the current instance, because we need to make a new one
                handler.CleanupInstance();

                // Setup new instance if possible
                if (rendererGo != null)
                    handler.SetupInstanceOn(rendererGo);
            }

            var portal = handler.CurrentPortal;

            if (portal != null)
            {
                portal.Normal = update.planeNormal.ToUnity();
                portal.ReflectLayers = RenderHelper.PUBLIC_RENDER_MASK;

                portal.ClipPlaneOffset = update.planeOffset;

                if (update.renderTextureId < 0)
                    portal.ReflectionTexture = null;
                else
                    portal.ReflectionTexture = RenderingManager.Instance.RenderTextures.GetAsset(update.renderTextureId).Texture;

                portal.OverrideClearFlag = update.overrideClearFlag?.ToUnity();
                portal.OverrideFarClip = update.overrideFarClip;
                //portal.OverrideNearClip = update.overrideNearClip;

                portal.DisablePixelLights = update.disablePerPixelLights;
                portal.DisableShadows = update.disableShadows;

                portal.RenderMode = update.portalMode ? CameraPortal.Mode.Portal : CameraPortal.Mode.Mirror;

                if (update.portalMode)
                {
                    portal.PortalTransform = update.portalTransform.ToUnity();
                    portal.PortalPlanePosition = update.portalPlanePosition.ToUnity();
                    portal.PortalPlaneNormal = update.portalPlaneNormal.ToUnity();
                }
            }
        }
    }
}
