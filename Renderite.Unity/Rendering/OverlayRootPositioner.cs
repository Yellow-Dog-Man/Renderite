using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class OverlayRootPositioner : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = (1 / transform.parent.lossyScale.x) * Vector3.one;
        }
    }
}
