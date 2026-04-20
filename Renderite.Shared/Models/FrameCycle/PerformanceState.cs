using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class PerformanceState : IMemoryPackable
    {
        public float fps;
        public float immediateFPS;
        public float renderTime;
        public float externalUpdateTime;

        public int renderedFramesSinceLast;

        public float frameBeginToSubmitTime;
        public float frameProcessedToNextBeginTime;

        
        public float integrationProcessingTime;
        public float extraParticleProcessingTime;
        
        public int processedAssetIntegratorTasks;
        public int integrationHighPriorityTasks;
        public int integrationTasks;
        public int integrationRenderTasks;
        public int integrationParticleTasks;
        
        public int processingHandleWaits;
        
        public float frameUpdateHandleTime;
        
        public int renderedCameras;
        public int renderedCameraPortals;
        public int updatedTextures;
        public int textureSliceUploads;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(fps);
            packer.Write(immediateFPS);
            packer.Write(renderTime);
            packer.Write(externalUpdateTime);

            packer.Write(renderedFramesSinceLast);

            packer.Write(frameBeginToSubmitTime);
            packer.Write(frameProcessedToNextBeginTime);

            packer.Write(integrationProcessingTime);
            packer.Write(extraParticleProcessingTime);
            packer.Write(processedAssetIntegratorTasks);
            packer.Write(integrationHighPriorityTasks);
            packer.Write(integrationTasks);
            packer.Write(integrationRenderTasks);
            packer.Write(integrationParticleTasks);

            packer.Write(processingHandleWaits);
            packer.Write(frameUpdateHandleTime);

            packer.Write(renderedCameras);
            packer.Write(renderedCameraPortals);
            packer.Write(updatedTextures);
            packer.Write(textureSliceUploads);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref fps);
            packer.Read(ref immediateFPS);
            packer.Read(ref renderTime);
            packer.Read(ref externalUpdateTime);

            packer.Read(ref renderedFramesSinceLast);

            packer.Read(ref frameBeginToSubmitTime);
            packer.Read(ref frameProcessedToNextBeginTime);

            packer.Read(ref integrationProcessingTime);
            packer.Read(ref extraParticleProcessingTime);
            packer.Read(ref processedAssetIntegratorTasks);
            packer.Read(ref integrationHighPriorityTasks);
            packer.Read(ref integrationTasks);
            packer.Read(ref integrationRenderTasks);
            packer.Read(ref integrationParticleTasks);

            packer.Read(ref processingHandleWaits);
            packer.Read(ref frameUpdateHandleTime);

            packer.Read(ref renderedCameras);
            packer.Read(ref renderedCameraPortals);
            packer.Read(ref updatedTextures);
            packer.Read(ref textureSliceUploads);
        }

    }
}
