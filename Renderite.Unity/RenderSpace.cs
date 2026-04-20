using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

namespace Renderite.Unity
{
    public class RenderSpace : MonoBehaviour
    {
        public int Id { get; private set; }

        public bool IsActive { get; private set; }
        public bool IsOverlay { get; private set; }
        public bool IsPrivate { get; private set; }

        public int DefaultLayer { get; private set; }

        public bool WasUpdated { get; private set; }

        public Vector3 RootPosition { get; private set; }
        public Quaternion RootRotation { get; private set; }
        public Vector3 RootScale { get; private set; }

        public bool ViewPositionIsExternal { get; private set; }
        public bool OverrideViewPosition { get; private set; }

        public Vector3 OverridenViewPosition { get; private set; }
        public Quaternion OverridenViewRotation { get; private set; }
        public Vector3 OverridenViewScale { get; private set; }

        public TransformManager Transforms { get; private set; }

        public MeshRendererManager Meshes { get; private set; }
        public SkinnedMeshRendererManager SkinnedMeshes { get; private set; }
        public LightManager Lights { get; private set; }
        public CameraManager Cameras { get; private set; }
        public CameraPortalManager CameraPortals { get; private set; }
        public ReflectionProbeManager ReflectionProbes { get; private set; }
        public ReflectionProbeSH2Manager ReflectionProbeSH2s { get; private set; }
        public LayerManager Layers { get; private set; }
        public BillboardBufferRendererManager BillboardBufferRenderers { get; private set; }
        public MeshBufferRendererManager MeshBufferRenderers { get; private set; }
        public TrailsBufferRendererManager TrailsBufferRenderers { get; private set; }
        public LightsBufferRendererManager LightsBuffersRenderers { get; private set; }
        public RenderTransformOverrideManager RenderTransformOverrides { get; private set; }
        public RenderMaterialOverrideManager RenderMaterialOverrides { get; private set; }
        public BlitToDisplayManager BlitToDisplays { get; private set; }
        public LODGroupRenderableManager LODGroups { get; private set; }
        public GaussianSplatRenderableManager GaussianSplats { get; private set; }

        bool _lastPrivate;

        int _shAssignmentIndex;

        public void Initialize(int id)
        {
            Id = id;

            Transforms = new TransformManager(this, gameObject.transform);
            Meshes = new MeshRendererManager(this);
            SkinnedMeshes = new SkinnedMeshRendererManager(this);
            Lights = new LightManager(this);
            Cameras = new CameraManager(this);
            CameraPortals = new CameraPortalManager(this);
            ReflectionProbes = new ReflectionProbeManager(this);
            ReflectionProbeSH2s = new ReflectionProbeSH2Manager(this);
            Layers = new LayerManager(this);
            BillboardBufferRenderers = new BillboardBufferRendererManager(this);
            MeshBufferRenderers = new MeshBufferRendererManager(this);
            TrailsBufferRenderers = new TrailsBufferRendererManager(this);
            LightsBuffersRenderers = new LightsBufferRendererManager(this);
            RenderTransformOverrides = new RenderTransformOverrideManager(this);
            RenderMaterialOverrides = new RenderMaterialOverrideManager(this);
            BlitToDisplays = new BlitToDisplayManager(this);
            LODGroups = new LODGroupRenderableManager(this);
            GaussianSplats = new GaussianSplatRenderableManager(this);
        }

        public void ClearUpdated()
        {
            WasUpdated = false;
        }

