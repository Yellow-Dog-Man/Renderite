using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public struct ReflectionProbeSH2Task
    {
        /// <summary>
        /// The idnex of the renderable reflection probe that this task belongs to
        /// </summary>
        public int renderableIndex;

        /// <summary>
        /// Which reflection probe to compute the SH2 from
        /// </summary>
        public int reflectionProbeRenderableIndex;

        /// <summary>
        /// Type of result from this compute
        /// </summary>
        public ComputeResult result;

        /// <summary>
        /// The result data containing the computed values (when succeeded)
        /// </summary>
        public RenderSH2 resultData;
    }
}
