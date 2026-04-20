using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class MaterialAsset : Asset
    {
        public Material Material { get; private set; }

        public bool SetShader(Shader shader)
        {
            shader = shader ?? RenderingManager.Instance.NullShader;

            if (Material == null)
            {
                Material = new Material(shader);
                return true;
            }
            else
            {
                Material.shader = shader;
                return false;
            }
        }

        public void Destroy()
        {
            if (Material != null)
                UnityEngine.Object.Destroy(Material);

            Material = null;
        }
    }
}