        public void HandleUpdate(Renderite.Shared.RenderSpaceUpdate data)
        {
            // Mark it as updated so it doesn't get removed
            WasUpdated = true;

            IsActive = data.isActive;
            IsOverlay = data.isOverlay;
            IsPrivate = data.isPrivate;

            if (data.isActive)
            {
                // We must activate it if it hasn't been
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                if (data.isPrivate != _lastPrivate)
                {
                    _lastPrivate = data.isPrivate;

                    DefaultLayer = data.isPrivate ? LayerMask.NameToLayer("Private") : LayerMask.NameToLayer("Default");

                    SetLayerRecursively(Transforms.Root, DefaultLayer);
                }

                if (!data.isOverlay)
                {
                    // Assign the skybox material if it isn't yet
                    Material skyboxMaterial;

                    if (data.skyboxMaterialAssetId < 0)
                        skyboxMaterial = RenderingManager.Instance.NullMaterial;
                    else
                        skyboxMaterial = RenderingManager.Instance.Materials.Materials.GetAsset(data.skyboxMaterialAssetId).Material;

                    if (RenderSettings.skybox != skyboxMaterial)
                        RenderSettings.skybox = skyboxMaterial;

                    // IMPORTANT!!! This is necessary to work around yet another cursed Unity bug
                    // For some reason, assigning value to RenderSettings.ambientProbe has chance of not succeeding in some cases.
                    // The value will get assigned and the value will be able to be read back, but Unity will straight up not use it
                    // Any subsequent assignments of the same value will also fail to update the value - as long as we assign the same
                    // value, it will fail and keep using to whatever it was before the failed assignment.
                    // To work around this, we slightly modify the value on each assignment to ensure that one of those assignments will
                    // succeed and update the ambient light properly.
                    if ((_shAssignmentIndex++ & 1) == 0)
                        data.ambientLight.sh0.x += 0.0001f;

                    RenderSettings.ambientProbe = data.ambientLight.ToUnity();
                }
            }
            else
            {
                // Deactivate this space
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            }

            // We want to process all of these updates regardless of whether it's active or not, because otherwise they'll
            // just go to the void, which will lead to desync.
            // Some of them also need to compute some data that is sent back, so if we didn't process these, we'd end up
            // with garbage data being handled in the main process

            RootPosition = data.rootTransform.position.ToUnity();
            RootRotation = data.rootTransform.rotation.ToUnity();
            RootScale = data.rootTransform.scale.ToUnity();

            ViewPositionIsExternal = data.viewPositionIsExternal;
            OverrideViewPosition = data.overrideViewPosition;

            OverridenViewPosition = data.overridenViewTransform.position.ToUnity();
            OverridenViewRotation = data.overridenViewTransform.rotation.ToUnity();
            OverridenViewScale = data.overridenViewTransform.scale.ToUnity();

            // Handle transforms update
            if(data.transformsUpdate != null)
                Transforms.HandleUpdate(data.transformsUpdate);

            if (data.meshRenderersUpdate != null)
                Meshes.HandleUpdate(data.meshRenderersUpdate);

            if (data.skinnedMeshRenderersUpdate != null)
                SkinnedMeshes.HandleUpdate(data.skinnedMeshRenderersUpdate);

            if (data.lightsUpdate != null)
                Lights.HandleUpdate(data.lightsUpdate);

            if (data.camerasUpdate != null)
                Cameras.HandleUpdate(data.camerasUpdate);

            if (data.cameraPortalsUpdate != null)
                CameraPortals.HandleUpdate(data.cameraPortalsUpdate);

            if (data.reflectionProbesUpdate != null)
                ReflectionProbes.HandleUpdate(data.reflectionProbesUpdate);

            if (data.reflectionProbeSH2Taks != null)
                ReflectionProbeSH2s.HandleUpdate(data.reflectionProbeSH2Taks);

            if (data.layersUpdate != null)
                Layers.HandleUpdate(data.layersUpdate);

            if (data.billboardBuffersUpdate != null)
                BillboardBufferRenderers.HandleUpdate(data.billboardBuffersUpdate);

            if (data.meshRenderBuffersUpdate != null)
                MeshBufferRenderers.HandleUpdate(data.meshRenderBuffersUpdate);

            if (data.trailRenderersUpdate != null)
                TrailsBufferRenderers.HandleUpdate(data.trailRenderersUpdate);

            if (data.lightsBufferRenderersUpdate != null)
                LightsBuffersRenderers.HandleUpdate(data.lightsBufferRenderersUpdate);

            if(data.reflectionProbeRenderTasks != null)
                ReflectionProbes.HandleRenderTasks(data.reflectionProbeRenderTasks);

            if (data.renderTransformOverridesUpdate != null)
                RenderTransformOverrides.HandleUpdate(data.renderTransformOverridesUpdate);

            if (data.renderMaterialOverridesUpdate != null)
                RenderMaterialOverrides.HandleUpdate(data.renderMaterialOverridesUpdate);

            if (data.blitToDisplaysUpdate != null)
                BlitToDisplays.HandleUpdate(data.blitToDisplaysUpdate);

            if (data.lodGroupUpdate != null)
                LODGroups.HandleUpdate(data.lodGroupUpdate);

            if (data.gaussianSplatRenderersUpdate != null)
                GaussianSplats.HandleUpdate(data.gaussianSplatRenderersUpdate);
        }

        public void UpdateOverlayPositioning(Transform referenceTransform)
        {
            if (!IsOverlay)
                throw new InvalidOperationException("This space is not an overlay");

            var root = gameObject.transform;

            root.position = referenceTransform.position - RootPosition;
            root.rotation = referenceTransform.rotation * RootRotation;
            root.localScale = referenceTransform.localScale;
        }

        static void SetLayerRecursively(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;

            for (int i = 0; i < transform.childCount; i++)
                SetLayerRecursively(transform.GetChild(i), layer);
        }

        public void Remove()
        {
            // We need to dispose these, because the overrides hook into global rendering events and those need to be
            // unregistered, otherwise they will start throwing lots of exceptions
            RenderTransformOverrides.Dispose();
            RenderMaterialOverrides.Dispose();

            Destroy(gameObject);
        }

        public override string ToString() => $"RenderSpace. Id: {Id}, IsActive: {IsActive}, IsOverlay: {IsOverlay}";
    }
}
