using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.InteropServices;
using System;

namespace Blamcon.Lightguns.LowLevel
{
    // BlamconRecoilCommand remains the same as it defines an output command.
    // Ensure its size and offsets are correct for your device's output report.
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct BlamconRecoilCommand : IBlamconCommand
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        // The size of bytes here needs to match the largest output report accepted by the device?
        internal const int kSize = InputDeviceCommand.BaseCommandSize + 40;
        internal const int kReportId = 0x20; // Must match the device's output report ID

        [FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public byte enableFFBControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte recoil;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public byte recoilOnPeriod;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public byte recoilOffPeriod;

        public FourCC typeStatic => Type;
        public void EnableFFBControl(bool enable = true)
        {
            enableFFBControl = enable ? (byte)3 : (byte)2;
        }
        public void SetRecoil(int pulse) {
            recoil = (byte)Math.Clamp(pulse, 0, 10);
        }
        public void SetRecoil(int pulse, int on, int off) {
            SetRecoil(pulse);
            recoilOnPeriod = (byte)Math.Clamp(on, 15, 255);
            recoilOffPeriod = (byte)Math.Clamp(off, 15, 255);
        }

        public static BlamconRecoilCommand Create(int pulse) // Removed size param if it's fixed
        {
            return new BlamconRecoilCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                recoil = (byte)Math.Clamp(pulse, 0, 10)
            };
        }
        public static BlamconRecoilCommand Create(int pulse, int on, int off) // Removed size param if it's fixed
        {
            return new BlamconRecoilCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                recoil = (byte)Math.Clamp(pulse, 0, 10),
                recoilOnPeriod = (byte)Math.Clamp(on, 15, 255),
                recoilOffPeriod = (byte)Math.Clamp(off, 15, 255)
            };
        }
    }
}
