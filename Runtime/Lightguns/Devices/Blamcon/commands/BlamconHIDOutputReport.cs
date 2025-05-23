using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.InteropServices;
using System;

namespace Blamcon.Lightguns.LowLevel
{
    /// <summary>
    /// Interface implemented by all input device command structs which reports the data format identifier of the command.
    /// </summary>
    public interface IBlamconCommand : IInputDeviceCommandInfo
    {
    }

    // BlamconHIDOutputReport remains the same as it defines an output command.
    // Ensure its size and offsets are correct for your device's output report.
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct BlamconHIDOutputReport : IBlamconCommand
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        internal const int kSize = InputDeviceCommand.BaseCommandSize + 40;
        internal const int kReportId = 16; // Make sure this matches your device's output report ID

        [FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public byte EnableRumbleUpdate;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte EnableRumbleFFBControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public byte EnableLedUpdate;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public byte EnableLedFFBControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 5)] public byte EnableRecoilUpdate;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 6)] public byte EnableRecoilFFBControl;

        // [FieldOffset(InputDeviceCommand.BaseCommandSize + 7)] public fixed byte pad1[8];

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 15)] public byte rumble;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 16)] public short rumbleOnPeriod;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 18)] public short rumbleOffPeriod;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 20)] public byte ledRed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 21)] public byte ledGreen;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 22)] public byte ledBlue;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 23)] public byte ledIndex;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 24)] public byte ledFlash;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 25)] public short ledFlashOffPeriod;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 27)] public short ledFlashOnPeriod;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 29)] public byte recoil;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 30)] public byte recoilOnPeriod;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 31)] public byte recoilOffPeriod;

        // [FieldOffset(InputDeviceCommand.BaseCommandSize + 32)] public fixed byte pad2[8];

        public FourCC typeStatic => Type;
        public void EnableFFBControl(bool recoil = true, bool rumble = true, bool led = true)
        {
            EnableRecoilUpdate = 1;
            EnableRecoilFFBControl = (recoil == true) ? (byte)3 : (byte)2;
            EnableRumbleUpdate = 1;
            EnableRumbleFFBControl = (rumble == true) ? (byte)3 : (byte)2;
            EnableLedUpdate = 1;
            EnableLedFFBControl = (led == true) ? (byte)3 : (byte)2;
        }
        public void SetRumble(int pulse)
        {
            EnableRumbleUpdate = 1;
            rumble = (byte)Math.Clamp(pulse, 0, 10);
        }
        public void SetRumble(int pulse, int on, int off)
        {
            SetRumble(pulse);
            rumbleOnPeriod = (short)Math.Clamp(on, 100, 2000);
            rumbleOffPeriod = (short)Math.Clamp(off, 100, 2000);
        }
        public void SetColor(int index, Color color)
        {
            EnableLedUpdate = 1;
            ledRed = (byte)Mathf.Clamp(color.r * 255, 0, 255);
            ledGreen = (byte)Mathf.Clamp(color.g * 255, 0, 255);
            ledBlue = (byte)Mathf.Clamp(color.b * 255, 0, 255);
            ledIndex = (byte)index;
        }
        public void SetColor(int index, Color color, int pulse)
        {
            SetColor(index, color);
            ledFlash = (byte)Math.Clamp(pulse, 0, 100);
        }
        public void SetColor(int index, Color color, int pulse, int on, int off)
        {
            SetColor(index, color, pulse);
            ledFlashOnPeriod = (short)Math.Clamp(on, 40, 2000);
            ledFlashOffPeriod = (short)Math.Clamp(off, 40, 2000);
        }
        public void SetRecoil(int pulse) {
            EnableRecoilUpdate = 1;
            recoil = (byte)Math.Clamp(pulse, 0, 10);
        }
        public void SetRecoil(int pulse, int on, int off) {
            SetRecoil(pulse);
            recoilOnPeriod = (byte)Math.Clamp(on, 15, 255);
            recoilOffPeriod = (byte)Math.Clamp(off, 15, 255);
        }
        public static BlamconHIDOutputReport Create() // Removed size param if it's fixed
        {
            return new BlamconHIDOutputReport
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
            };
        }
        public static BlamconHIDOutputReport Create(bool recoil = true, bool rumble = true, bool led = true) // Removed size param if it's fixed
        {
            return new BlamconHIDOutputReport
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                EnableRecoilUpdate = 1,
                EnableRecoilFFBControl = (recoil == true) ? (byte)3 : (byte)2,
                EnableRumbleUpdate = 1,
                EnableRumbleFFBControl = (rumble == true) ? (byte)3 : (byte)2,
                EnableLedUpdate = 1,
                EnableLedFFBControl = (led == true) ? (byte)3 : (byte)2
            };
        }
    }
}
