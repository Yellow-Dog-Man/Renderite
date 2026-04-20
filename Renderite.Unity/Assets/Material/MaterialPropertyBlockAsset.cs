using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class MaterialPropertyBlockAsset : Asset
    {
        static Stack<MaterialPropertyBlock> _blockPool = new Stack<MaterialPropertyBlock>();

        public MaterialPropertyBlock PropertyBlock { get; private set; }

        public bool EnsureInstance()
        {
            if (PropertyBlock != null)
                return false;

            if (_blockPool.Count == 0)
                PropertyBlock = new MaterialPropertyBlock();
            else
                PropertyBlock = _blockPool.Pop();

            return true;
        }

        public void Free()
        {
            if (PropertyBlock == null)
                return;

            PropertyBlock.Clear();
            _blockPool.Push(PropertyBlock);
            PropertyBlock = null;
        }
    }
}
