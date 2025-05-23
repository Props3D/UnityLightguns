using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using Blamcon.Lightguns.LowLevel;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Blamcon.Lightguns.LowLevel
{
    [StructLayout(LayoutKind.Explicit, Size = 22)]
    public struct BlamconLightgunState : IInputStateTypeInfo
    {
        public static FourCC kFormat = new FourCC('H', 'I', 'D');
        // public static FourCC kFormat = new FourCC('L','G','V','S');
        public FourCC format => kFormat;

        // HID input reports can start with an 8-bit report ID. It depends on the device
        // whether this is present or not. On Blamcon lightguns, it is present.
        // We don't really need to add the field, but let's do so for the sake of
        // completeness. This can also help with debugging.
        [FieldOffset(0)]
        public byte reportId;

        // ─── Button Bitmask @ byte 0 (4 bytes) ───────────────────────
        // Define the raw data field. Mapping individual buttons will happen in the class.
        [FieldOffset(1)]
        [InputControl(name = "buttonWest", layout = "Button", offset = 1, bit = 0, usages = new string[] { "PrimaryAction", "Submit" }, aliases = new string[] { "x", "square" }, displayName = "Button West", shortDisplayName = "X")]
        [InputControl(name = "buttonSouth", layout = "Button", offset = 1, bit = 1, usages = new string[] { "SecondaryAction", "Reload" }, aliases = new string[] { "a", "cross" }, displayName = "Button South", shortDisplayName = "A")]
        [InputControl(name = "buttonEast", layout = "Button", offset = 1, bit = 2, usages = new string[] { "Back", "Cancel" }, aliases = new string[] { "b", "circle" }, displayName = "Button East", shortDisplayName = "B")]
        [InputControl(name = "buttonNorth", layout = "Button", offset = 1, bit = 3, aliases = new string[] { "y", "triangle" }, displayName = "Button North", shortDisplayName = "Y")]
        [InputControl(name = "leftShoulder", layout = "Button", offset = 1, bit = 4, displayName = "Left Shoulder", shortDisplayName = "LB")]
        [InputControl(name = "rightShoulder", layout = "Button", offset = 1, bit = 5, displayName = "Right Shoulder", shortDisplayName = "RB")]
        // Using 'Button' layout for triggers based on original struct. Rename if they are axes.
        [InputControl(name = "leftTrigger", usage = "SecondaryTrigger", displayName = "Left Trigger", layout = "Button", format = "BIT", offset = 1, bit = 6)]
        [InputControl(name = "rightTrigger", usage = "SecondaryTrigger", displayName = "Right Trigger", layout = "Button", format = "BIT", offset = 1, bit = 7)]
        [InputControl(name = "select", layout = "Button", offset = 1, bit = 8, displayName = "Select")]
        [InputControl(name = "start", layout = "Button", offset = 1, bit = 9, usage = "Menu", displayName = "Start")]
        [InputControl(name = "leftStickPress", layout = "Button", offset = 1, bit = 10, displayName = "Left Stick Press")]
        [InputControl(name = "rightStickPress", layout = "Button", offset = 1, bit = 11, displayName = "Right Stick Press")]
        public uint buttons;

        // ─── D‑Pad Value @ byte 5 (1 byte) ───────────────────────────
        // Define the raw data field. Mapping D-pad directions will happen in the class.
        [FieldOffset(5)]
        [InputControl(name = "dpad", layout = "Dpad", usage = "Hatswitch", displayName = "D-Pad", offset = 5, format = "BIT", sizeInBits = 4)] // sizeInBits should match the data size used for dpad
        [InputControl(name = "dpad/up", layout = "DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=1,maxValue=1")]
        [InputControl(name = "dpad/right", layout = "DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=3,maxValue=3")]
        [InputControl(name = "dpad/down", layout = "DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=5,maxValue=5")]
        [InputControl(name = "dpad/left", layout = "DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=7,maxValue=7")]
        public byte hat; // 1-7 for directions, and 0 for neutral

        [FieldOffset(6)]
#if UNITY_EDITOR
        [InputControl(name = "position", layout = "Vector2", displayName = "Position", usage = "Point", processors = "AbsolutePositionRemap", dontReset = true)]
#else
        [InputControl(name = "position", layout = "Vector2", displayName = "Position", usage = "Point", processors = "AbsolutePositionRemap", dontReset = true)]
#endif
        public Vector2 position; // this vector has 2 floats that requires 8 bytes in total

        [FieldOffset(14)]
#if UNITY_EDITOR
        [InputControl(name = "secondaryMotion", layout = "Vector2", displayName = "Secondary Motion", usage = "Point", processors = "AbsolutePositionRemap", dontReset = true)]
#else
        [InputControl(name = "secondaryMotion", layout = "Vector2", displayName = "SecondaryMotion", usage = "Point", processors = "AbsolutePositionRemap", dontReset = true)]
#endif
        public Vector2 secondaryMotion; // this vector has 2 floats that requires 8 bytes in total

        /// <summary>
        /// Set the button mask for the given button.
        /// </summary>
        /// <param name="button">Button whose state to set.</param>
        /// <param name="state">Whether to set the bit on or off.</param>
        /// <returns>The same MouseState with the change applied.</returns>
        /// <seealso cref="buttons"/>
        public BlamconLightgunState WithButton(LightgunButton button, bool state = true)
        {
            Debug.Assert((int)button < 16, $"Expected button < 16, so we fit into the 16 bit wide bitmask");
            var bit = 1U << (int)button;
            if (state)
                buttons |= (ushort)bit;
            else
                buttons &= (ushort)~bit;
            return this;
        }
    }
    public enum LightgunButton {
        BUTTON_WEST = 0,
        BUTTON_SOUTH,
        BUTTON_EAST,
        BUTTON_NORTH,
        BUTTON_LEFT_SHOULDER,
        BUTTON_RIGHT_SHOULDER,
        BUTTON_LEFT_TRIGGER,
        BUTTON_RIGHT_TRIGGER,
        BUTTON_SELECT,
        BUTTON_START,
        BUTTON_LSTICK,
        BUTTON_RSTICK
    }
}

