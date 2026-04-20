using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Renderite.Shared;
using System.Runtime.InteropServices;

namespace Renderite.Unity
{
    public class TrailsRenderBufferData : IPoolable
    {
        public Span<TrailOffset> Trails => _trails == null ? default : _trails.AsSpan().Slice(0, _trailsCount);
        public Span<Vector3> TrailPositions => _positions == null ? default : _positions.AsSpan().Slice(0, _positionsCount);
        public Span<Color> TrailColors => _colors == null ? default : _colors.AsSpan().Slice(0, _colorsCount);
        public Span<float> TrailSizes => _sizes == null ? default : _sizes.AsSpan().Slice(0, _sizesCount);

        int _trailsCount;
        int _positionsCount;
        int _colorsCount;
        int _sizesCount;

        TrailOffset[] _trails;
        Vector3[] _positions;
        Color[] _colors;
        float[] _sizes;

        public void CopyFrom(TrailRenderBufferUpload data)
        {
            var rawData = RenderingManager.Instance.SharedMemory.AccessData(data.buffer);

            var trails = MemoryMarshal.Cast<byte, TrailOffset>(rawData.Slice(data.trailsOffset)).Slice(0, data.trailsCount);

            var positions = MemoryMarshal.Cast<byte, Vector3>(rawData.Slice(data.positionsOffset)).Slice(0, data.trailPointCount);
            var colors = MemoryMarshal.Cast<byte, Color>(rawData.Slice(data.colorsOffset)).Slice(0, data.trailPointCount);
            var sizes = MemoryMarshal.Cast<byte, float>(rawData.Slice(data.sizesOffset)).Slice(0, data.trailPointCount);

            Copy(ref _trails, ref _trailsCount, trails);
            Copy(ref _positions, ref _positionsCount, positions);
            Copy(ref _colors, ref _colorsCount, colors);
            Copy(ref _sizes, ref _sizesCount, sizes);
        }

        void Copy<T>(ref T[] array, ref int count, Span<T> source)
        {
            count = source.Length;

            if (source.IsEmpty)
                return;

            if (array == null || array.Length < count)
                array = new T[count];

            source.CopyTo(array);
        }

        public void Clean()
        {
            _trailsCount = 0;
            _positionsCount = 0;
            _colorsCount = 0;
            _sizesCount = 0;
        }
    }

