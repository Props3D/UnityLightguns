using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

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
