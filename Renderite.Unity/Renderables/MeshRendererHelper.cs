using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public static class MeshRendererHelper
    {
        public static IMeshRenderable GetMeshRenderable(int packedId, RenderSpace space)
        {
            if (packedId == ~0)
                return null;

            IdPacker<MeshRendererType>.Unpack(packedId, out var id, out var type);

            switch(type)
            {
                case MeshRendererType.SkinnedMeshRenderer: return space.SkinnedMeshes[id];
                case MeshRendererType.MeshRenderer: return space.Meshes[id];

                default:
                    throw new NotImplementedException($"Unsupported mesh renderer type: {type}");
            }
        }
    }
}
