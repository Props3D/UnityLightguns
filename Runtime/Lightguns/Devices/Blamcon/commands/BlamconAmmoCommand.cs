using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.InteropServices;
using System;

namespace Blamcon.Lightguns.LowLevel
{
    // BlamconAmmoCommand remains the same as it defines an output command.
    // Ensure its size and offsets are correct for your device's output report.
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct BlamconAmmoCommand : IBlamconCommand
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        // The size of bytes here needs to match the largest output report accepted by the device?
        internal const int kSize = InputDeviceCommand.BaseCommandSize + 40;
        internal const int kReportId = 0x23; // Must match the device's output report ID

        [FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public byte enableFFBControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte ammoRemaining;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public byte ammoUsed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public byte ammoMax;

        public FourCC typeStatic => Type;
        public void EnableFFBControl(bool enable = true)
        {
            enableFFBControl = enable ? (byte)3 : (byte)2;
        }
        public void SetAmmo(int remaining) {
            ammoRemaining = (byte)remaining;
        }
        public static BlamconAmmoCommand Create(int remaining) // Removed size param if it's fixed
        {
            return new BlamconAmmoCommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize), // Use kSize
                reportId = kReportId,
                ammoRemaining = (byte)remaining
            };
        }
    }
}
