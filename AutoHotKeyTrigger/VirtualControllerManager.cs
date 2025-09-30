namespace AutoHotKeyTrigger
{
    using Nefarius.ViGEm.Client;
    using Nefarius.ViGEm.Client.Targets;
    using Nefarius.ViGEm.Client.Targets.Xbox360;
    using SharpDX.XInput;
    using System;
    using System.Collections.Concurrent;

    public class VirtualControllerManager : IDisposable
    {
        private readonly ViGEmClient? client;
        private readonly IXbox360Controller? virtualController;
        private bool isDisposed = false;
        private readonly ConcurrentDictionary<Xbox360Button, DateTime> injectedPresses = new();

        public VirtualControllerManager()
        {
            try
            {
                this.client = new ViGEmClient();
                this.virtualController = this.client.CreateXbox360Controller();
                this.virtualController.Connect();
            }
            catch (Exception ex) { Console.WriteLine($"ERRO ao inicializar o controle virtual: {ex.Message}."); }
        }

        public void Update(State physicalState)
        {
            if (isDisposed || this.virtualController == null) return;
            try
            {
                // Limpa o estado anterior para garantir que não haja botões "presos"
                this.virtualController.ResetReport();

                // Define os botões um por um, combinando com os injetados
                this.virtualController.SetButtonState(Xbox360Button.A, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A));
                this.virtualController.SetButtonState(Xbox360Button.B, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B));
                this.virtualController.SetButtonState(Xbox360Button.X, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X));
                this.virtualController.SetButtonState(Xbox360Button.Y, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y));
                this.virtualController.SetButtonState(Xbox360Button.LeftShoulder, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder));
                this.virtualController.SetButtonState(Xbox360Button.RightShoulder, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder));
                this.virtualController.SetButtonState(Xbox360Button.LeftThumb, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb));
                this.virtualController.SetButtonState(Xbox360Button.RightThumb, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb));
                this.virtualController.SetButtonState(Xbox360Button.Start, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start));
                this.virtualController.SetButtonState(Xbox360Button.Back, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back));
                this.virtualController.SetButtonState(Xbox360Button.Up, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp));
                this.virtualController.SetButtonState(Xbox360Button.Down, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown));
                this.virtualController.SetButtonState(Xbox360Button.Left, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft));
                this.virtualController.SetButtonState(Xbox360Button.Right, physicalState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight));

                this.virtualController.SetAxisValue(Xbox360Axis.LeftThumbX, physicalState.Gamepad.LeftThumbX);
                this.virtualController.SetAxisValue(Xbox360Axis.LeftThumbY, physicalState.Gamepad.LeftThumbY);
                this.virtualController.SetAxisValue(Xbox360Axis.RightThumbX, physicalState.Gamepad.RightThumbX);
                this.virtualController.SetAxisValue(Xbox360Axis.RightThumbY, physicalState.Gamepad.RightThumbY);
                this.virtualController.SetSliderValue(Xbox360Slider.LeftTrigger, physicalState.Gamepad.LeftTrigger);
                this.virtualController.SetSliderValue(Xbox360Slider.RightTrigger, physicalState.Gamepad.RightTrigger);

                foreach (var injection in injectedPresses)
                {
                    if (DateTime.UtcNow > injection.Value) { injectedPresses.TryRemove(injection.Key, out _); }
                    else { virtualController.SetButtonState(injection.Key, true); }
                }

                this.virtualController.SubmitReport();
            }
            catch { /* Ignorar erros */ }
        }

        public void InjectPress(Xbox360Button button, int durationMs = 200)
        {
            if (button != null) { injectedPresses.TryAdd(button, DateTime.UtcNow.AddMilliseconds(durationMs)); }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                this.virtualController?.Disconnect();
                this.client?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}