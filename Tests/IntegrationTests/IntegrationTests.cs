using System.Collections;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
using Blamcon.Lightguns;
using Blamcon.Lightguns.LowLevel;

#if UNITY_EDITOR
using UnityEngine.InputSystem.Editor;
#endif

// Disable irrelevant warning about there not being underscores in method names.
#pragma warning disable CA1707

// These tests are the only ones that we put *in* the package. The rest of our tests live in Assets/Tests and run separately
// from our CI and not through upm-ci. This also means that IntegrationTests is the only thing we put on trunk through our
// verified package.
//
// Rationale:
// (1) Our APIVerificationTests have extra package requirements and thus need a custom package manifest.json. This will not
//     work with upm-ci.
// (2) The tests we have in Assets/Tests exercise the input system in isolation. Having these run on trunk in addition to our
//     CI in the input system repo adds little value while adding extra execution time to trunk QV runs. This is unlike
//     the integration tests here which add value to trunk by making sure the input system is intact all the way through
//     to the native input module.
// (3) If we added everything in Assets/Tests to the package, we would add more stuff to user projects that has no value to users.
//
// NOTE: The tests here are necessary to pass the requirement imposed by upm-ci that a package MUST have tests in it.

public class IntegrationTests
{
    /// <summary>
    /// Raw gamepad input report exactly as the Blamcon firmware (3.0.0) sends it over USB:
    /// report id, 32 button bits, 4-bit hat + 4-bit pad, then X/Y/Rx/Ry as 32-bit values.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 22)]
    struct FirmwareGamepadReport : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('H', 'I', 'D');

