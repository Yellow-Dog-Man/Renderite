using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public interface IBackingMemoryBuffer : IDisposable
    {
        int SizeBytes { get; }
        Span<byte> RawData { get; }
        Memory<byte> Memory { get; }

        bool IsDisposed { get; }

        bool TryLockUse();
        void Unlock();
    }

    public abstract class BackingMemoryBuffer : IBackingMemoryBuffer
    {
        public abstract int SizeBytes { get; }
        public abstract Span<byte> RawData { get; }
        public abstract Memory<byte> Memory { get; }

        public bool IsDisposed { get; private set; }

        int _activeUses;
        bool _dispose;

        public void Dispose()
        {
            IsDisposed = true;

            lock(this)
            {
                _dispose = true;

                if (_activeUses == 0)
                    ActuallyDispose();
            }
        }

        public bool TryLockUse()
        {
            if (_dispose)
                return false;

            lock (this)
            {
                if (_dispose)
                    return false;

                _activeUses++;

                return true;
            }
        }

        public void Unlock()
        {
            lock(this)
            {
                if (_activeUses == 0)
                    throw new InvalidOperationException("Trying to unlock buffer that has no active uses");

                if (--_activeUses == 0 && _dispose)
                    ActuallyDispose();
            }
        }

        protected abstract void ActuallyDispose();
    }
}
