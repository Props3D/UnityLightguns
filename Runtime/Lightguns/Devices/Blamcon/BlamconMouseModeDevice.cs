using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using Blamcon.Lightguns.LowLevel;

namespace Blamcon.Lightguns
{
    /// <summary>
    /// A Blamcon lightgun in mouse mode, reached through the vendor-defined HID collection (usage page
    /// 0xFF00, usage 0x01) that current firmware adds next to the mouse and keyboard collections. Windows
    /// opens those two exclusively, so this collection is the only way to send a mouse-mode gun feedback.
    /// </summary>
    /// <remarks>
    /// Output only: aim and fire arrive through the system <see cref="Mouse"/>. Games reach it through
    /// <see cref="BlamconLightgunHID.GetForceFeedback"/>, never directly.
    ///
    /// Deliberately not a <see cref="Lightgun"/>. The collection has no input reports, so as a Lightgun it
    /// would expose a position and buttons that never change, and lightgun bindings could pick it up. It
    /// has no controls at all. The Input System only sizes a control-less device from a primitive state
    /// format, hence BYTE; no state events arrive, because the collection has no input reports.
    /// </remarks>
    [InputControlLayout(stateFormat = "BYTE")]
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] // Make sure static constructor is called during startup.
#endif
    internal class BlamconMouseModeDevice : InputDevice, IForceFeedback
    {
        static BlamconMouseModeDevice()
        {
            for (var player = 0; player < BlamconDevices.kPlayerCount; player++)
            {
                InputSystem.RegisterLayout<BlamconMouseModeDevice>($"Blamcon Lightgun Mouse Mode - P{player + 1}",
                    matches: new InputDeviceMatcher()
                        .WithInterface("HID")
                        .WithCapability("usagePage", 0xFF00)                                  // Vendor-defined.
                        .WithCapability("usage", 0x01)
                        .WithCapability("productId", BlamconDevices.kFirstProductId + player)
                        .WithCapability("vendorId", BlamconDevices.kVendorId));
            }
        }

        // In the Player, to trigger the calling of the static constructor,
        // create an empty method annotated with RuntimeInitializeOnLoadMethod.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {}

        /// <summary>0-based player index from the HID product ID, or -1.</summary>
        internal int playerIndex { get; private set; } = -1;

        /// <summary>Increases with every Blamcon device created; the highest is the most recently added.</summary>
        internal long creationOrder { get; private set; }

        /// <inheritdoc />
        protected override void FinishSetup()
        {
            base.FinishSetup();
            playerIndex = BlamconDevices.PlayerIndexFrom(description);
            creationOrder = BlamconDevices.NextCreationOrder();
        }

        /// <summary>
        /// Use to send HID output report to this specific device.
        /// </summary>
        /// <param name="command">The HID output report.</param>
        public bool SendCommand(ref BlamconHIDOutputReport command) =>
            BlamconForceFeedback.Send(this, ref command, "command");

        /// <inheritdoc />
        public bool EnableFFBControl(bool recoil = true, bool rumble = true, bool led = true, bool ammo = false) =>
            BlamconForceFeedback.EnableFFBControl(this, recoil, rumble, led, ammo);
        /// <inheritdoc />
        public bool EnableAmmoFFBControl(bool ammo = true) =>
            BlamconForceFeedback.EnableAmmoFFBControl(this, ammo);

        /// <inheritdoc />
        public bool ActivateRumble(int pulses = 1) =>
            BlamconForceFeedback.ActivateRumble(this, pulses);
        /// <inheritdoc />
        public bool ActivateRumble(int pulse, int on, int off) =>
            BlamconForceFeedback.ActivateRumble(this, pulse, on, off);
        /// <inheritdoc />
        public bool ActivateRecoil(int pulse = 1) =>
            BlamconForceFeedback.ActivateRecoil(this, pulse);
        /// <inheritdoc />
        public bool ActivateRecoil(int pulse, int on, int off) =>
            BlamconForceFeedback.ActivateRecoil(this, pulse, on, off);
        /// <inheritdoc />
        public bool ActivateLED(int index, Color color) =>
            BlamconForceFeedback.ActivateLED(this, index, color);
        /// <inheritdoc />
        public bool ActivateLED(int index, Color color, int pulse) =>
            BlamconForceFeedback.ActivateLED(this, index, color, pulse);
        /// <inheritdoc />
        public bool SendAmmoCount(int remaining) =>
            BlamconForceFeedback.SendAmmoCount(this, remaining);
    }
}
