using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Layouts;

namespace Blamcon.Lightguns
{
    /// <summary>
    /// Identity and routing shared by the Blamcon device classes.
    /// </summary>
    /// <remarks>
    /// Kept apart from the device classes on purpose: touching a device class runs its static
    /// constructor, which registers layouts, and device setup must not do that part-way through
    /// creating a device.
    /// </remarks>
    internal static class BlamconDevices
    {
        public const int kPlayerCount = 4;
        public const int kVendorId = 0x3673;
        public const int kFirstProductId = 0x0100;

        static long s_CreationCounter;

        /// <summary>
        /// Picks the device that feedback for a player goes to: a gamepad device over a mouse-mode device,
        /// and the most recently added among the same kind.
        /// </summary>
        public static InputDevice SelectFeedbackDevice(IEnumerable<InputDevice> devices, int player)
        {
            InputDevice best = null;
            var bestIsGamepad = false;
            var bestOrder = 0L;
            foreach (var device in devices)
            {
                bool isGamepad;
                int index;
                long order;
                switch (device)
                {
                    case BlamconLightgunHID gun:
                        isGamepad = true;
                        index = gun.playerIndex;
                        order = gun.creationOrder;
                        break;
                    case BlamconMouseModeDevice mouseMode:
                        isGamepad = false;
                        index = mouseMode.playerIndex;
                        order = mouseMode.creationOrder;
                        break;
                    default:
                        continue;
                }

                if (index != player)
                    continue;
                if (best == null || (isGamepad && !bestIsGamepad) || (isGamepad == bestIsGamepad && order > bestOrder))
                {
                    best = device;
                    bestIsGamepad = isGamepad;
                    bestOrder = order;
                }
            }
            return best;
        }

        /// <summary>Firmware 1.0.16: the first that takes force feedback in Gamepad mode.</summary>
        public const int kMinFeedbackVersion = 10016;

        /// <summary>Firmware 2.1.0: the first with the vendor-defined collection, so feedback in mouse mode.</summary>
        public const int kMinMouseModeFeedbackVersion = 20100;

        /// <summary>Firmware 3.0.0: the first that shipped on the RP2350. Everything before it is an RP2040.</summary>
        public const int kFirstRp2350Version = 30000;

        /// <summary>
        /// Reads the firmware version out of a device description. The firmware sets the USB
        /// <c>bcdDevice</c> (and the Bluetooth device-ID record) to its version in BCD as <c>0xJJMN</c>, so
        /// 3.0.0 arrives as 768 and 2.0.1 as 513.
        /// </summary>
        /// <param name="value">The description's version field.</param>
        /// <param name="number">major * 10000 + minor * 100 + patch, or 0 when it couldn't be read.</param>
        /// <param name="text">"3.0.0", or empty when it couldn't be read.</param>
        /// <returns>False when the field is missing, or holds something that isn't a Blamcon BCD version.</returns>
        public static bool TryParseFirmwareVersion(string value, out int number, out string text)
        {
            number = 0;
            text = string.Empty;
            if (!int.TryParse(value, out var bcd) || bcd <= 0 || bcd > 0xFFFF)
                return false;

            var majorTens = (bcd >> 12) & 0xF;
            var majorUnits = (bcd >> 8) & 0xF;
            var minor = (bcd >> 4) & 0xF;
            var patch = bcd & 0xF;
            // Every digit of a BCD version is 0-9. Anything else is some other device's numbering.
            if (majorTens > 9 || majorUnits > 9 || minor > 9 || patch > 9)
                return false;

            var major = majorTens * 10 + majorUnits;
            number = major * 10000 + minor * 100 + patch;
            text = $"{major}.{minor}.{patch}";
            return true;
        }

        /// <summary>Describes a player's gun, from the device that answers for it and its description.</summary>
        public static BlamconLightgunInfo BuildInfo(IEnumerable<InputDevice> devices, int player)
        {
            var device = SelectFeedbackDevice(devices, player);
            if (device == null)
                return new BlamconLightgunInfo { playerIndex = -1, firmwareVersion = string.Empty, productName = string.Empty };

            var gamepad = device is BlamconLightgunHID;
            var known = TryParseFirmwareVersion(device.description.version, out var number, out var text);
            var minimum = gamepad ? kMinFeedbackVersion : kMinMouseModeFeedbackVersion;
            return new BlamconLightgunInfo
            {
                connected = true,
                playerIndex = player,
                hasGunInput = gamepad,
                // An unknown version doesn't mean no feedback: the gun is reachable, or it wouldn't be here.
                feedbackAvailable = !known || number >= minimum,
                detailsKnown = known,
                firmwareVersion = text,
                firmwareVersionNumber = number,
                board = !known ? LightgunBoard.Unknown
                    : number >= kFirstRp2350Version ? LightgunBoard.RP2350 : LightgunBoard.RP2040,
                mode = gamepad ? LightgunMode.Gamepad : LightgunMode.Mouse,
                playerNumberOnGun = player + 1,
                productName = device.description.product ?? string.Empty,
            };
        }

        /// <summary>0-based player index (0-3) from a Blamcon HID product ID, or -1 for anything else.</summary>
        public static int PlayerIndexFrom(InputDeviceDescription description)
        {
            if (description.interfaceName != "HID" || string.IsNullOrEmpty(description.capabilities))
                return -1;
            var hid = HID.HIDDeviceDescriptor.FromJson(description.capabilities);
            var index = hid.productId - kFirstProductId;
            return hid.vendorId == kVendorId && index >= 0 && index < kPlayerCount ? index : -1;
        }

        public static long NextCreationOrder() => ++s_CreationCounter;
    }
}
