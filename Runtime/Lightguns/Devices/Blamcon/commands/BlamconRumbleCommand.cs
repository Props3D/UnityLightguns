using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.InteropServices;
using System;

namespace Blamcon.Lightguns.LowLevel
{
    // BlamconRumbleCommand remains the same as it defines an output command.
    // Ensure its size and offsets are correct for your device's output report.
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct BlamconRumbleCommand : IBlamconCommand
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        // The size of bytes here needs to match the largest output report accepted by the device?
        internal const int kSize = InputDeviceCommand.BaseCommandSize + 40;
        internal const int kReportId = 0x21; // Must match the device's output report ID

        [FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public byte enableFFBControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte rumble;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public short rumbleOnPeriod;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 5)] public short rumbleOffPeriod;

        public FourCC typeStatic => Type;
        public void EnableFFBControl(bool enable = true)
        {
            enableFFBControl = enable ? (byte)3 : (byte)2;
        }
        public void SetRumble(int pulse)
        {
            rumble = (byte)Math.Clamp(pulse, 0, 10);
        }
        public void SetRumble(int pulse, int on, int off)
        {
            rumbleOnPeriod = (short)Math.Clamp(on, 100, 2000);
            rumbleOffPeriod = (short)Math.Clamp(off, 100, 2000);
        }

        public static BlamconRumbleCommand Create(int pulse) // Removed size param if it's fixed
        {
            return new BlamconRumbleCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                rumble = (byte)Math.Clamp(pulse, 0, 10)
            };
        }
        public static BlamconRumbleCommand Create(int pulse, int on, int off) // Removed size param if it's fixed
        {
            return new BlamconRumbleCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                rumble = (byte)Math.Clamp(pulse, 0, 10),
                rumbleOnPeriod = (short)Math.Clamp(on, 100, 2000),
                rumbleOffPeriod = (short)Math.Clamp(off, 100, 2000)
            };
        }
    }
}
