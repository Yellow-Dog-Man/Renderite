using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using NativeGraphics.NET;
using UnityEngine;

namespace Renderite.Unity
{
    public class AssetIntegrator
    {
        const int DELAYED_REMOVAL_UPDATES = 3;

        public UnityEngine.Rendering.GraphicsDeviceType GraphicsDeviceType { get; private set; }
        public bool IsUsingLinearSpace { get; private set; }
        public static bool IsEditor { get; private set; }
        public static bool IsDebugBuild { get; private set; }

        internal static SharpDX.Direct3D11.Device _dx11device;

        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        static void RenderThreadCallback()
        {
            try
            {
                if (!IsDebugBuild)
                    UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Disabled;

                var instance = RenderingManager.Instance.AssetIntegrator;

                instance.ProcessRenderThreadQueue(instance.maxMilliseconds);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("Exception in render thread queue processing:\n" + ex);
            }
            finally
            {
                if (!IsDebugBuild)
                    UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Enabled;
            }
        }

        struct QueueAction
        {
            public readonly Action action;
            public readonly Action<object> actionWithData;
            public readonly IEnumerator coroutine;
            public readonly object data;

            public QueueAction(Action action)
            {
                this.action = action;

                this.actionWithData = null;
                this.coroutine = null;
                this.data = null;
            }

            public QueueAction(IEnumerator coroutine)
            {
                this.coroutine = coroutine;

                this.action = null;
                this.actionWithData = null;
                this.data = null;
            }

            public QueueAction(Action<object> actionWithData, object data)
            {
                this.actionWithData = actionWithData;
                this.data = data;

                this.action = null;
                this.coroutine = null;
            }
        }

        ConcurrentQueue<QueueAction> highpriorityQueue = new ConcurrentQueue<QueueAction>();
        ConcurrentQueue<QueueAction> processingQueue = new ConcurrentQueue<QueueAction>();
        ConcurrentQueue<QueueAction> renderThreadQueue = new ConcurrentQueue<QueueAction>();
        ConcurrentQueue<QueueAction> particlesQueue = new ConcurrentQueue<QueueAction>();

        ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();

        Queue<Action> delayedRemovals = new Queue<Action>();
        int[] delayedRemovalCounts = new int[DELAYED_REMOVAL_UPDATES];
        int delayedRemovalBucketIndex;

        Stopwatch stopwatch = new Stopwatch();
        Stopwatch particlesStopwatch = new Stopwatch();
        double maxMilliseconds;

        Action<int> renderThreadCallback;
        IntPtr renderThreadPointer;

        Action tasksAvailable;

        public int HighPriorityTasks => highpriorityQueue.Count;
        public int NormalTasks => processingQueue.Count;
        public int RenderThreadTasks => renderThreadQueue.Count;
        public int ParticleTasks => particlesQueue.Count;

        public bool RenderThreadProcessingEnabled { get; private set; }

        public void Initialize(Action onTasksAvailable)
        {
            IsUsingLinearSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;

            GraphicsDeviceType = UnityEngine.SystemInfo.graphicsDeviceType;
            IsEditor = UnityEngine.Application.isEditor;
            IsDebugBuild = UnityEngine.Debug.isDebugBuild;

            UnityEngine.Debug.Log($"Graphics Device Type: {GraphicsDeviceType}");

            switch (GraphicsDeviceType)
            {
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:
                    var dummyTex = new UnityEngine.Texture2D(4, 4);
                    var dummySharpTex = new SharpDX.Direct3D11.Texture2D(dummyTex.GetNativeTexturePtr());
                    _dx11device = dummySharpTex.Device;

                    if (dummyTex)
                        UnityEngine.Object.Destroy(dummyTex);

                    RenderThreadProcessingEnabled = true;
                    break;

                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                    RenderThreadProcessingEnabled = true;
                    break;
            }

            if (RenderThreadProcessingEnabled)
            {
                Callback.SetUpdateCallback(RenderThreadCallback);
                renderThreadPointer = Callback.GetRenderEventFunc();
            }

            tasksAvailable = onTasksAvailable;
        }

        public void EnqueueDelayedRemoval(Action removalAction)
        {
            delayedRemovals.Enqueue(removalAction);
            delayedRemovalCounts[delayedRemovalBucketIndex]++;
        }

        public void ProcessDelayedRemovals()
        {
            var index = (delayedRemovalBucketIndex + (DELAYED_REMOVAL_UPDATES - 1)) % DELAYED_REMOVAL_UPDATES;
            var count = delayedRemovalCounts[index];

            for (int i = 0; i < count; i++)
                delayedRemovals.Dequeue()();

            delayedRemovalCounts[index] = 0;

            delayedRemovalBucketIndex++;
            delayedRemovalBucketIndex %= DELAYED_REMOVAL_UPDATES;
        }

