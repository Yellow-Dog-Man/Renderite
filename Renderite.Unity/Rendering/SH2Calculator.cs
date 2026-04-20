using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TextureFormat = UnityEngine.TextureFormat;

namespace Renderite.Unity
{
    public static class SH2Calculator
    {
        static ComputeShader _compute;

        static int _ReduceKernel;
        static int[] _SHkernels = new int[9];

        static ComputeBuffer[] _buffers = new ComputeBuffer[2];

        public static ComputeResult ComputeFromProbe(ReflectionProbe unityProbe, Vector4[] output, ref RenderTexture convertTexture)
        {
            if (unityProbe == null)
                return ComputeResult.Failed;

            if (unityProbe.customBakedTexture == null)
            {
                var realtimeTexture = unityProbe.realtimeTexture;

                // This usually means it hasn't been assigned yet, so just postpone the computation
                if (realtimeTexture == null)
                    return ComputeResult.Postpone;

                return GPU_Project_Uniform_9Coeff(realtimeTexture, output, ref convertTexture) ? ComputeResult.Computed : ComputeResult.Failed;
            }
            else
            {
                var bakedTexture = unityProbe.customBakedTexture as Cubemap;

                if (bakedTexture != null)
                    return GPU_Project_Uniform_9Coeff(bakedTexture, output, ref convertTexture) ? ComputeResult.Computed : ComputeResult.Failed;
            }

            return ComputeResult.Failed;
        }

        public static bool GPU_Project_Uniform_9Coeff(RenderTexture input, Vector4[] output, ref RenderTexture currentTexture)
        {
            //can't have direct access to the cubemap in the compute shader (I think), so i copy the cubemap faces onto a texture2d array
            RenderTextureDescriptor desc = new RenderTextureDescriptor();
            desc.autoGenerateMips = false;
            desc.bindMS = false;
            desc.colorFormat = input.format;
            desc.depthBufferBits = 0;
            desc.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
            desc.enableRandomWrite = false;
            desc.height = input.height;
            desc.width = input.width;
            desc.msaaSamples = 1;
            desc.sRGB = true;
            desc.useMipMap = false;
            desc.volumeDepth = 6;

            if (currentTexture == null ||
                    currentTexture.descriptor.colorFormat != desc.colorFormat ||
                    currentTexture.descriptor.height != desc.height ||
                    currentTexture.descriptor.width != desc.width)
            {
                if (currentTexture != null)
                    UnityEngine.Object.Destroy(currentTexture);

                currentTexture = new RenderTexture(desc);
                currentTexture.Create();
            }

            for (int face = 0; face < 6; ++face)
                Graphics.CopyTexture(input, face, 0, currentTexture, face, 0);

            return Render_GPU_Project_Uniform_9Coeff(currentTexture, output);
        }

        public static bool GPU_Project_Uniform_9Coeff(Cubemap input, Vector4[] output, ref RenderTexture currentTexture)
        {
            var format = ConvertRenderFormat(input.format);
            if (format == null)
                return false;

            //can't have direct access to the cubemap in the compute shader (I think), so i copy the cubemap faces onto a texture2d array
            RenderTextureDescriptor desc = new RenderTextureDescriptor();
            desc.autoGenerateMips = false;
            desc.bindMS = false;
            desc.colorFormat = format.Value;
            desc.depthBufferBits = 0;
            desc.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
            desc.enableRandomWrite = false;
            desc.height = input.height;
            desc.width = input.width;
            desc.msaaSamples = 1;
            desc.sRGB = true;
            desc.useMipMap = false;
            desc.volumeDepth = 6;

            if (currentTexture == null ||
                currentTexture.descriptor.colorFormat != desc.colorFormat ||
                currentTexture.descriptor.height != desc.height ||
                currentTexture.descriptor.width != desc.width)
            {
                if (currentTexture != null)
                    UnityEngine.Object.Destroy(currentTexture);

                currentTexture = new RenderTexture(desc);
                currentTexture.Create();
            }

            for (int face = 0; face < 6; ++face)
                Graphics.CopyTexture(input, face, 0, currentTexture, face, 0);

            return Render_GPU_Project_Uniform_9Coeff(currentTexture, output);
        }

        static RenderTextureFormat? ConvertRenderFormat(TextureFormat input_format)
        {
            switch (input_format)
            {
                case TextureFormat.RGBA32:
                    return RenderTextureFormat.ARGB32;

                case TextureFormat.RGBAHalf:
                    return RenderTextureFormat.ARGBHalf;

                case TextureFormat.RGBAFloat:
                    return RenderTextureFormat.ARGBFloat;

                default:
                    return null;
            }
        }

        static bool Render_GPU_Project_Uniform_9Coeff(RenderTexture input, Vector4[] output)
        {
            if (_compute == null)
            {
                _compute = Resources.Load<ComputeShader>("SphericalHarmonics/SH_Reduce_Uniform");

                _ReduceKernel = _compute.FindKernel("Reduce");

                for (int c = 0; c < 9; c++)
                    _SHkernels[c] = _compute.FindKernel("sh_" + c);
            }

            //the starting number of groups 
            int ceiled_size = Mathf.CeilToInt(input.width / 8.0f);

            var output_buffer = new ComputeBuffer(9, 16);  //the output is a buffer with 9 float4
            var ping_buffer = new ComputeBuffer(ceiled_size * ceiled_size * 6, 16);
            var pong_buffer = new ComputeBuffer(ceiled_size * ceiled_size * 6, 16);

            //cycle 9 coefficients
            for (int c = 0; c < 9; ++c)
            {
                ceiled_size = Mathf.CeilToInt(input.width / 8.0f);

                int kernel = _SHkernels[c];
                _compute.SetInt("coeff", c);

                //first pass, I compute the integral and make a first pass of reduction
                _compute.SetTexture(kernel, "input_data", input);
                _compute.SetBuffer(kernel, "output_buffer", ping_buffer);
                _compute.SetBuffer(kernel, "coefficients", output_buffer);
                _compute.SetInt("ceiled_size", ceiled_size);
                _compute.SetInt("input_size", input.width);
                _compute.SetInt("row_size", ceiled_size);
                _compute.SetInt("face_size", ceiled_size * ceiled_size);
                _compute.Dispatch(kernel, ceiled_size, ceiled_size, 1);

                //second pass, complete reduction
                kernel = _ReduceKernel;

                int index = 0;

                _buffers[0] = ping_buffer;
                _buffers[1] = pong_buffer;

                while (ceiled_size > 1)
                {
                    _compute.SetInt("input_size", ceiled_size);
                    ceiled_size = Mathf.CeilToInt(ceiled_size / 8.0f);
                    _compute.SetInt("ceiled_size", ceiled_size);
                    _compute.SetBuffer(kernel, "coefficients", output_buffer);
                    _compute.SetBuffer(kernel, "input_buffer", _buffers[index]);
                    _compute.SetBuffer(kernel, "output_buffer", _buffers[(index + 1) % 2]);
                    _compute.Dispatch(kernel, ceiled_size, ceiled_size, 1);
                    index = (index + 1) % 2;
                }
            }

            output_buffer.GetData(output);

            pong_buffer.Release();
            ping_buffer.Release();
            output_buffer.Release();

            return true;
        }
    }
}
