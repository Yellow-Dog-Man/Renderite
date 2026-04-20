using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public class CameraPortalRenderable : Renderable
    {
        public CameraPortal CurrentPortal { get; private set; }

        protected override void Cleanup()
        {
            CleanupInstance();
        }

        protected override void Setup(Transform root)
        {
            // We don't actually have anything to setup, because the portal is put ona mesh renderer
            // meaning we need to wait for an update to come in
        }

        public void SetupInstanceOn(GameObject gameObject)
        {
            if (CurrentPortal != null)
                throw new InvalidOperationException("Instance has already been setup. Clean it up first.");

            CurrentPortal = gameObject.AddComponent<CameraPortal>();
            CurrentPortal.ReflectLayers = CameraPortalManager.LayerMask;
        }

        public void CleanupInstance()
        {
            if (CurrentPortal == null)
                return;

            UnityEngine.Object.Destroy(CurrentPortal);

            CurrentPortal = null;
        }
    }
}
