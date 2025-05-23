using UnityEngine;
using System;

namespace Blamcon.Lightguns
{
    /// <summary>
    /// Interface that provides functions for controlling typical feedback components.
    /// </summary>
    /// <remarks>
    /// These are a set of standard operations for most modern lightguns. Typically,
    /// three common components include vibration motor, recoil solenoid, and addressable
    /// LEDs.
    /// </remarks>
    public interface IForceFeedback
    {
        /// <summary>
        /// Enables force feedback control to the application.
        /// </summary>
        /// <remarks>
        /// This command will shut down feedback effects on the device so that
        /// the application takes control of the components. For example, when FFB control
        /// is enabled the recoil will not fire when pulling the trigger, it will only recoil
        /// when the application gives the command.
        /// </remarks>
        public bool EnableFFBControl(bool recoil = true, bool rumble = true, bool led = true);

        /// <summary>
        /// Activate vibration motor on the device.
        /// </summary>
        /// <value>Specify the number of vibration pulses to execute.</value>
        /// <remarks>
        /// Vibration motors can be activated by specifying a number of pulses or vibration
        /// cycles. A cycle has an on period followed by an off period. Some lightgun devices
        /// allow this timing to be specified in the command, and others only allow the timings
        /// to be set through UI.
        ///
        /// Returns a boolean to indicate if the IOCTL command was successful.
        ///
        /// Note vibration motor can not be activated separately such that it can be later
        /// deactivated with a separate deactivation command. It can only be pulsed on and off.
        /// </remarks>
        public bool ActivateRumble(int pulses = 1);

        /// <summary>
        /// Activate vibration motor on the device.
        /// </summary>
        /// <value>Specify the number of vibration pulses to execute.</value>
        /// <value>Amount of time the vibration motor should be active, in milliseconds.</value>
        /// <value>Amount of time the vibration motor should be off before the next cycle can start, in milliseconds.</value>
        /// <remarks>
        /// Vibration motors can be activated by specifying a number of pulses or vibration
        /// cycles. A cycle has an on period followed by an off period. Some lightgun devices
        /// allow this timing to be specified in the command, and others only allow the timings
        /// to be set through UI.
        ///
        /// Returns a boolean to indicate if the IOCTL command was successful.
        ///
        /// Note vibration motor can not be activated separately such that it can be later
        /// deactivated with a separate deactivation command. It can only be pulsed on and off.
        /// </remarks>
        public bool ActivateRumble(int pulse, int on, int off);

        /// <summary>
        /// Activate recoil solenoid on the device.
        /// </summary>
        /// <value>Specify the number of recoil pulses to execute.</value>
        /// <remarks>
        /// Recoil solenoids can be activated by specifying a number of pulses or recoil
        /// cycles. A cycle has an on period followed by an off period. Some lightgun devices
        /// allow this timing to be specified in the command, and others only allow the timings
        /// to be set through UI.
        ///
        /// Returns a boolean to indicate if the IOCTL command was successful.
        ///
        /// Note recoil solenoid can not be activated separately such that it can be later
        /// deactivated with a separate deactivation command. It can only be pulsed on and off.
        /// </remarks>
        public bool ActivateRecoil(int pulse = 1);

        /// <summary>
        /// Activate recoil solenoid on the device.
        /// </summary>
        /// <value>Specify the number of recoil pulses to execute.</value>
        /// <value>Amount of time the solenoid should be active, in milliseconds.</value>
        /// <value>Amount of time the solenoid should be off before the next cycle can start, in milliseconds.</value>
        /// <remarks>
        /// Recoil solenoids can be activated by specifying a number of pulses or recoil
        /// cycles. A cycle has an on period followed by an off period. Some lightgun devices
        /// allow this timing to be specified in the command, and others only allow the timings
        /// to be set through UI.
        ///
        /// Returns a boolean to indicate if the IOCTL command was successful.
        ///
        /// Note recoil solenoid can not be activated separately such that it can be later
        /// deactivated with a separate deactivation command. It can only be pulsed on and off.
        /// </remarks>
        public bool ActivateRecoil(int pulse, int on, int off);

        /// <summary>
        /// Activate an LED on the device.
        /// </summary>
        /// <value>Index of the led to activate.</value>
        /// <value>Specified color to set the LED.</value>
        /// <remarks>
        /// Addressable LEDs can be activated by specifying a color. To disable the LED, you can
        /// specify [Color.Black].
        ///
        /// Returns a boolean to indicate if the IOCTL command was successful.
        ///
        /// </remarks>
        public bool ActivateLED(int index, Color color);
        /// <summary>
        /// Activate an LED on the device.
        /// </summary>
        /// <value>Index of the led to activate.</value>
        /// <value>Specified color to set the LED.</value>
        /// <value>Specify the number of flashes to execute.</value>
        /// <value>Amount of time the LED should be off before the next cycle starts, in milliseconds.</value>
        /// <remarks>
        /// Addressable LEDs can be activated by specifying a number of flashes. A cycle has an on period
        /// followed by an off period.
        ///
        /// Returns a boolean to indicate if the IOCTL command was successful.
        ///
        /// </remarks>
        public bool ActivateLED(int index, Color color, int flashes);
    }
}
