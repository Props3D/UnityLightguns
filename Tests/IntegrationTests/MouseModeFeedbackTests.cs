using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Layouts;
using Blamcon.Lightguns;
using Blamcon.Lightguns.LowLevel;

/// <summary>
/// Mouse-mode feedback through the vendor-defined HID collection, and routing feedback to a player's gun.
/// </summary>
/// <remarks>
/// Devices here are created from HID device descriptions, the way Unity's native backend creates them.
/// Routing is tested through <see cref="BlamconDevices.SelectFeedbackDevice"/> with only the devices a
/// test created, so real guns plugged in during a test run can't change the result or receive commands.
/// </remarks>
public class MouseModeFeedbackTests
{
    const int kVendorId = 0x3673;
    const int kFirstProductId = 0x0100;
    const HID.UsagePage kGenericDesktop = (HID.UsagePage)0x01;
    const HID.UsagePage kVendorDefined = (HID.UsagePage)0xFF00;

    readonly List<InputDevice> m_Added = new List<InputDevice>();

    [SetUp]
    public void Setup()
    {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var device in m_Added)
        {
            if (device.added)
                InputSystem.RemoveDevice(device);
        }
        m_Added.Clear();
        InputSystem.settings.backgroundBehavior = default;
    }

    static InputDeviceDescription HidDescription(HID.UsagePage usagePage, int usage, int productId, int vendorId = kVendorId)
    {
        var descriptor = new HID.HIDDeviceDescriptor
        {
            vendorId = vendorId,
            productId = productId,
            usagePage = usagePage,
            usage = usage,
            outputReportSize = 40,
        };
        return new InputDeviceDescription
        {
            interfaceName = "HID",
            manufacturer = "Props3D",
            product = "Blamcon Lightgun (test)",
            capabilities = descriptor.ToJson(),
        };
    }

    InputDevice AddMouseMode(int player) => Add(HidDescription(kVendorDefined, 0x01, kFirstProductId + player));
    InputDevice AddGamepad(int player) => Add(HidDescription(kGenericDesktop, 0x05, kFirstProductId + player));

    InputDevice Add(InputDeviceDescription description)
    {
        var device = InputSystem.AddDevice(description);
        m_Added.Add(device);
        return device;
    }

    static void AssertSends(InputDevice device, Func<IForceFeedback, bool> send, byte[] expected)
    {
        using (var capture = new HidoCapture(device))
        {
            Assert.That(send((IForceFeedback)device), Is.True, "feedback call reported failure");
            Assert.That(capture.commands.Count, Is.EqualTo(1), "expected exactly one HIDO command");
            Assert.That(capture.commands[0].payload, Is.EqualTo(expected));
        }
    }

    // --- device creation -------------------------------------------------------------------------

    [Test]
    [Category("MouseMode")]
    public void VendorCollection_CreatesMouseModeDevice()
    {
        var device = AddMouseMode(0);

        Assert.That(device, Is.InstanceOf<BlamconMouseModeDevice>());
        Assert.That(device, Is.InstanceOf<IForceFeedback>());
        Assert.That(((BlamconMouseModeDevice)device).playerIndex, Is.EqualTo(0));
    }

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_IsNotALightgunAndHasNoControls()
    {
        var device = AddMouseMode(0);

        Assert.That(device, Is.Not.InstanceOf<Lightgun>());
        Assert.That(device.allControls, Is.Empty);
    }

    [Test]
    [Category("MouseMode")]
    public void GamepadCollection_StillCreatesLightgunWithPlayerIndex()
    {
        var device = AddGamepad(1);

        Assert.That(device, Is.InstanceOf<BlamconLightgunHID>());
        Assert.That(((BlamconLightgunHID)device).playerIndex, Is.EqualTo(1));
    }

    // --- same reports as the gamepad device ------------------------------------------------------

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_EnableFFBControl_SendsSameReport()
    {
        AssertSends(AddMouseMode(0), gun => gun.EnableFFBControl(),
            FeedbackCommandTests.Report((1, 1), (2, 3), (3, 1), (4, 3), (5, 1), (6, 3), (7, 1), (8, 2)));
    }

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_ActivateRecoil_SendsSameReport()
    {
        AssertSends(AddMouseMode(0), gun => gun.ActivateRecoil(2, 45, 60),
            FeedbackCommandTests.Report((5, 1), (29, 2), (30, 45), (31, 60)));
    }

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_ActivateRumble_SendsSameReport()
    {
        AssertSends(AddMouseMode(0), gun => gun.ActivateRumble(3, 500, 400),
            FeedbackCommandTests.Report((1, 1), (15, 3), (16, 0xF4), (17, 0x01), (18, 0x90), (19, 0x01)));
    }

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_ActivateLED_SendsSameReport()
    {
        AssertSends(AddMouseMode(0), gun => gun.ActivateLED(1, Color.red, 3),
            FeedbackCommandTests.Report((3, 1), (20, 255), (23, 1), (24, 3)));
    }

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_SendAmmoCount_SendsSameReport()
    {
        AssertSends(AddMouseMode(0), gun => gun.SendAmmoCount(42), FeedbackCommandTests.Report((7, 1), (32, 42)));
    }

    [Test]
    [Category("MouseMode")]
    public void MouseModeDevice_SendCommand_SendsCombinedReport()
    {
        var device = (BlamconMouseModeDevice)AddMouseMode(0);
        AssertSends(device, _ =>
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetRecoil(1);
            report.SetAmmo(7);
            return device.SendCommand(ref report);
        },
        FeedbackCommandTests.Report((5, 1), (29, 1), (7, 1), (32, 7)));
    }

    // --- routing ---------------------------------------------------------------------------------

    [Test]
    [Category("MouseMode")]
    public void SelectFeedbackDevice_PrefersGamepadDeviceOverMouseMode()
    {
        var mouseMode = AddMouseMode(0);
        var gamepad = AddGamepad(0);

        Assert.That(BlamconDevices.SelectFeedbackDevice(new[] { mouseMode, gamepad }, 0), Is.SameAs(gamepad));
        Assert.That(BlamconDevices.SelectFeedbackDevice(new[] { gamepad, mouseMode }, 0), Is.SameAs(gamepad));
    }

    [Test]
    [Category("MouseMode")]
    public void SelectFeedbackDevice_MostRecentlyAddedDuplicateWins()
    {
        var older = AddMouseMode(0);
        var newer = AddMouseMode(0);

        Assert.That(BlamconDevices.SelectFeedbackDevice(new[] { older, newer }, 0), Is.SameAs(newer));
        Assert.That(BlamconDevices.SelectFeedbackDevice(new[] { newer, older }, 0), Is.SameAs(newer));
    }

    [Test]
    [Category("MouseMode")]
    public void SelectFeedbackDevice_MatchesPlayerIndex()
    {
        var mouseModeP2 = AddMouseMode(1);
        var gamepadP3 = AddGamepad(2);
        var devices = new[] { mouseModeP2, gamepadP3 };

        Assert.That(BlamconDevices.SelectFeedbackDevice(devices, 1), Is.SameAs(mouseModeP2));
        Assert.That(BlamconDevices.SelectFeedbackDevice(devices, 2), Is.SameAs(gamepadP3));
        Assert.That(BlamconDevices.SelectFeedbackDevice(devices, 0), Is.Null);
        Assert.That(BlamconDevices.SelectFeedbackDevice(devices, 3), Is.Null);
    }

    [Test]
    [Category("MouseMode")]
    public void SelectFeedbackDevice_IgnoresDevicesWithoutAPlayerIndex()
    {
        var gun = InputSystem.AddDevice<BlamconLightgunHID>();
        m_Added.Add(gun);

        Assert.That(gun.playerIndex, Is.EqualTo(-1));
        Assert.That(BlamconDevices.SelectFeedbackDevice(new InputDevice[] { gun }, 0), Is.Null);
    }

    [Test]
    [Category("MouseMode")]
    public void GetForceFeedback_FindsAddedMouseModeDevice()
    {
        // Real guns may be connected, so only assert that the player has a feedback target.
        AddMouseMode(3);

        Assert.That(BlamconLightgunHID.GetForceFeedback(3), Is.Not.Null);
    }

    // --- player index ----------------------------------------------------------------------------

    [Test]
    [Category("MouseMode")]
    public void PlayerIndexFrom_MapsBlamconProductIds()
    {
        for (var player = 0; player < 4; player++)
            Assert.That(BlamconDevices.PlayerIndexFrom(HidDescription(kVendorDefined, 0x01, kFirstProductId + player)), Is.EqualTo(player));
    }

    [Test]
    [Category("MouseMode")]
    public void PlayerIndexFrom_RejectsOtherDevices()
    {
        Assert.That(BlamconDevices.PlayerIndexFrom(HidDescription(kVendorDefined, 0x01, kFirstProductId + 4)), Is.EqualTo(-1));
        Assert.That(BlamconDevices.PlayerIndexFrom(HidDescription(kVendorDefined, 0x01, kFirstProductId, vendorId: 0x1234)), Is.EqualTo(-1));
        Assert.That(BlamconDevices.PlayerIndexFrom(new InputDeviceDescription { interfaceName = "HID" }), Is.EqualTo(-1));
        Assert.That(BlamconDevices.PlayerIndexFrom(new InputDeviceDescription { interfaceName = "XInput", capabilities = "{}" }), Is.EqualTo(-1));
    }
}
