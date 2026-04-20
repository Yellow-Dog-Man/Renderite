using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Renderite.Unity
{
    public class PointRenderBufferData : IPoolable
    {
        public int Count { get; set; }

        public Span<Vector3> Positions => _positions == null ? default : _positions.AsSpan().Slice(0, Count);
        public Span<Quaternion> Rotations => _rotations == null ? default : _rotations.AsSpan().Slice(0, Count);
        public Span<Vector3> Scales => _scales == null ? default : _scales.AsSpan().Slice(0, Count);
        public Span<Color> Colors => _colors == null ? default : _colors.AsSpan().Slice(0, Count);

        public Vector2Int FrameGridSize { get; set; }

        public Span<ushort> FrameIndexes
        {
            get
            {
                if (_frameIndexes == null || _frameIndexes.Length < Count || !_hasFrameIndexes)
                    return default;

                return _frameIndexes.AsSpan().Slice(0, Count);
            }
        }

        bool _hasFrameIndexes;

        Vector3[] _positions;
        Quaternion[] _rotations;
        Vector3[] _scales;
        Color[] _colors;
        ushort[] _frameIndexes;

        public void CopyFrom(PointRenderBufferUpload data)
        {
            var rawData = RenderingManager.Instance.SharedMemory.AccessData(data.buffer);

            Count = data.count;

            FrameGridSize = data.frameGridSize.ToUnity();

            _hasFrameIndexes = data.frameIndexesOffset >= 0;

            var positions = MemoryMarshal.Cast<byte, Vector3>(rawData.Slice(data.positionsOffset)).Slice(0, Count);
            var rotations = MemoryMarshal.Cast<byte, Quaternion>(rawData.Slice(data.rotationsOffset)).Slice(0, Count);
            var scales = MemoryMarshal.Cast<byte, Vector3>(rawData.Slice(data.sizesOffset)).Slice(0, Count);
            var colors = MemoryMarshal.Cast<byte, Color>(rawData.Slice(data.colorsOffset)).Slice(0, Count);

            Span<ushort> frameIndexes;

            if (_hasFrameIndexes)
                frameIndexes = MemoryMarshal.Cast<byte, ushort>(rawData.Slice(data.frameIndexesOffset)).Slice(0, Count);
            else
                frameIndexes = default;

            Copy(ref _positions, positions);
            Copy(ref _rotations, rotations);
            Copy(ref _scales, scales);
            Copy(ref _colors, colors);
            Copy(ref _frameIndexes, frameIndexes);
        }

        void Copy<T>(ref T[] array, Span<T> source)
        {
            if (source.IsEmpty)
                return;

            if (array == null || array.Length < Count)
                array = new T[Count];

            source.Slice(0, Count).CopyTo(array);
        }

        public void Clean()
        {
            Count = 0;
            FrameGridSize = default;
        }
    }

    public abstract class ParticleBasedPointRenderBufferRenderer<TState> :
        ParticleBasedRenderBufferRenderer<PointRenderBufferAsset, PointRenderBufferData, PointRenderBufferUpload, TState>
        where TState : unmanaged
    {
        const int CONVERT_GROUP_SIZE = 1024 * 4;

        protected enum RotationMode
        {
            None,
            EulerAngles,
            VelocityAndRotationForward,
            VelocityAndRotationUp,
            VelocityOnly,
        }

        protected abstract RotationMode RotationHandling { get; }

        UnityEngine.ParticleSystem.TextureSheetAnimationModule textureSheet;

        protected override void ParticleSystemAllocated(UnityEngine.ParticleSystem system, ParticleSystemRenderer renderer)
        {
            textureSheet = system.textureSheetAnimation;
        }

        protected override void OnSubmitBufferData(BufferSubmission data)
        {
            // Update particle sheet configuration
            if (data.gridSize == Vector2Int.one)
            {
                if (textureSheet.enabled)
                    textureSheet.enabled = false;
            }
            else
            {
                if (!textureSheet.enabled)
                    textureSheet.enabled = true;

                if (textureSheet.numTilesX != data.gridSize.x)
                    textureSheet.numTilesX = data.gridSize.x;

                if (textureSheet.numTilesY != data.gridSize.y)
                    textureSheet.numTilesY = data.gridSize.y;
            }
        }

        protected override PointRenderBufferData ExtractData(PointRenderBufferAsset buffer, PointRenderBufferUpload uploadData)
        {
            var data = MemoryPool.Borrow<PointRenderBufferData>();
            data.CopyFrom(uploadData);
            return data;
        }

        protected override void AssignFrame(ref UnityEngine.ParticleSystem.Particle particle, ushort frame, int frameCount)
        {
            var maxIndex = frameCount - 1;

            particle.startLifetime = maxIndex;
            particle.remainingLifetime = Mathf.Max(maxIndex - frame, 0.5f);
        }

        protected override BufferSubmission GenerateSubmissionData(PointRenderBufferData data)
        {
            // Schedule an update to happen with the new buffer data
            var length = data.Count;
            var gridSize = data.FrameGridSize;

            var positions = data.Positions;
            var rotations = data.Rotations;
            var scales = data.Scales;
            var colors = data.Colors;
            var frames = data.FrameIndexes;

            var hasFrames = !frames.IsEmpty;
            var frameCount = gridSize.x * gridSize.y;

            if (positions.Length != length ||
                rotations.Length != length ||
                scales.Length != length ||
                colors.Length != length ||
                (!frames.IsEmpty && frames.Length != length))
            {
                // Something has changed, bail out
                return null;
            }

            var submissionData = MemoryPool.Borrow<BufferSubmission>();

            submissionData.buffer = new NativeArray<ParticleSystem.Particle>(length,
                Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            var targetData = submissionData.buffer;

            var rotationMode = RotationHandling;

            var groups = MathHelper.CeilToInt(length / (double)CONVERT_GROUP_SIZE);

            Parallel.For(0, groups, g =>
            {
                var start = g * CONVERT_GROUP_SIZE;
                var end = Mathf.Min(start + CONVERT_GROUP_SIZE, length);

                var positions = data.Positions;
                var rotations = data.Rotations;
                var scales = data.Scales;
                var colors = data.Colors;
                var frames = data.FrameIndexes;

                var p = new ParticleSystem.Particle();

                for (int i = start; i < end; i++)
                {
                    p.position = positions[i];
                    p.startSize3D = scales[i];
                    p.startColor = colors[i];

                    Vector3 velocity;
                    float angle;

                    switch (rotationMode)
                    {
                        case RotationMode.EulerAngles:
                            p.rotation3D = Quaternion.LookRotation(rotations[i] * Vector3.forward, rotations[i] * Vector3.up).eulerAngles;
                            break;

                        case RotationMode.VelocityOnly:
                            p.velocity = rotations[i] * Vector3.forward;
                            break;

                        case RotationMode.VelocityAndRotationForward:
                            ComputeVelocityOrientation(rotations[i], Vector3.forward, out velocity, out angle);

                            p.velocity = velocity;
                            p.rotation = angle;
                            break;

                        case RotationMode.VelocityAndRotationUp:
                            ComputeVelocityOrientation(rotations[i], Vector3.up, out velocity, out angle);

                            p.velocity = velocity;
                            p.rotation = -angle;
                            break;
                    }

                    if (hasFrames)
                        AssignFrame(ref p, frames[i], frameCount);

                    targetData[i] = p;
                }
            });

            submissionData.gridSize = hasFrames ? gridSize : Vector2Int.one;

            return submissionData;
        }

        static void ComputeVelocityOrientation(in Quaternion rotation, in Vector3 targetUp, out Vector3 velocity, out float angle)
        {
            var forward = rotation * Vector3.forward;

            // Unfortunate hack to get around more weirdness with Unity. For some reason, the rotations are fully
            // local within the coordinate system for the particle system UNLESS the forward vector is exactly aligned
            // when it does that, then it just produces... garbage rotations. So we offset the forward vector
            // very slightly - not enough to really be noticeable, but enough to get around the singularity issue
            if (Vector3.Dot(forward, Vector3.forward) >= 0.999999f)
                forward = new Vector3(forward.x + 0.00001f, forward.y, forward.z).normalized;

            var psForward = forward;

            var psRotation = Quaternion.LookRotation(forward, targetUp);

            var wantedUp = rotation * Vector3.up;
            var psUp = psRotation * Vector3.up;

            var cross = Vector3.Cross(wantedUp, psUp);

            angle = Vector3.Angle(wantedUp, psUp);

            var dot = Vector3.Dot(forward, cross);

            if (float.IsNaN(dot))
                dot = 0;

            var sign = Mathf.Sign(dot);

            if (sign != 0)
                angle *= sign;

            velocity = forward;
        }
    }
}
