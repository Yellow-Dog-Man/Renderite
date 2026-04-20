using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Renderite.Unity
{
    public abstract class InputDriver : MonoBehaviour
    {
        public virtual void Initialize(InputManager manager) { }
        public abstract void UpdateState(InputState state);
    }

    public interface IOutputDriver
    {
        void HandleOutputState(OutputState state);
    }

    public class InputManager
    {
        /// <summary>
        /// The current input state. This instance is persisted. It's used to serialize the data and send over to
        /// the main process, but it's never deallocated. Instead it's updated in-place to avoid GC pressure every frame
        /// </summary>
        public InputState State { get; private set; } = new InputState();

        public InputManager(MouseInput mouse, KeyboardInput keyboard, WindowInput window, DisplayInput display, List<InputDriver> drivers)
        {
            this.mouse = mouse;
            this.keyboard = keyboard;
            this.window = window;
            this.display = display;
            this.drivers = drivers;

            foreach (var driver in drivers)
            {
                driver.Initialize(this);

                if (driver is IOutputDriver outputDriver)
                    outputDrivers.Add(outputDriver);
            }
        }

        public event Action<bool> OnVR_ActiveChanged;

        MouseInput mouse;
        KeyboardInput keyboard;

        WindowInput window;
        DisplayInput display;

        List<InputDriver> drivers;
        List<IOutputDriver> outputDrivers = new List<IOutputDriver>();

        public void RegisterDriver(InputDriver driver)
        {
            drivers.Add(driver);

            driver.Initialize(this);

            if (driver is IOutputDriver outputDriver)
                outputDrivers.Add(outputDriver);
        }

        public void UpdateStateDecoupled()
        {
            // The displays need to be update to keep rendering. The actual collected state might not be used
            // but we don't care - the latest version will be
            display.UpdateState(State);
        }

        public void UpdateState()
        {
            mouse.UpdateState(State);
            keyboard.UpdateState(State);
            window.UpdateState(State);
            display.UpdateState(State);

            foreach (var driver in drivers)
                driver.UpdateState(State);
        }

        public void HandleOutputState(OutputState state)
        {
            mouse.HandleStateUpdate(state);
            keyboard.HandleOutputState(state);

            foreach (var driver in outputDrivers)
                driver.HandleOutputState(state);
        }

        public void VR_ActiveChanged(bool vrActive) => OnVR_ActiveChanged?.Invoke(vrActive);
    }
}
