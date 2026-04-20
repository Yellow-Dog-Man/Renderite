using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public abstract class RendererCommand : PolymorphicMemoryPackableEntity<RendererCommand>
    {
        static RendererCommand()
        {
            InitTypes(new List<Type>()
            {
                typeof(RendererInitData),
                typeof(RendererInitResult),
                typeof(RendererInitProgressUpdate),
                typeof(RendererInitFinalizeData),
                typeof(RendererEngineReady),
                typeof(RendererShutdownRequest),
                typeof(RendererShutdown),
                typeof(KeepAlive),
                typeof(RendererParentWindow),
                typeof(FreeSharedMemoryView),

                typeof(SetWindowIcon),
                typeof(SetWindowIconResult),
                typeof(SetTaskbarProgress),

                typeof(FrameStartData),
                typeof(FrameSubmitData),

                typeof(PostProcessingConfig),
                typeof(QualityConfig),
                typeof(ResolutionConfig),
                typeof(DesktopConfig),
                typeof(GaussianSplatConfig),
                typeof(RenderDecouplingConfig),

                typeof(MeshUploadData),
                typeof(MeshUnload),
                typeof(MeshUploadResult),

                typeof(ShaderUpload),
                typeof(ShaderUnload),
                typeof(ShaderUploadResult),

                typeof(MaterialPropertyIdRequest),
                typeof(MaterialPropertyIdResult),
                typeof(MaterialsUpdateBatch),
                typeof(MaterialsUpdateBatchResult),
                typeof(UnloadMaterial),
                typeof(UnloadMaterialPropertyBlock),

                typeof(SetTexture2DFormat),
                typeof(SetTexture2DProperties),
                typeof(SetTexture2DData),
                typeof(SetTexture2DResult),
                typeof(UnloadTexture2D),

                typeof(SetTexture3DFormat),
                typeof(SetTexture3DProperties),
                typeof(SetTexture3DData),
                typeof(SetTexture3DResult),
                typeof(UnloadTexture3D),

                typeof(SetCubemapFormat),
                typeof(SetCubemapProperties),
                typeof(SetCubemapData),
                typeof(SetCubemapResult),
                typeof(UnloadCubemap),

                typeof(SetRenderTextureFormat),
                typeof(RenderTextureResult),
                typeof(UnloadRenderTexture),

                typeof(SetDesktopTextureProperties),
                typeof(DesktopTexturePropertiesUpdate),
                typeof(UnloadDesktopTexture),

                typeof(PointRenderBufferUpload),
                typeof(PointRenderBufferConsumed),
                typeof(PointRenderBufferUnload),

                typeof(TrailRenderBufferUpload),
                typeof(TrailRenderBufferConsumed),
                typeof(TrailRenderBufferUnload),

                typeof(GaussianSplatUploadRaw),
                typeof(GaussianSplatUploadEncoded),
                typeof(GaussianSplatResult),
                typeof(UnloadGaussianSplat),

                typeof(LightsBufferRendererSubmission),
                typeof(LightsBufferRendererConsumed),

                typeof(ReflectionProbeRenderResult),

                typeof(VideoTextureLoad),
                typeof(VideoTextureUpdate),
                typeof(VideoTextureReady),
                typeof(VideoTextureChanged),
                typeof(VideoTextureProperties),
                typeof(VideoTextureStartAudioTrack),
                typeof(UnloadVideoTexture),
            });
        }
    }
}
