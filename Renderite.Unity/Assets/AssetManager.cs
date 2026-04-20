using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public class AssetManager<T>
        where T : Asset, new()
    {
        Dictionary<int, T> _instances = new Dictionary<int, T>();

        //HashSet<int> _removedInstances = new HashSet<int>();

        public T GetAsset(int id)
        {
            if (id < 0)
                return null;

            lock (_instances)
            {
                if (!_instances.TryGetValue(id, out var asset))
                {
                    //if (_removedInstances.Contains(id))
                    //    throw new InvalidOperationException($"Trying to access previously removed asset {id} of type {typeof(T).FullName}");

                    asset = new T();
                    asset.AssignId(id);

                    _instances.Add(id, asset);
                }

                return asset;
            }
        }

        public void RemoveAsset(T asset)
        {
            lock (_instances)
            {
                _instances.Remove(asset.AssetId);

                //_removedInstances.Add(asset.AssetId);
            }
        }
    }
}
