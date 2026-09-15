using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Records the HID output commands ('HIDO') sent to one device, without hardware, by handling them
/// in <see cref="InputSystem.onDeviceCommand"/>. Handled commands report success, so the device under
/// test doesn't log a send failure.
/// </summary>
/// <remarks>
/// Always dispose before removing the device. <see cref="InputDevice.ExecuteCommand{TCommand}"/>
/// (Input System 1.14) returns from inside its callback loop without unlocking the callback list when
/// a callback handles a command, so unsubscribing can stay pending and leak this callback into later
/// tests. Subscribing while the list is locked is deferred the same way. So the constructor and
/// <see cref="Dispose"/> each send one command that no callback handles, which lets the loop run to the
/// end and apply pending changes. Harmless if a later Input System fixes the loop.
/// </remarks>
internal sealed unsafe class HidoCapture : IDisposable
{
    public struct Command
    {
        public int sizeInBytes;
        public byte[] payload;
    }

    static readonly FourCC k_HidOutput = new FourCC('H', 'I', 'D', 'O');

    readonly InputDevice m_Device;
    readonly InputDeviceCommandDelegate m_Callback;
    bool m_Capturing = true;

    public readonly List<Command> commands = new List<Command>();

    public HidoCapture(InputDevice device)
    {
        m_Device = device;
        m_Callback = OnDeviceCommand;
        FlushCallbackChanges();
        InputSystem.onDeviceCommand += m_Callback;
    }

    long? OnDeviceCommand(InputDevice device, InputDeviceCommand* command)
    {
        if (!m_Capturing || device != m_Device || command->type != k_HidOutput)
            return null;

        var payload = new byte[command->payloadSizeInBytes];
        Marshal.Copy((IntPtr)command->payloadPtr, payload, 0, payload.Length);
        commands.Add(new Command { sizeInBytes = command->sizeInBytes, payload = payload });
        return 1;
    }

    public void Dispose()
    {
        m_Capturing = false;
        InputSystem.onDeviceCommand -= m_Callback;
        FlushCallbackChanges();
    }

    void FlushCallbackChanges()
    {
        var command = QueryEnabledStateCommand.Create();
        m_Device.ExecuteCommand(ref command);
    }
}
