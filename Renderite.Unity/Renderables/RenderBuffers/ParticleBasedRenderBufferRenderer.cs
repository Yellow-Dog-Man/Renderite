using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Renderite.Shared;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Renderite.Unity
{
    public abstract class ParticleBasedRenderBufferRenderer<TAsset, TSubmission, TUpdate, TState> : Renderable
        where TAsset : class, IRenderBufferAsset<TAsset,TUpdate>
        where TUpdate : RenderBufferUpload
        where TSubmission : class, IPoolable, new()
        where TState : unmanaged
    {
        const int MAX_SCHEDULED_SUBMISSIONS = 2;

        class QueuedBuffer
        {
            public TAsset buffer;
            public TUpdate update;

            public QueuedBuffer(TAsset buffer, TUpdate update)
            {
                this.buffer = buffer;
                this.update = update;
            }
        }

        protected class BufferSubmission : IPoolable
        {
            public NativeArray<UnityEngine.ParticleSystem.Particle> buffer;
            public Vector2Int gridSize;
            public int ribbonCount;

            public void Clean()
            {
                buffer.Dispose();
                buffer = default;

                gridSize = Vector2Int.zero;
                ribbonCount = 0;
            }
        }

        readonly struct BufferUpdate
        {
            public readonly Action<TAsset, TUpdate> submit;
            public readonly TAsset buffer;
            public readonly TUpdate update;

            public BufferUpdate(Action<TAsset, TUpdate> submit, TAsset buffer, TUpdate update)
            {
                this.submit = submit;
                this.buffer = buffer;
                this.update = update;
            }
        }

        UnityEngine.ParticleSystem particleSystem;
        ParticleSystemRenderer particleRenderer;

        TAsset _registeredRenderBuffer;
        bool _lastSubmissionEmpty;

        static ActionBlock<BufferUpdate> updateProcessor;

        Action<TAsset, TUpdate> _submitMethod;

        int _scheduledSubmissions;
        QueuedBuffer _queuedBuffer;

        static ParticleBasedRenderBufferRenderer()
        {
            updateProcessor = new ActionBlock<BufferUpdate>(
                u =>
                {
                    try
                    {
                        u.submit(u.buffer, u.update);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception converting buffer data:\n{ex}");
                    }
                },
                new ExecutionDataflowBlockOptions()
                {
                    EnsureOrdered = false,
                    MaxDegreeOfParallelism = -1
                });
        }

        protected abstract BufferSubmission GenerateSubmissionData(TSubmission updatingBuffer);

        void ConvertBufferData(TAsset updatingBuffer, TUpdate uploadData)
        {
            if (particleSystem == null || updatingBuffer != _registeredRenderBuffer)
            {
                updatingBuffer.BufferConsumed();
                return;
            }

            // Extract the data and tell the buffer it's been consumed as fast as possible
            // This will let the conversion run on a copy and let the simulation run another update
            // while the rest of the submission happens
            var data = ExtractData(updatingBuffer, uploadData);
            updatingBuffer.BufferConsumed();

            var submissionData = GenerateSubmissionData(data);

            MemoryPool.Return(ref data);

            if (submissionData == null)
                return;

            // Indicate that a submission is scheduled. This will prevent any further updates being computed
            // while we are still waiting to submit this data
            Interlocked.Increment(ref _scheduledSubmissions);

            RenderingManager.Instance.AssetIntegrator.EnqueueParticleProcessing(SubmitBufferData, submissionData);
        }

        void SubmitBufferData(object data)
        {
            // Try process any scheduled buffer if there's one
            // We do this early before we call SetParticles which can take a little bit, so another computation
            // can get a head start
            ProcessQueuedBuffer();

            var submissionData = (BufferSubmission)data;

            if (particleSystem != null)
            {
                OnSubmitBufferData(submissionData);

                // Update the particles
                particleSystem.SetParticles(submissionData.buffer);

                RenderingManager.Instance.Stats.ParticlesUploaded(submissionData.buffer.Length);
            }

            MemoryPool.Return(ref submissionData);

            // We are done with the submission, so drop the indication
            var decremented = Interlocked.Decrement(ref _scheduledSubmissions);

            // Try process any scheduled buffer if there's one
            if(decremented < MAX_SCHEDULED_SUBMISSIONS)
                ProcessQueuedBuffer();
        }

        protected override void Setup(Transform root)
        {
            _submitMethod = ConvertBufferData;
        }

        public void ApplyState(ref TState state)
        {
            if (particleSystem == null)
            {
                var go = ActualTransform.gameObject;

                particleSystem = go.AddComponent<UnityEngine.ParticleSystem>();
                particleRenderer = go.GetComponent<ParticleSystemRenderer>();

                // We don't want the system to restart on itself when disabled and enabled
                var main = particleSystem.main;
                main.playOnAwake = false;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;

                // We do not want the system to scale the particle for us - we do this ourselves with the scale properties
                particleRenderer.lengthScale = 1;
                particleRenderer.velocityScale = 0;

                particleRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;

                ParticleSystemAllocated(particleSystem, particleRenderer);

                // We don't want it to do any actual simulation
                particleSystem.Pause();
            }

            ApplyState(particleSystem, particleRenderer, ref state);

            var buffer = ExtractBuffer(ref state);

            if (buffer == null)
            {
                // Clear it out
                particleSystem.Clear();

                // Unregister existing render buffer
                UnregisterRenderBuffer();
            }
            else
            {
                if (_registeredRenderBuffer != buffer)
                {
                    // Unregister previous buffer
                    UnregisterRenderBuffer();

                    _registeredRenderBuffer = buffer;
                    _registeredRenderBuffer.RegisterListener(OnRenderBufferUpdate);
                }
            }
        }

        void OnRenderBufferUpdate(TAsset buffer, TUpdate data)
        {
            // We have unregistered it in the meanwhile, we don't care about the buffer anymore, just tell it
            // that it's been consumed (that way it's not stuck waiting on this update to finish
            // If the buffer is empty and last submission was also empty, we skip the submission altogether
            // to not clog the processing queue
            if (buffer != _registeredRenderBuffer || (data.IsEmpty && _lastSubmissionEmpty))
            {
                buffer.BufferConsumed();
                return;
            }

            _lastSubmissionEmpty = data.IsEmpty;

            var id = GetHashCode();

            if (_scheduledSubmissions >= MAX_SCHEDULED_SUBMISSIONS)
            {
                if (_queuedBuffer != null)
                    throw new InvalidOperationException("There's already a queued buffer when render buffer update is called");

                // There's already submission scheduled, wait until it finished before we submit another update
                // Queued is guaranteed to be null right now
                _queuedBuffer = new QueuedBuffer(buffer, data);

                // The submission might've finished in the meanwhile, check it again and try to process the buffer
                // in case the submission missed it
                if (_scheduledSubmissions < MAX_SCHEDULED_SUBMISSIONS)
                    ProcessQueuedBuffer();
            }
            else
            {
                // Schedule the computation to happen on background threads
                updateProcessor.Post(new BufferUpdate(_submitMethod, buffer, data));
            }
        }

        void ProcessQueuedBuffer()
        {
            var buffer = Interlocked.Exchange(ref _queuedBuffer, null);

            if (buffer == null)
                return;

            updateProcessor.Post(new BufferUpdate(_submitMethod, buffer.buffer, buffer.update));
        }

        void UnregisterRenderBuffer()
        {
            if (_registeredRenderBuffer == null)
                return;

            _registeredRenderBuffer.UnregisterListener(OnRenderBufferUpdate);
            _registeredRenderBuffer = null;
        }

        protected override void Cleanup()
        {
            if (particleSystem != null)
                UnityEngine.Object.Destroy(particleSystem);

            particleSystem = null;
        }

        protected abstract TAsset ExtractBuffer(ref TState state);
        protected abstract TSubmission ExtractData(TAsset buffer, TUpdate data);
        protected abstract void ParticleSystemAllocated(ParticleSystem system, ParticleSystemRenderer renderer);
        protected abstract void ApplyState(ParticleSystem system, ParticleSystemRenderer renderer, ref TState state);

        protected abstract void OnSubmitBufferData(BufferSubmission data);

        protected abstract void AssignFrame(ref ParticleSystem.Particle particle, ushort frame, int frameCount);
    }
}
