using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    /// <summary>
    /// Data representing a render space and necessary updates to render it.
    /// </summary>
    public class RenderSpaceUpdate : IMemoryPackable
    {
        /// <summary>
        /// Unique identifier of this render space. This will let the renderer match any updates to locally allocated data
        /// </summary>
        public int id;

        /// <summary>
        /// Indicates whether this space is active and should be rendered
        /// </summary>
        public bool isActive;

        /// <summary>
        /// Indicates if this render space is an overlay or not. This affects some things like skybox material being used
        /// </summary>
        public bool isOverlay;

        /// <summary>
        /// Is this render space considered private - it will be configured to be in a private layer
        /// </summary>
        public bool isPrivate;

        /// <summary>
        /// The root of the rendering space
        /// </summary>
        public RenderTransform rootTransform;

        /// <summary>
        /// Indicates that the current view position is external to the user's head view.
        /// E.g. when it's freecam or third person camera.
        /// </summary>
        public bool viewPositionIsExternal;

        /// <summary>
        /// Indicates if the view position should be overriden to a specific point.
        /// </summary>
        public bool overrideViewPosition;

        /// <summary>
        /// The AssetID of the material that this world uses as skybox
        /// </summary>
        public int skyboxMaterialAssetId;

        /// <summary>
        /// Describes the spherical harmonics of the ambient light for the world
        /// </summary>
        public RenderSH2 ambientLight;

        /// <summary>
        /// Transform of the overriden view
        /// </summary>
        public RenderTransform overridenViewTransform;

        /// <summary>
        /// Update for the transforms of this render space.
        /// </summary>
        public TransformsUpdate? transformsUpdate;

        /// <summary>
        /// Update to any mesh renderers
        /// </summary>
        public MeshRenderablesUpdate? meshRenderersUpdate;

        /// <summary>
        /// Update to any skinned mesh renderers
        /// </summary>
        public SkinnedMeshRenderablesUpdate? skinnedMeshRenderersUpdate;

        /// <summary>
        /// Update to any lights
        /// </summary>
        public LightRenderablesUpdate? lightsUpdate;

        /// <summary>
        /// Update to any cameras
        /// </summary>
        public CameraRenderablesUpdate? camerasUpdate;

        /// <summary>
        /// Update to any camera portals
        /// </summary>
        public CameraPortalsRenderablesUpdate? cameraPortalsUpdate;

        /// <summary>
        /// Update to any reflection probes
        /// </summary>
        public ReflectionProbeRenderablesUpdate? reflectionProbesUpdate;

        /// <summary>
        /// Any SH2 compute tasks from reflection probes to compute in this update
        /// </summary>
        public ReflectionProbeSH2Tasks? reflectionProbeSH2Taks;

        /// <summary>
        /// Update to any layers
        /// </summary>
        public LayerUpdate? layersUpdate;

        /// <summary>
        /// Billboard render buffers
        /// </summary>
        public BillboardRenderBufferUpdate? billboardBuffersUpdate;

        /// <summary>
        /// Mesh render buffers
        /// </summary>
        public MeshRenderBufferUpdate? meshRenderBuffersUpdate;

        /// <summary>
        /// Trails renderers
        /// </summary>
        public TrailsRendererUpdate? trailRenderersUpdate;

        /// <summary>
        /// Lights buffer renderers
        /// </summary>
        public LightsBufferRendererUpdate? lightsBufferRenderersUpdate;

        /// <summary>
        /// Updates to any render transform overrides
        /// </summary>
        public RenderTransformOverridesUpdate? renderTransformOverridesUpdate;

        /// <summary>
        /// Updates to any material overrides
        /// </summary>
        public RenderMaterialOverridesUpdate? renderMaterialOverridesUpdate;

        /// <summary>
        /// Updates to any material overrides
        /// </summary>
        public BlitToDisplayRenderablesUpdate? blitToDisplaysUpdate;

        /// <summary>
        /// Update to any LOD groups
        /// </summary>
        public LODGroupRenderablesUpdate? lodGroupUpdate;

        /// <summary>
        /// Update to any gaussian splat renderers
        /// </summary>
        public GaussianSplatRenderablesUpdate? gaussianSplatRenderersUpdate;

        /// <summary>
        /// Any tasks for reflection probes to be rendered
        /// </summary>
        public List<ReflectionProbeRenderTask> reflectionProbeRenderTasks;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(id);

            packer.Write(isActive);
            packer.Write(isOverlay);
            packer.Write(isPrivate);

            packer.Write(rootTransform);
            packer.Write(viewPositionIsExternal);
            packer.Write(overrideViewPosition);
            packer.Write(skyboxMaterialAssetId);

            packer.Write(ambientLight);
            packer.Write(overridenViewTransform);

            packer.WriteObject(transformsUpdate);

            packer.WriteObject(meshRenderersUpdate);
            packer.WriteObject(skinnedMeshRenderersUpdate);
            packer.WriteObject(lightsUpdate);
            packer.WriteObject(camerasUpdate);
            packer.WriteObject(cameraPortalsUpdate);
            packer.WriteObject(reflectionProbesUpdate);
            packer.WriteObject(reflectionProbeSH2Taks);
            packer.WriteObject(layersUpdate);
            packer.WriteObject(billboardBuffersUpdate);
            packer.WriteObject(meshRenderBuffersUpdate);
            packer.WriteObject(trailRenderersUpdate);
            packer.WriteObject(lightsBufferRenderersUpdate);

            packer.WriteObject(renderTransformOverridesUpdate);
            packer.WriteObject(renderMaterialOverridesUpdate);
            packer.WriteObject(blitToDisplaysUpdate);
            packer.WriteObject(lodGroupUpdate);
            packer.WriteObject(gaussianSplatRenderersUpdate);

            packer.WriteObjectList(reflectionProbeRenderTasks);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref id);

            packer.Read(ref isActive);
            packer.Read(ref isOverlay);
            packer.Read(ref isPrivate);

            packer.Read(ref rootTransform);
            packer.Read(ref viewPositionIsExternal);
            packer.Read(ref overrideViewPosition);
            packer.Read(ref skyboxMaterialAssetId);

            packer.Read(ref ambientLight);
            packer.Read(ref overridenViewTransform);

            packer.ReadObject(ref transformsUpdate);

            packer.ReadObject(ref meshRenderersUpdate);
            packer.ReadObject(ref skinnedMeshRenderersUpdate);
            packer.ReadObject(ref lightsUpdate);
            packer.ReadObject(ref camerasUpdate);
            packer.ReadObject(ref cameraPortalsUpdate);
            packer.ReadObject(ref reflectionProbesUpdate);
            packer.ReadObject(ref reflectionProbeSH2Taks);
            packer.ReadObject(ref layersUpdate);
            packer.ReadObject(ref billboardBuffersUpdate);
            packer.ReadObject(ref meshRenderBuffersUpdate);
            packer.ReadObject(ref trailRenderersUpdate);
            packer.ReadObject(ref lightsBufferRenderersUpdate);

            packer.ReadObject(ref renderTransformOverridesUpdate);
            packer.ReadObject(ref renderMaterialOverridesUpdate);
            packer.ReadObject(ref blitToDisplaysUpdate);
            packer.ReadObject(ref lodGroupUpdate);
            packer.ReadObject(ref gaussianSplatRenderersUpdate);

            packer.ReadObjectList(ref reflectionProbeRenderTasks);
        }

    }
}
