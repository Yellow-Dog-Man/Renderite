using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class RenderContextOverride : Renderable
    {
        RenderingContextHandler _handler;
        protected RenderingContext? _registeredContext { get; private set; }

        bool _isOverriden;
        RenderingContext? _overridenContext;

        protected abstract void Override();
        protected abstract void Restore();

        protected void BeginUpdateSetup(RenderingContext targetContext)
        {
            if (_registeredContext != targetContext)
            {
                if (_isOverriden)
                    RunRestore();

                UnregisterHandler();

                RenderContextHelper.RegisterRenderContextEvents(targetContext, _handler);

                _registeredContext = targetContext;
            }
        }

        protected void FinishUpdateSetup()
        {
            if (_isOverriden)
                throw new InvalidOperationException($"RenderTransform is overriden while being updated. " +
                    $"Current: {RenderContextHelper.CurrentRenderingContext}, Overriden: {_overridenContext}");

            // check if we actually want to override
            if (_registeredContext != null && RenderContextHelper.CurrentRenderingContext == _registeredContext)
                RunOverride();
        }

        protected override void Setup(Transform root)
        {
            _handler = HandleRenderingContextSwitch;
        }

        protected override void Cleanup()
        {
            UnregisterHandler();
        }

        void HandleRenderingContextSwitch(RenderingContextStage stage)
        {
            if (stage == RenderingContextStage.Begin)
                RunOverride();
            else if (stage == RenderingContextStage.End && _isOverriden)
                RunRestore();
        }

        protected void RunOverride()
        {
            if (_isOverriden)
                throw new Exception("RenderTransform is already overriden!");

            _isOverriden = true;
            _overridenContext = RenderContextHelper.CurrentRenderingContext;

            Override();
        }

        protected void RunRestore()
        {
            if (!_isOverriden)
                throw new Exception("RenderTransform is not overriden");

            Restore();

            _isOverriden = false;
            _overridenContext = null;
        }

        void UnregisterHandler()
        {
            if (_registeredContext == null)
                return;

            RenderContextHelper.UnregisterRenderContextEvents(_registeredContext.Value, _handler);

            _registeredContext = null;
        }
    }
}
