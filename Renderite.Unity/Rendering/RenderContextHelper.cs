using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Unity
{
    public enum RenderingContextStage
    {
        Begin,
        End
    }

    public delegate void RenderingContextHandler(RenderingContextStage stage);

    public static class RenderContextHelper
    {
        public static RenderingContext? CurrentRenderingContext => _currentContext;

        static RenderingContext? _currentContext;

        public static void BeginRenderContext(RenderingContext context)
        {
            if (context == _currentContext)
                return;

            EndCurrentRenderContext();

            HashSet<RenderingContextHandler> handlers;

            _currentContext = context;

            if (renderingContexts.TryGetValue(context, out handlers))
                foreach (var handler in handlers)
                    handler(RenderingContextStage.Begin);
        }

        public static void EndCurrentRenderContext()
        {
            if (_currentContext == null)
                return;

            HashSet<RenderingContextHandler> handlers;

            if (renderingContexts.TryGetValue(_currentContext.Value, out handlers))
                foreach (var handler in handlers)
                    handler(RenderingContextStage.End);

            _currentContext = null;
        }

        public static void RegisterRenderContextEvents(RenderingContext context, RenderingContextHandler handler)
        {
            if (!renderingContexts.TryGetValue(context, out var handlers))
            {
                handlers = new HashSet<RenderingContextHandler>();
                renderingContexts.Add(context, handlers);
            }

            if (!handlers.Add(handler))
                throw new InvalidOperationException("Handler already registered");
        }

        public static void UnregisterRenderContextEvents(RenderingContext context, RenderingContextHandler handler)
        {
            if (!renderingContexts[context].Remove(handler))
                throw new InvalidOperationException("Handler not registered");
        }

        static Dictionary<RenderingContext, HashSet<RenderingContextHandler>> renderingContexts = new Dictionary<RenderingContext, HashSet<RenderingContextHandler>>();
    }
}
