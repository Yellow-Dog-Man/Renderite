using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class ReflectionProbeSH2Renderable : Renderable
    {
        static Vector4[] output = new Vector4[9];

        // We keep one per renderable, because it's often reused frequently for particular probe
        RenderTexture _convertTexture;

        protected override void Cleanup()
        {
            if(_convertTexture != null)
            {
                UnityEngine.Object.Destroy(_convertTexture);
                _convertTexture = null;
            }
        }

        protected override void Setup(Transform root)
        {
        }

        public ComputeResult Compute(ReflectionProbe reflectionProbe, out RenderSH2 sh2)
        {
            var result = SH2Calculator.ComputeFromProbe(reflectionProbe, output, ref _convertTexture);

            if (result == ComputeResult.Computed)
            {
                RenderVector3 ToVector(Vector4 sh) => new RenderVector3(sh.x, sh.y, sh.z);

                // Fill the data to sh2
                sh2 = new RenderSH2(
                    ToVector(output[0]),
                    ToVector(output[1]),
                    ToVector(output[2]),
                    ToVector(output[3]),
                    ToVector(output[4]),
                    ToVector(output[5]),
                    ToVector(output[6]),
                    ToVector(output[7]),
                    ToVector(output[8])
                    );
            }
            else
                sh2 = default;

            return result;
        }
    }
}