        public void EnqueueRenderThreadProcessing(IEnumerator coroutine)
        {
            if (!RenderThreadProcessingEnabled)
                throw new NotSupportedException("Render Thread Processing is not enabled");

            renderThreadQueue.Enqueue(new QueueAction(coroutine));

            tasksAvailable();
        }

        public void EnqueueRenderThreadProcessing(Action action)
        {
            if (!RenderThreadProcessingEnabled)
                throw new NotSupportedException("Render Thread Processing is not enabled");

            renderThreadQueue.Enqueue(new QueueAction(action));

            tasksAvailable();
        }

        public void EnqueueProcessing(IEnumerator coroutine, bool highPriority)
        {
            if (highPriority)
                highpriorityQueue.Enqueue(new QueueAction(coroutine));
            else
                processingQueue.Enqueue(new QueueAction(coroutine));

            tasksAvailable();
        }

        public void EnqueueProcessing(Action action, bool highPriority)
        {
            if (highPriority)
                highpriorityQueue.Enqueue(new QueueAction(action));
            else
                processingQueue.Enqueue(new QueueAction(action));

            tasksAvailable();
        }

        public void EnqueueProcessing(Action<object> action, object data, bool highPriority)
        {
            if (highPriority)
                highpriorityQueue.Enqueue(new QueueAction(action, data));
            else
                processingQueue.Enqueue(new QueueAction(action, data));

            tasksAvailable();
        }

        public void EnqueueParticleProcessing(Action action)
        {
            particlesQueue.Enqueue(new QueueAction(action));

            tasksAvailable();
        }

        public void EnqueueParticleProcessing(Action<object> action, object data)
        {
            particlesQueue.Enqueue(new QueueAction(action, data));

            tasksAvailable();
        }

        public void EnqueueTask(Action action)
        {
            taskQueue.Enqueue(action);

            tasksAvailable();
        }

        public bool Process()
        {
            bool anyProcessed = false;

            // Process all the tasks first
            while (taskQueue.TryDequeue(out Action action))
            {
                try
                {
                    action();
                    anyProcessed = true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Exception running AssetIntegrator task:\n" + ex);
                }
            }

            if (anyProcessed)
                return true;

            if (ProcessHighPriorityQueueTask())
                return true;

            if (ProcessNormalQueueTask())
                return true;

            if (ProcessParticleQueueTask())
                return true;

            return false;
        }

        public void RunRenderThreadUploads(double maxMilliseconds)
        {
            if (!RenderThreadProcessingEnabled)
                return;

            if (renderThreadQueue.Count == 0)
                return;

            this.maxMilliseconds = maxMilliseconds;

            UnityEngine.GL.IssuePluginEvent(renderThreadPointer, 0);
        }

        public int ProcessRenderThreadQueue(double maxMilliseconds)
        {
            int processed = 0;

            stopwatch.Restart();

            try
            {
                do
                {
                    if (ProcessRenderThreadTask())
                        processed++;
                    else
                        break;

                } while (stopwatch.ElapsedMilliseconds < maxMilliseconds);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError("Exception integrating asset: " + ex);
            }

            return processed;
        }

        public bool ProcessHighPriorityQueueTask() => ProcessQueueTask(highpriorityQueue);
        public bool ProcessNormalQueueTask() => ProcessQueueTask(processingQueue);

        bool ProcessQueueTask(ConcurrentQueue<QueueAction> queue)
        {
            if (!queue.TryPeek(out var process))
                return false;

            if (ProcessQueueAction(process))
                queue.TryDequeue(out _);

            return true;
        }

        bool ProcessRenderThreadTask()
        {
            var fetched = renderThreadQueue.TryPeek(out var process);

            if (!fetched)
                return false;

            if (ProcessQueueAction(process))
                renderThreadQueue.TryDequeue(out _);

            return true;
        }

        public bool ProcessParticleQueueTask()
        {
            QueueAction process;

            var fetched = particlesQueue.TryPeek(out process);

            if (!fetched)
                return false;

            if(ProcessQueueAction(process))
                particlesQueue.TryDequeue(out _);

            return true;
        }

        bool ProcessQueueAction(QueueAction process)
        {
            if (process.action != null)
            {
                process.action();
                return true;
            }
            else if (process.actionWithData != null)
            {
                process.actionWithData(process.data);
                return true;
            }
            else
                return !process.coroutine.MoveNext();
        }
    }
}
