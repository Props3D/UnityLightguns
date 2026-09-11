using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.InteropServices;
using System;

namespace Blamcon.Lightguns.LowLevel
{
    // BlamconLEDCommand remains the same as it defines an output command.
    // Ensure its size and offsets are correct for your device's output report.
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct BlamconLEDCommand : IBlamconCommand
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        // The size of bytes here needs to match the largest output report accepted by the device?
        internal const int kSize = InputDeviceCommand.BaseCommandSize + 40;
        internal const int kReportId = 0x22; // Must match the device's output report ID

        [FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public byte enableFFBControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte ledRed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public byte ledGreen;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public byte ledBlue;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 5)] public byte ledIndex;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 6)] public byte ledFlash;
        // The firmware labels byte 7 "on" and byte 9 "off", but its LED pulse stays dark for
        // byte 7's period and lit for byte 9's. These names reflect the actual behavior.
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 7)] public short ledFlashOffPeriod;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 9)] public short ledFlashOnPeriod;

        public FourCC typeStatic => Type;
        public void EnableFFBControl(bool enable = true)
        {
            enableFFBControl = enable ? (byte)3 : (byte)2;
        }
        public void SetColor(int index, Color color)
        {
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
            ledFlashOnPeriod = (short)Math.Clamp(on, 20, 5000);
            ledFlashOffPeriod = (short)Math.Clamp(off, 20, 5000);
        }

        public static BlamconLEDCommand Create(int index, Color color) // Removed size param if it's fixed
        {
            return new BlamconLEDCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                ledRed = (byte)Mathf.Clamp(color.r * 255, 0, 255),
                ledGreen = (byte)Mathf.Clamp(color.g * 255, 0, 255),
                ledBlue = (byte)Mathf.Clamp(color.b * 255, 0, 255),
                ledIndex = (byte)Math.Clamp(index, 0, 1)
            };
        }
        public static BlamconLEDCommand Create(int index, Color color, int flash) // Removed size param if it's fixed
        {
            return new BlamconLEDCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                ledRed = (byte)Mathf.Clamp(color.r * 255, 0, 255),
                ledGreen = (byte)Mathf.Clamp(color.g * 255, 0, 255),
                ledBlue = (byte)Mathf.Clamp(color.b * 255, 0, 255),
                ledIndex = (byte)Math.Clamp(index, 0, 1),
                ledFlash = (byte)Math.Clamp(flash, 0, 10)
            };
        }
        public static BlamconLEDCommand Create(int index, Color color, int flash, int on, int off) // Removed size param if it's fixed
        {
            return new BlamconLEDCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                ledRed = (byte)Mathf.Clamp(color.r * 255, 0, 255),
                ledGreen = (byte)Mathf.Clamp(color.g * 255, 0, 255),
                ledBlue = (byte)Mathf.Clamp(color.b * 255, 0, 255),
                ledIndex = (byte)Math.Clamp(index, 0, 1),
                ledFlash = (byte)Math.Clamp(flash, 0, 10),
                ledFlashOnPeriod = (short)Math.Clamp(on, 20, 5000),
                ledFlashOffPeriod = (short)Math.Clamp(off, 20, 5000)
            };
        }
    }
}