    public class TrailsRenderBufferRenderer :
        ParticleBasedRenderBufferRenderer<TrailsRenderBufferAsset, TrailsRenderBufferData, TrailRenderBufferUpload, TrailsRendererState>
    {
        const int CONVERT_GROUP_SIZE = 1024 * 4;

        ParticleSystem.TrailModule trailModule;

        protected override TrailsRenderBufferAsset ExtractBuffer(ref TrailsRendererState state)
        {
            return RenderingManager.Instance.TrailsRenderBuffers.GetAsset(state.trailsRenderBufferAssetId);
        }

        protected override void AssignFrame(ref ParticleSystem.Particle particle, ushort frame, int frameCount)
        {
            // This doesn't use frames as all
        }

        protected override TrailsRenderBufferData ExtractData(TrailsRenderBufferAsset buffer, TrailRenderBufferUpload uploadData)
        {
            var data = MemoryPool.Borrow<TrailsRenderBufferData>();
            data.CopyFrom(uploadData);
            return data;
        }

        protected override BufferSubmission GenerateSubmissionData(TrailsRenderBufferData data)
        {
            var trails = data.Trails;

            var submissionData = MemoryPool.Borrow<BufferSubmission>();

            var trailCount = trails.Length;

            // Compute how many points we need
            int pointCount = 0;

            var maxTrailLength = 0;

            foreach (var trail in trails)
                maxTrailLength = Mathf.Max(maxTrailLength, trail.count);

            // Add padding
            maxTrailLength += 2;

            // We need 2 extra points for each trail for padding to separate them
            /*foreach (var trail in trails)
            {
                var trailPointCount = trail.count;

                if (trailPointCount < 2)
                    continue;

                pointCount += trail.count + 2;
            }*/

            pointCount = maxTrailLength * trailCount;

            // Compute how many trails should we process in a single threading group
            var trailsPerGroup = Mathf.Max(1, MathHelper.RoundToInt(maxTrailLength / (double)CONVERT_GROUP_SIZE));

            // Compute how many groups we'll need to process all the trails
            var groups = MathHelper.CeilToInt(trailCount / trailsPerGroup);

            submissionData.buffer = new NativeArray<ParticleSystem.Particle>(pointCount,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            var targetData = submissionData.buffer;

            var maxLifetime = pointCount + 1;

            Parallel.For(0, groups, g =>
            {
                var p = new ParticleSystem.Particle();

                var trails = data.Trails;

                var positions = data.TrailPositions;
                var colors = data.TrailColors;
                var sizes = data.TrailSizes;

                var start = g * trailsPerGroup;
                var end = Mathf.Min(start + trailsPerGroup, trailCount);

                for(int trailIndex = start; trailIndex < end; trailIndex++)
                {
                    int TrailIndexToUnity(int idx) => (trailIndex + idx * trailCount);

                    var index = 0;

                    ref var trail = ref trails[trailIndex];

                    int paddingStartIndex = index++;
                    int startIndex = index;

                    for (int i = 0; i < trail.count; i++)
                    {
                        var dIdx = trail.GetIndex(i);
                        var uIdx = TrailIndexToUnity(index);

                        p.position = positions[dIdx];
                        p.startColor = colors[dIdx];
                        p.startSize = sizes[dIdx];

                        p.startLifetime = maxLifetime;
                        p.remainingLifetime = maxLifetime - uIdx;

                        targetData[uIdx] = p;

                        index++;
                    }

                    int endIndex = index - 1;
                    var uEndIdx = TrailIndexToUnity(endIndex);

                    // Pad the particles to the full trail length
                    var lastPos = targetData[uEndIdx].position;

                    for (int i = trail.count; i < maxTrailLength - 2; i++)
                    {
                        var uIdx = TrailIndexToUnity(index);

                        p.position = lastPos;
                        p.startColor = default;
                        p.startSize = 0;

                        p.startLifetime = maxLifetime;
                        p.remainingLifetime = maxLifetime - uIdx;

                        targetData[uIdx] = p;

                        index++;
                    }

                    int paddingEndIndex = index++;

                    // Update the padding particles
                    var uStartIdx = TrailIndexToUnity(startIndex);
                    var uPadStartIdx = TrailIndexToUnity(paddingStartIndex);
                    var uPadEndIdx = TrailIndexToUnity(paddingEndIndex);

                    // Start Particle
                    p.position = targetData[uStartIdx].position;
                    p.startColor = targetData[uStartIdx].startColor;

                    p.startSize = 0;
                    p.startLifetime = maxLifetime;
                    p.remainingLifetime = maxLifetime - uPadStartIdx;

                    targetData[uPadStartIdx] = p;

                    // End particle
                    p.position = targetData[uEndIdx].position;
                    p.startColor = targetData[uEndIdx].startColor;

                    p.startSize = 0;
                    p.startLifetime = maxLifetime;
                    p.remainingLifetime = maxLifetime - uPadEndIdx;

                    targetData[uPadEndIdx] = p;
                }
            });

            submissionData.gridSize = Vector2Int.one;
            submissionData.ribbonCount = trailCount;

            return submissionData;
        }

        protected override void ParticleSystemAllocated(ParticleSystem system, ParticleSystemRenderer renderer)
        {
            renderer.renderMode = ParticleSystemRenderMode.None;

            trailModule = system.trails;

            trailModule.enabled = true;
            trailModule.mode = ParticleSystemTrailMode.Ribbon;
            trailModule.ribbonCount = 1;
            trailModule.textureMode = ParticleSystemTrailTextureMode.Stretch;
        }

        protected override void OnSubmitBufferData(BufferSubmission data)
        {
            trailModule.ribbonCount = data.ribbonCount;
        }

        protected override void ApplyState(ParticleSystem system, ParticleSystemRenderer renderer, ref TrailsRendererState state)
        {
            renderer.trailMaterial = RenderingManager.Instance.Materials.Materials.GetAsset(state.materialAssetId)?.Material;
            renderer.motionVectorGenerationMode = state.motionVectorMode.ToUnity();
            trailModule.textureMode = state.textureMode.ToUnity();
            trailModule.generateLightingData = state.generateLightingData;
        }
    }
}
