using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Renderite.Shared;

namespace Renderite.Unity
{
    public interface IRenderBufferAsset<A, U>
        where A : IRenderBufferAsset<A,U>
        where U : RenderBufferUpload
    {
        void RegisterListener(RenderBufferUpdateHandler<A,U> handler);
        void UnregisterListener(RenderBufferUpdateHandler<A, U> handler);
        void BufferConsumed();
    }

    public delegate void RenderBufferUpdateHandler<A, D>(A asset, D data);

    public abstract class RenderBufferAssetBase<A, U, C> : Asset, IRenderBufferAsset<A,U>
        where A : RenderBufferAssetBase<A, U, C>
        where U : RenderBufferUpload, new()
        where C : AssetCommand, new()
    {
        int remainingListenersToUpdate;

        HashSet<RenderBufferUpdateHandler<A,U>> bufferUpdateListeners = new HashSet<RenderBufferUpdateHandler<A,U>>();

        U _lastData;

        public void HandleUpload(U data)
        {
            lock(bufferUpdateListeners)
            {
                if (remainingListenersToUpdate > 0)
                    throw new InvalidOperationException("There are still listeners handling previous buffer update, cannot update them again!");

                _lastData = data;

                if (bufferUpdateListeners.Count == 0)
                {
                    // There's nothing consuming these, so we just run the callback immediately
                    SendBuffersConsumed();
                    return;
                }

                // We store the count immediately. This is important, because as we trigger the events one by one
                // They can report that they have finished, resulting in decrementing the buffer
                remainingListenersToUpdate = bufferUpdateListeners.Count;

                foreach (var listener in bufferUpdateListeners)
                    listener((A)this, data);
            }
        }

        public void BufferConsumed()
        {
            var newCount = Interlocked.Decrement(ref remainingListenersToUpdate);

            // This is the final buffer, invoke the event
            if (newCount == 0)
                SendBuffersConsumed();
        }

        void SendBuffersConsumed()
        {
            PackerMemoryPool.Instance.Return(_lastData);
            _lastData = null;

            var r = new C();
            r.assetId = AssetId;

            RenderingManager.Instance.SendAssetUpdate(r);
        }

        protected void Unload()
        {
            bufferUpdateListeners = null;
        }

        public void RegisterListener(RenderBufferUpdateHandler<A,U> callback)
        {
            lock (bufferUpdateListeners)
            {
                if (!bufferUpdateListeners.Add(callback))
                    throw new InvalidOperationException("Listener is already registered");
            }
        }

        public void UnregisterListener(RenderBufferUpdateHandler<A,U> callback)
        {
            lock (bufferUpdateListeners)
            {
                if (!bufferUpdateListeners.Remove(callback))
                    throw new InvalidOperationException("Listener is not registered");
            }
        }
    }
}
