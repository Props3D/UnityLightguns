using UnityEngine;
using UnityEngine.InputSystem;
using Blamcon.Lightguns.LowLevel;

namespace Blamcon.Lightguns
{
    /// <summary>
    /// The <see cref="IForceFeedback"/> implementation shared by Blamcon devices that take HID output
    /// report 0x10. Each call builds one <see cref="BlamconHIDOutputReport"/> and sends it to the device.
    /// </summary>
    internal static class BlamconForceFeedback
    {
        /// <summary>
        /// Sends a report to the device, logging an error on failure.
        /// </summary>
        /// <param name="description">What was sent, for the error message.</param>
        public static bool Send(InputDevice device, ref BlamconHIDOutputReport report, string description)
        {
            long result = device.ExecuteCommand(ref report);
            if (result < 0)
                Debug.LogError($"Failed to send {description} to {device.name}. Error: {result}");
            return result >= 0;
        }

        public static bool EnableFFBControl(InputDevice device, bool recoil, bool rumble, bool led, bool ammo)
        {
            var report = BlamconHIDOutputReport.Create(recoil, rumble, led, ammo);
            return Send(device, ref report, "FFB control command");
        }

        public static bool EnableAmmoFFBControl(InputDevice device, bool ammo)
        {
            var report = BlamconHIDOutputReport.Create();
            report.EnableAmmoFFBControl(ammo);
            return Send(device, ref report, "Ammo FFB control command");
        }

        public static bool ActivateRumble(InputDevice device, int pulses)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetRumble(pulses);
            return Send(device, ref report, "rumble command");
        }

        public static bool ActivateRumble(InputDevice device, int pulse, int on, int off)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetRumble(pulse, on, off);
            return Send(device, ref report, "rumble command");
        }

        public static bool ActivateRecoil(InputDevice device, int pulse)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetRecoil(pulse);
            return Send(device, ref report, "recoil command");
        }

        public static bool ActivateRecoil(InputDevice device, int pulse, int on, int off)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetRecoil(pulse, on, off);
            return Send(device, ref report, "recoil command");
        }

        public static bool ActivateLED(InputDevice device, int index, Color color)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetColor(index, color);
            return Send(device, ref report, "LED command");
        }

        public static bool ActivateLED(InputDevice device, int index, Color color, int pulse)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetColor(index, color, pulse);
            return Send(device, ref report, "LED command");
        }

        public static bool SendAmmoCount(InputDevice device, int remaining)
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetAmmo(remaining);
            return Send(device, ref report, "Ammo count");
        }
    }
}
