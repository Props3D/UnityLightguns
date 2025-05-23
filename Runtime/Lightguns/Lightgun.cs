using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.InteropServices;
using Blamcon.Lightguns.LowLevel;

namespace Blamcon.Lightguns.LowLevel
{
    [StructLayout(LayoutKind.Explicit, Size = 14)]
    public struct LightgunState : IInputStateTypeInfo // Use IInputStateTypeInfo
    {
        public static FourCC kFormat = new FourCC('H','I','D');
        public FourCC format => kFormat;

        // HID input reports can start with an 8-bit report ID. We don't really need
        // to add the field, but let's do so for the sake of completeness. This can
        // also help with debugging.
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
        [InputControl(name = "dpad/up", layout="DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=1,maxValue=1")]
        [InputControl(name = "dpad/right", layout="DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=3,maxValue=3")]
        [InputControl(name = "dpad/down", layout="DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=5,maxValue=5")]
        [InputControl(name = "dpad/left", layout="DiscreteButton", offset = 0, bit = 0, format = "BIT", sizeInBits = 4, parameters = "minValue=7,maxValue=7")]
        public byte hat; // Assuming values 0-7 for directions, or 8/15 for neutral? Verify format.

        /// <summary>
        /// Position of the pointer in screen space.
        /// </summary>
        [FieldOffset(6)]
        [InputControl(name = "position", layout = "Vector2", displayName = "Position", usage = "Point", processors = "AbsolutePositionRemap", dontReset = true)]
        public Vector2 position; // this vector has 2 floats that requires 8 bytes in total

        [FieldOffset(14)]
        [InputControl(name = "secondaryMotion", layout = "Vector2", displayName = "Secondary Motion", usage = "Point", processors = "AbsolutePositionRemap", dontReset = true)]
        public Vector2 secondaryMotion; // this vector has 2 floats that requires 8 bytes in total

    }
}

namespace Blamcon.Lightguns
{
    /// <summary>
    /// Base class for lightgun-style devices moving on a 2D screen.
    /// </summary>
    /// <remarks>
    /// This class abstracts over general "pointing" behavior where a pointer is moved across a 2D
    /// surface.
    ///
    /// </remarks>
    [InputControlLayout(stateType = typeof(LightgunState), isGenericTypeOfDevice = true)]
    public class Lightgun : InputDevice, IInputStateCallbackReceiver
    {
        public ButtonControl buttonWest { get; protected set; }
        public ButtonControl buttonNorth { get; protected set; }
        public ButtonControl buttonSouth { get; protected set; }
        public ButtonControl buttonEast { get; protected set; }
        public ButtonControl leftStickButton { get; protected set; }
        public ButtonControl rightStickButton { get; protected set; }
        public ButtonControl startButton { get; protected set; }
        public ButtonControl selectButton { get; protected set; }
        public ButtonControl leftShoulder { get; protected set; }
        public ButtonControl rightShoulder { get; protected set; }
        public ButtonControl leftTrigger { get; protected set; }
        public ButtonControl rightTrigger { get; protected set; }
        public ButtonControl aButton => buttonSouth;
        public ButtonControl bButton => buttonEast;
        public ButtonControl xButton => buttonWest;
        public ButtonControl yButton => buttonNorth;
        public ButtonControl triangleButton => buttonNorth;
        public ButtonControl squareButton => buttonWest;
        public ButtonControl circleButton => buttonEast;
        public ButtonControl crossButton => buttonSouth;
        public DpadControl dpad { get; protected set; }

        /// <summary>
        /// The current pointer coordinates in window space.
        /// </summary>
        /// <value>Control representing the current position of the pointer on screen.</value>
        /// <remarks>
        /// Within player code, the coordinates are in the coordinate space of Unity's <c>Display</c>.
        ///
        /// Within editor code, the coordinates are in the coordinate space of the current <c>EditorWindow</c>
        /// This means that if you query the <see cref="Mouse"/> <see cref="position"/> in <c>EditorWindow.OnGUI</c>, for example,
        /// the returned 2D vector will be in the coordinate space of your local GUI (same as
        /// <c>Event.mousePosition</c>).
        /// </remarks>
        public Vector2Control position { get; protected set; }

        /// <summary>
        /// If the controller is connected over HID, returns <see cref="HID.HID.HIDDeviceDescriptor"/> data parsed from <see cref="InputDeviceDescription.capabilities"/>.
        /// </summary>
        internal HID.HIDDeviceDescriptor hidDescriptor { get; private set; }

        /// <inheritdoc />
        protected override void FinishSetup()
        {
            buttonWest = GetChildControl<ButtonControl>("buttonWest");
            buttonNorth = GetChildControl<ButtonControl>("buttonNorth");
            buttonSouth = GetChildControl<ButtonControl>("buttonSouth");
            buttonEast = GetChildControl<ButtonControl>("buttonEast");
            startButton = GetChildControl<ButtonControl>("start");
            selectButton = GetChildControl<ButtonControl>("select");
            leftStickButton = GetChildControl<ButtonControl>("leftStickPress");
            rightStickButton = GetChildControl<ButtonControl>("rightStickPress");
            leftShoulder = GetChildControl<ButtonControl>("leftShoulder");
            rightShoulder = GetChildControl<ButtonControl>("rightShoulder");
            leftTrigger = GetChildControl<ButtonControl>("leftTrigger");
            rightTrigger = GetChildControl<ButtonControl>("rightTrigger");
            dpad = GetChildControl<DpadControl>("dpad");

            position = GetChildControl<Vector2Control>("position");

            base.FinishSetup();

            if (base.description.capabilities != null && base.description.interfaceName == "HID")
                hidDescriptor = HID.HIDDeviceDescriptor.FromJson(base.description.capabilities);
        }

        public static Lightgun current { get; internal set; }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            if (current == this)
            {
                current = null;
            }
        }

        /// <summary>
        /// Called whenever the input system advances by one frame.
        /// </summary>
        /// <seealso cref="InputSystem.Update"/>
        virtual protected void OnNextUpdate()
        {
            // InputState.Change(delta, Vector2.zero);
        }

        /// <summary>
        /// Called when the pointer receives a state event.
        /// </summary>
        /// <param name="eventPtr">The input event.</param>
        virtual protected unsafe void OnStateEvent(InputEventPtr eventPtr)
        {
            // Debug.Log($"New state event: {stateEventPtr->stateFormat}");
            InputState.Change(this, eventPtr);
        }

        void IInputStateCallbackReceiver.OnNextUpdate()
        {
            OnNextUpdate();
        }

        void IInputStateCallbackReceiver.OnStateEvent(InputEventPtr eventPtr)
        {
            OnStateEvent(eventPtr);
        }

        bool IInputStateCallbackReceiver.GetStateOffsetForEvent(InputControl control, InputEventPtr eventPtr, ref uint offset)
        {
            return false;
        }
    }
}
