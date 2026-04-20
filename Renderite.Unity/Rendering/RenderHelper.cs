using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Renderite.Unity
{
    public static class RenderHelper
    {
        public static int PUBLIC_RENDER_MASK => ~LayerMask.GetMask(PRIVATE_LAYER, TEMP_LAYER, OVERLAY_LAYER, HIDDEN_LAYER);
        public static int PRIVATE_RENDER_MASK => ~LayerMask.GetMask(TEMP_LAYER, OVERLAY_LAYER, HIDDEN_LAYER);

        public const string PRIVATE_LAYER = "Private";
        public const string TEMP_LAYER = "Temp";
        public const string HIDDEN_LAYER = "Hidden";
        public const string OVERLAY_LAYER = "Overlay";

        public const string CAPTURE_CAMERA_TAG = "CaptureCamera";

        public static void SetHiearchyLayer(List<GameObject> gameObjects, int layer, Dictionary<GameObject, int> previous)
        {
            if (gameObjects == null)
                return;

            foreach (var go in gameObjects)
                if (go != null)
                    SetHiearchyLayer(go, layer, previous);
        }

        public static void RestoreHiearachyLayer(List<GameObject> gameObjects, Dictionary<GameObject, int> previous)
        {
            if (gameObjects == null)
                return;

            foreach (var go in gameObjects)
                if (go != null)
                    RestoreHiearachyLayer(go, previous);
        }

        public static void SetHiearchyLayer(GameObject root, int layer, Dictionary<GameObject, int> previous)
        {
            if (previous.ContainsKey(root))
                return;

            if (root.layer == layer)
                return;

            previous.Add(root, root.layer);
            root.layer = layer;

            for (int i = 0; i < root.transform.childCount; i++)
                SetHiearchyLayer(root.transform.GetChild(i).gameObject, layer, previous);
        }

        public static void RestoreHiearachyLayer(GameObject root, Dictionary<GameObject, int> previous)
        {
            if (previous.TryGetValue(root, out int previousLayer))
            {
                // already restored
                if (root.layer == previousLayer)
                    return;

                root.layer = previousLayer;
            }

            for (int i = 0; i < root.transform.childCount; i++)
                RestoreHiearachyLayer(root.transform.GetChild(i).gameObject, previous);
        }

        public static void RestoreLayers(Dictionary<GameObject, int> previous)
        {
            foreach (var go in previous)
                go.Key.layer = go.Value;
        }
    }
}
