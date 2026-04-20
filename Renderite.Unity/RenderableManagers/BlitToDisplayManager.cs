using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class BlitToDisplayManager : RenderableStateChangeManager<BlitToDisplayRenderable, BlitToDisplayRenderablesUpdate,
        BlitToDisplayState, EmptyUpdateData>
    {
        public BlitToDisplayManager(RenderSpace space) : base(space)
        {
        }

        protected override void ApplyState(ref BlitToDisplayState update, BlitToDisplayRenderable handler, ref EmptyUpdateData updateData, BlitToDisplayRenderablesUpdate batch)
        {
            var blitter = handler.Blitter;

            blitter.Texture = TextureHelper.GetTexture(update.textureId);
            blitter.DisplayIndex = update.displayIndex;
            blitter.Color = update.backgroundColor.ToUnity();

            blitter.FlipHorizontally = update.flipHorizontally;
            blitter.FlipVertically = update.flipVertically;
        }

        protected override int GetRenderableIndex(ref BlitToDisplayState state) => state.renderableIndex;

        protected override EmptyUpdateData InitUpdateData(BlitToDisplayRenderablesUpdate batch) => default;
    }
}