namespace Blamcon.Lightguns
{
    [InputControlLayout(stateType = typeof(BlamconLightgunState),
                        isGenericTypeOfDevice = false)]
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] // Make sure static constructor is called during startup.
    #endif
    public class BlamconLightgunHID : Lightgun, IForceFeedback
    {
        static BlamconLightgunHID()
        {
            // TODO - move this to the base class?
            InputSystem.RegisterLayout<Lightgun>("LightGun");

            InputSystem.RegisterLayout<BlamconLightgunHID>("Blamcon Lightgun - P1",
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("usagePage", 0x01)     // Generic Desktop Controls.
                    .WithCapability("usage", 0x05)         // Gamepad.
                    .WithCapability("productId", 0x0100)   // Player 1
                    .WithCapability("vendorId", 0x3673));  // vendor ID.

            InputSystem.RegisterLayout<BlamconLightgunHID>("Blamcon Lightgun - P2",
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("usagePage", 0x01)     // Generic Desktop Controls.
                    .WithCapability("usage", 0x05)         // Gamepad.
                    .WithCapability("productId", 0x0101)   // Player 2
                    .WithCapability("vendorId", 0x3673));  // vendor ID.
            InputSystem.RegisterLayout<BlamconLightgunHID>("Blamcon Lightgun - P3",
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("usagePage", 0x01)     // Generic Desktop Controls.
                    .WithCapability("usage", 0x05)         // Gamepad.
                    .WithCapability("productId", 0x0102)   // Player 3
                    .WithCapability("vendorId", 0x3673));  // vendor ID.
            InputSystem.RegisterLayout<BlamconLightgunHID>("Blamcon Lightgun - P4",
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("usagePage", 0x01)     // Generic Desktop Controls.
                    .WithCapability("usage", 0x05)         // Gamepad.
                    .WithCapability("productId", 0x0103)   // Player 4
                    .WithCapability("vendorId", 0x3673));  // vendor ID.
        }

        // In the Player, to trigger the calling of the static constructor,
        // create an empty method annotated with RuntimeInitializeOnLoadMethod.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {}

        /// <inheritdoc />
        protected override void FinishSetup()
        {
            base.FinishSetup();
        }

        unsafe bool PreProcessEvent(InputEventPtr eventPtr)
        {
            if (eventPtr.type != StateEvent.Type)
                return eventPtr.type != DeltaStateEvent.Type; // only skip delta state events

            var stateEvent = (StateEvent*)eventPtr.data;

            var size = stateEvent->stateSizeInBytes;
            if (stateEvent->stateFormat != LightgunHIDInputReport.kFormat || size < sizeof(LightgunHIDInputReport))
                return false; // skip unrecognized state events otherwise they will corrupt control states

            if (size < sizeof(LightgunHIDInputReport))
                return false;

            var binaryData = (byte*)stateEvent->state;

            // report id
            switch (binaryData[0])
            {
                // normal USB report
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                {
                    // if (size < sizeof(LightgunHIDInputReport))
                    //     return false;
                    var data = ((LightgunHIDInputReport*)(binaryData))->ToHIDInputReport();
                    *((BlamconLightgunState*)stateEvent->state) = data;
                    stateEvent->stateFormat = BlamconLightgunState.kFormat;
                    return true;
                }
                default: {
                    var data = ((LightgunHIDInputReport*)(binaryData))->EmptyHIDInputReport();
                    *((BlamconLightgunState*)stateEvent->state) = data;
                    stateEvent->stateFormat = BlamconLightgunState.kFormat;
                    return true; // skip unrecognized reportId
                }
            }
        }

        /// <summary>
        /// Called when the device receives a state event.
        /// </summary>
        /// <param name="eventPtr">The input event.</param>
        protected override unsafe void OnStateEvent(InputEventPtr eventPtr)
        {
            var stateEventPtr = (StateEvent*)eventPtr.data;
            if (eventPtr.type == StateEvent.Type && stateEventPtr->stateFormat == LightgunHIDInputReport.kFormat)
            {
                if (false == PreProcessEvent(eventPtr))
                    return;
            }
            InputState.Change(this, eventPtr);
        }

        /// <summary>
        /// Use to send HID output report to this specific device.
        /// </summary>
        /// <param name="command">The HID output report.</param>
        public bool SendCommand(ref BlamconHIDOutputReport command)
        {
            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <summary>
        /// Use to send recoil command to this specific device.
        /// </summary>
        /// <param name="command">The recoil command.</param>
        public bool SendCommand(ref BlamconRecoilCommand command)
        {
            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <summary>
        /// Use to send recoil command to this specific device.
        /// </summary>
        /// <param name="command">The rumble command.</param>
        public bool SendCommand(ref BlamconRumbleCommand command)
        {
            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <summary>
        /// Use to send recoil command to this specific device.
        /// </summary>
        /// <param name="command">The LED command.</param>
        public bool SendCommand(ref BlamconLEDCommand command)
        {
            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send command to {name}. Error: {result}");
            return result >= 0;
        }

        /// <inheritdoc />
        public bool EnableFFBControl(bool recoil = true, bool rumble = true, bool led = true)
        {
            var command = BlamconHIDOutputReport.Create(recoil, rumble, led);
            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send rumble command to {name}. Error: {result}");
            return result >= 0;
        }

        /// <inheritdoc />
        public bool ActivateRumble(int pulses = 1)
        {
            var command = BlamconHIDOutputReport.Create();
            command.SetRumble(pulses);

            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send rumble command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <inheritdoc />
        public bool ActivateRumble(int pulse, int on, int off)
        {
            var command = BlamconHIDOutputReport.Create();
            command.SetRumble(pulse, on, off);

            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send rumble command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <inheritdoc />
        public bool ActivateRecoil(int pulse = 1)
        {
            var command = BlamconHIDOutputReport.Create();
            command.SetRecoil(pulse);

            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send recoil command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <inheritdoc />
        public bool ActivateRecoil(int pulse, int on, int off)
        {
            var command = BlamconHIDOutputReport.Create();
            command.SetRecoil(pulse, on, off);

            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send recoil command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <inheritdoc />
        public bool ActivateLED(int index, Color color)
        {
            var command = BlamconHIDOutputReport.Create();
            command.SetColor(index, color);

            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send LED command to {name}. Error: {result}");
            return result >= 0;
        }
        /// <inheritdoc />
        public bool ActivateLED(int index, Color color, int pulse)
        {
            var command = BlamconHIDOutputReport.Create();
            command.SetColor(index, color, pulse);

            // Send the command to the device
            long result = ExecuteCommand(ref command);
            if (result < 0)
                Debug.LogError($"Failed to send LED command to {name}. Error: {result}");
            return result >= 0;
        }


        [StructLayout(LayoutKind.Explicit, Size = 22)]
        internal struct LightgunHIDInputReport
        {
            public static FourCC kFormat => new FourCC('H', 'I', 'D');
            public FourCC format => kFormat;

            [FieldOffset(0)] public byte reportId;
            [FieldOffset(1)] public uint buttons;
            [FieldOffset(5)] public byte hat;
            [FieldOffset(6)] public short leftStickX;
            [FieldOffset(10)] public short leftStickY;
            [FieldOffset(14)] public short rightStickX;
            [FieldOffset(18)] public short rightStickY;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BlamconLightgunState EmptyHIDInputReport()
            {
                return new BlamconLightgunState
                {
                    reportId = 0x05,
                    buttons = 0x01,
                    hat = 0,
                    position = Vector2.zero,
                    secondaryMotion = Vector2.zero
                };
            }
            public BlamconLightgunState ToHIDInputReport()
            {
                return new BlamconLightgunState
                {
                    reportId = reportId,
                    buttons = buttons,
                    hat = hat,
                    position = GetPrimary(),
                    secondaryMotion = GetSecondary()
                };
            }
            public Vector2 GetPrimary()
            {
                // TODO - normalize and convert to screen? 
                return new Vector2
                {
                    x = leftStickX,
                    y = leftStickY
                };
            }
            public Vector2 GetSecondary()
            {
                // TODO - normalize and convert to screen?
                return new Vector2
                {
                    x = rightStickX,
                    y = rightStickY
                };
            }
        }
    }
}

/**
Generated Device JSON Layout for reference

*/