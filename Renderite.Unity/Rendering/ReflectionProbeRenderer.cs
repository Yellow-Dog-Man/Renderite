using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;

namespace Renderite.Unity
{
    public class ReflectionProbeRenderer : UnityEngine.MonoBehaviour
    {
        public UnityEngine.ReflectionProbe probe;
        public ReflectionProbeRenderable renderable;

        public ReflectionProbeRenderTask task;

        public UnityEngine.RenderTexture texture;
        public Dictionary<UnityEngine.GameObject, int> previousLayers;
        public int renderId;
        public UnityEngine.Rendering.ReflectionProbeTimeSlicingMode previousTimeSlicingMode;

        bool finishDone;
        bool finishRunning;

        List<UnityEngine.Texture2D> readMips;

        void FinishRender()
        {
            if (probe != null)
                probe.timeSlicingMode = previousTimeSlicingMode;

            var desc = texture.descriptor;

            desc.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

            var baseSize = desc.width;

            var faceMips = new List<UnityEngine.RenderTexture>();
            readMips = new List<UnityEngine.Texture2D>();

            var miplevels = task.mipOrigins[0].Count;

            for (int m = 0; m < miplevels; m++)
            {
                faceMips.Add(UnityEngine.RenderTexture.GetTemporary(desc));

                // IMPORTANT: We actually need a texture for each of the faces, becase well be accessing the
                // native data trough NativeArray, which does not create a copy of the data.
                for (int f = 0; f < 6; f++)
                {
                    readMips.Add(new UnityEngine.Texture2D(desc.width, desc.height, desc.graphicsFormat,
                        UnityEngine.Experimental.Rendering.TextureCreationFlags.None));
                }

                desc.useMipMap = false;
                desc.width /= 2;
                desc.height /= 2;
            }

            var faceData = new List<NativeArray<byte>>();

            UnityEngine.Texture2D GetReadMip(int face, int mip) => readMips[face + mip * 6];

            for (int f = 0; f < 6; f++)
            {
                UnityEngine.Graphics.CopyTexture(texture, f, faceMips[0], 0);

                for (int m = 0; m < miplevels; m++)
                {
                    if (m > 0)
                        UnityEngine.Graphics.CopyTexture(faceMips[0], 0, m, faceMips[m], 0, 0);

                    var readTex = GetReadMip(f, m);

                    var prev = UnityEngine.RenderTexture.active;
                    UnityEngine.RenderTexture.active = faceMips[m];
                    readTex.ReadPixels(new UnityEngine.Rect(0, 0, readTex.width, readTex.height), 0, 0, false);
                    UnityEngine.RenderTexture.active = prev;

                    faceData.Add(readTex.GetRawTextureData<byte>());
                }
            }

            foreach (var mip in faceMips)
                UnityEngine.RenderTexture.ReleaseTemporary(mip);

            Task.Run(() =>
            {
                try
                {
                    var rawData = RenderingManager.Instance.SharedMemory.AccessData(task.resultData);

                    int idx = 0;

                    for (int f = 0; f < 6; f++)
                    {
                        var mipStarts = task.mipOrigins[f];

                        for (int m = 0; m < miplevels; m++)
                        {
                            var start = mipStarts[m];
                            var data = faceData[idx++];

                            var targetData = rawData.Slice(start, data.Length);

                            unsafe
                            {
                                var dataSpan = new Span<byte>(data.GetUnsafeReadOnlyPtr(), data.Length);
                                dataSpan.CopyTo(targetData);
                            }

                            data.Dispose();
                        }
                    }

                    SendResult(true);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Exception converting reflection probe render data for task ID {task.renderTaskId}:\n{ex}");
                    SendResult(false);
                }
                finally
                {
                    finishDone = true;
                }
            });
        }

        void LateUpdate()
        {
            if(finishDone)
            {
                Cleanup();
                return;
            }

            if (finishRunning)
                return;

            if (probe.IsFinishedRendering(renderId))
            {
                try
                {
                    finishRunning = true;

                    FinishRender();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Exception finishing reflection probe render task ID {task.renderTaskId}:\n{ex}");

                    SendResult(false);
                    Cleanup();
                }
            }
        }

        void SendResult(bool success)
        {
            var result = new ReflectionProbeRenderResult()
            {
                renderTaskId = task.renderTaskId,
                success = success
            };

            RenderingManager.Instance.SendReflectionProbeRenderResult(result);

            PackerMemoryPool.Instance.Return(task);
        }

        void OnDestroy() => Cleanup();

        void Cleanup()
        {
            if (texture != null)
                UnityEngine.RenderTexture.ReleaseTemporary(texture);

            if (previousLayers != null)
            {
                RenderHelper.RestoreLayers(previousLayers);
                previousLayers = null;
            }

            if(readMips != null)
            {
                foreach (var mip in readMips)
                    UnityEngine.Object.Destroy(mip);

                readMips = null;
            }

            probe = null;
            renderable = null;
            task = null;
            texture = null;

            Destroy(this);
        }
    }
}