        [FieldOffset(0)] public byte reportId;
        [FieldOffset(1)] public uint buttons;
        [FieldOffset(5)] public byte hat;
        [FieldOffset(6)] public int x;
        [FieldOffset(10)] public int y;
        [FieldOffset(14)] public int rx;
        [FieldOffset(18)] public int ry;
    }

    [Preserve]
    public static void PreserveMethods()
    {
        // Workaround a bug in com.unity.test-framework.utp-reporter
        // Due Stripping set to to High System.ComponentModel.StringConverter ctor is stripped, making first test Integration_CanSendAndReceiveEvents to fail
        var dummy = new System.ComponentModel.StringConverter();
    }

    [SetUp]
    public virtual void Setup()
    {
        // The standalone player can go out of focus when running tests. This makes the devices added during tests set
        // as disabled. Which in turn makes the InputSystem update not process input state changes for devices.
        // By ignoring focus, we can protect ourselves against this and always update the device state in the standalone
        // test players
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
    }

    [TearDown]
    public virtual void TearDown()
    {
        InputSystem.settings.backgroundBehavior = default;
    }

    [Test]
    [Category("Integration")]
    public void Integration_CanSendAndReceiveEvents()
    {
        var lightgun = InputSystem.AddDevice<BlamconLightgunHID>();

        try
        {
            // bit 1 = buttonSouth (A); bit 0 (trigger) must stay released
            InputSystem.QueueStateEvent(lightgun, new FirmwareGamepadReport { reportId = 1, buttons = 1u << 1 });
            InputSystem.Update();

            Assert.That(lightgun.buttonSouth.isPressed, Is.True);
            Assert.That(lightgun.buttonWest.isPressed, Is.False);

            InputSystem.QueueStateEvent(lightgun, new FirmwareGamepadReport { reportId = 1, buttons = 1u << 0 });
            InputSystem.Update();

            Assert.That(lightgun.buttonWest.isPressed, Is.True);
            Assert.That(lightgun.buttonSouth.isPressed, Is.False);
        }
        finally
        {
            InputSystem.RemoveDevice(lightgun);
        }
    }

    [Test]
    [Category("Integration")]
    public void Integration_UnknownReportIdIsIgnored()
    {
        var lightgun = InputSystem.AddDevice<BlamconLightgunHID>();

        try
        {
            InputSystem.QueueStateEvent(lightgun, new FirmwareGamepadReport { reportId = 0x10 });
            InputSystem.Update();

            Assert.That(lightgun.buttonWest.isPressed, Is.False);
        }
        finally
        {
            InputSystem.RemoveDevice(lightgun);
        }
    }

    [Test]
    [Category("Integration")]
    public void Integration_HatCardinalsOnly()
    {
        var lightgun = InputSystem.AddDevice<BlamconLightgunHID>();

        try
        {
            InputSystem.QueueStateEvent(lightgun, new FirmwareGamepadReport { reportId = 1, hat = 3 }); // right
            InputSystem.Update();

            Assert.That(lightgun.dpad.right.isPressed, Is.True);
            Assert.That(lightgun.dpad.up.isPressed, Is.False);

            // Diagonals are intentionally not supported.
            InputSystem.QueueStateEvent(lightgun, new FirmwareGamepadReport { reportId = 1, hat = 2 }); // up/right
            InputSystem.Update();

            Assert.That(lightgun.dpad.up.isPressed, Is.False);
            Assert.That(lightgun.dpad.right.isPressed, Is.False);
        }
        finally
        {
            InputSystem.RemoveDevice(lightgun);
        }
    }

    [Test]
    [Category("Integration")]
    public void Integration_PositionDecodesRawCoordinates()
    {
        var lightgun = InputSystem.AddDevice<BlamconLightgunHID>();

        try
        {
            InputSystem.QueueStateEvent(lightgun, new FirmwareGamepadReport { reportId = 3, x = 32767, y = 8192 });
            InputSystem.Update();

            Assert.That(lightgun.position.ReadUnprocessedValue(), Is.EqualTo(new Vector2(32767, 8192)));
        }
        finally
        {
            InputSystem.RemoveDevice(lightgun);
        }
    }

    // Offsets are relative to the start of the HID output report (byte 0 = report id), matching
    // handleDefaultOutputReport / handleLedOutputReport in the firmware's main.cpp.
    static int ReportOffset<T>(string field) =>
        Marshal.OffsetOf(typeof(T), field).ToInt32() - InputDeviceCommand.BaseCommandSize;

    [Test]
    [Category("Integration")]
    public void OutputReport_LayoutMatchesFirmware()
    {
        Assert.That(Marshal.SizeOf(typeof(BlamconHIDOutputReport)) - InputDeviceCommand.BaseCommandSize, Is.EqualTo(40));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("rumble"), Is.EqualTo(15));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("ledRed"), Is.EqualTo(20));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("ledFlash"), Is.EqualTo(24));
        // Firmware labels these the other way round, but its LED pulse is dark for the first
        // period and lit for the second (see ezRepeatFlash in easyledv4.h).
        Assert.That(ReportOffset<BlamconHIDOutputReport>("ledFlashOffPeriod"), Is.EqualTo(25));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("ledFlashOnPeriod"), Is.EqualTo(27));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("recoil"), Is.EqualTo(29));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("ammoRemaining"), Is.EqualTo(32));
        Assert.That(ReportOffset<BlamconHIDOutputReport>("ammoMax"), Is.EqualTo(34));

        Assert.That(ReportOffset<BlamconLEDCommand>("ledFlashOffPeriod"), Is.EqualTo(7));
        Assert.That(ReportOffset<BlamconLEDCommand>("ledFlashOnPeriod"), Is.EqualTo(9));
    }

    [Test]
    [Category("Integration")]
    public void RumbleCommand_SetRumbleWithTimingsSetsPulseCount()
    {
        var command = BlamconRumbleCommand.Create(1);
        command.SetRumble(3, 500, 400);

        Assert.That(command.rumble, Is.EqualTo(3));
        Assert.That(command.rumbleOnPeriod, Is.EqualTo(500));
        Assert.That(command.rumbleOffPeriod, Is.EqualTo(400));
    }
}
