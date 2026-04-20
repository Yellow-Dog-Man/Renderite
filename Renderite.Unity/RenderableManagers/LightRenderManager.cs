using System;
using System.Collections.Generic;
using System.Text;
using Renderite.Shared;
using UnityEngine;

namespace Renderite.Unity
{
    public class LightManager : RenderableStateChangeManager<LightRenderable, LightRenderablesUpdate, LightState, EmptyUpdateData>
    {
        public LightManager(RenderSpace space) : base(space)
        {
        }

        protected override EmptyUpdateData InitUpdateData(LightRenderablesUpdate batch) => default;
        protected override int GetRenderableIndex(ref LightState state) => state.renderableIndex;
        protected override void ApplyState(ref LightState update, LightRenderable lightHandler, ref EmptyUpdateData updateData, LightRenderablesUpdate batch)
        {
            var light = lightHandler.Light;

            light.type = update.type.ToUnity();
            light.intensity = update.intensity;
            light.range = update.range;
            light.spotAngle = update.spotAngle;
            light.color = update.color.ToUnity();
            light.shadows = update.shadowType.ToUnity();
            light.shadowStrength = update.shadowStrength;
            light.shadowNearPlane = update.shadowNearPlane;
            light.shadowCustomResolution = update.shadowMapResolutionOverride;
            light.shadowBias = update.shadowBias;
            light.shadowNormalBias = update.shadowNormalBias;

            // Only update this when it actually changes
            if (lightHandler.LastCookieAssetId != update.cookieTextureAssetId)
            {
                light.cookie = TextureHelper.GetTexture(update.cookieTextureAssetId);
                lightHandler.LastCookieAssetId = update.cookieTextureAssetId;
            }
        }
    }
}
