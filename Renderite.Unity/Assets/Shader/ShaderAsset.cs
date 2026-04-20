using Renderite.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Renderite.Unity
{
    public class ShaderAsset : Asset
    {
        public UnityEngine.Shader UnityShader { get; private set; }

        UnityEngine.AssetBundle _assetBundle;

        public void Handle(ShaderUpload uploadData)
        {
            LoadFromFile(uploadData.file);

            PackerMemoryPool.Instance.Return(uploadData);
        }

        public void Handle(ShaderUnload unload)
        {
            Unload();

            // Ensure it's removed from the manager
            RenderingManager.Instance.Shaders.RemoveAsset(this);

            PackerMemoryPool.Instance.Return(unload);
        }

        void LoadFromFile(string file)
        {
            AssetIntegrator.EnqueueProcessing(LoadShader(file), true);
        }

        IEnumerator LoadShader(string file)
        {
            // unload old
            UnloadImmediate();

            try
            {
                //UnityEngine.Debug.Log("Loading Shader Bundle: " + file + "\t" + this.GetHashCode());

                var bundleRequest = UnityEngine.AssetBundle.LoadFromFileAsync(file);

                bundleRequest.completed += result =>
                {
                    //UnityEngine.Debug.Log("Bundle Loaded: " + file + "\t" + this.GetHashCode());

                    try
                    {
                        _assetBundle = bundleRequest.assetBundle;

                        if (_assetBundle == null)
                        {
                            UnityEngine.Debug.LogWarning($"Could not load shader asset bundle: {file}, exists: {System.IO.File.Exists(file)}");
                            SendLoaded();
                            return;
                        }

                        var shaderRequest = _assetBundle.LoadAssetAsync<UnityEngine.Shader>(_assetBundle.GetAllAssetNames()[0]);

                        shaderRequest.completed += shaderResult =>
                        {
                            try
                            {
                                UnityShader = shaderRequest.asset as UnityEngine.Shader;
                                SendLoaded();

                                //UnityEngine.Debug.Log("Shader Loaded: " + UnityShader + "\t" + this.GetHashCode());
                            }
                            catch (Exception ex)
                            {
                                UnityEngine.Debug.LogError($"Exception loading shader from the loaded bundle {file}\n{ex}");
                                SendLoaded();
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Exception processing loaded shader bundle for {file}\n{ex}");
                        SendLoaded();
                    }
                };
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Exception loading shader from file: " + file + "\n" + ex);
                throw;
            }

            yield break;
        }

        void SendLoaded()
        {
            var result = new ShaderUploadResult();

            result.assetId = AssetId;

            // Right now the instance always changes
            result.instanceChanged = true;

            RenderingManager.Instance.SendAssetUpdate(result);
        }

        void Unload()
        {
            AssetIntegrator.EnqueueProcessing(UnloadImmediate, true);
        }

        void UnloadImmediate()
        {
            if (_assetBundle != null)
            {
                _assetBundle.Unload(true);

                if (_assetBundle)
                    UnityEngine.Object.Destroy(_assetBundle);
            }

            if (UnityShader)
                UnityEngine.Object.Destroy(UnityShader);

            _assetBundle = null;
            UnityShader = null;
        }
    }
}
