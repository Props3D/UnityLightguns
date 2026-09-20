using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Layouts;
using Blamcon.Lightguns;

/// <summary>
/// What <see cref="BlamconLightgunHID.GetInfo"/> reports about a player's gun, and the package version
/// constant.
/// </summary>
/// <remarks>
/// Devices are created from HID device descriptions, the way Unity's native backend creates them, and the
/// info is built from only the devices a test created, so a real gun plugged in during a test run can't
/// change the result. The firmware sets the description's version to its own version in BCD: 768 is 3.0.0
/// and 513 is 2.0.1.
/// </remarks>
public class LightgunInfoTests
{
    const int kVendorId = MouseModeFeedbackTests.kVendorId;
    const int kFirstProductId = MouseModeFeedbackTests.kFirstProductId;
    const HID.UsagePage kGenericDesktop = MouseModeFeedbackTests.kGenericDesktop;
    const HID.UsagePage kVendorDefined = MouseModeFeedbackTests.kVendorDefined;

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

    InputDevice AddGamepad(int player, string version = "768") =>
        Add(MouseModeFeedbackTests.HidDescription(kGenericDesktop, 0x05, kFirstProductId + player, kVendorId, version));

    InputDevice AddMouseMode(int player, string version = "768") =>
        Add(MouseModeFeedbackTests.HidDescription(kVendorDefined, 0x01, kFirstProductId + player, kVendorId, version));

    InputDevice Add(InputDeviceDescription description)
    {
        var device = InputSystem.AddDevice(description);
        m_Added.Add(device);
        return device;
    }

    BlamconLightgunInfo Info(int player) => BlamconDevices.BuildInfo(m_Added, player);

    [Test]
    [Category("Integration")]
    public void Info_GamepadReportsModeVersionAndBoard()
    {
        AddGamepad(1);

        var info = Info(1);

        Assert.That(info.connected, Is.True);
        Assert.That(info.playerIndex, Is.EqualTo(1));
        Assert.That(info.hasGunInput, Is.True);
        Assert.That(info.mode, Is.EqualTo(LightgunMode.Gamepad));
        Assert.That(info.detailsKnown, Is.True);
        Assert.That(info.firmwareVersion, Is.EqualTo("3.0.0"));
        Assert.That(info.firmwareVersionNumber, Is.EqualTo(30000));
        Assert.That(info.board, Is.EqualTo(LightgunBoard.RP2350));
        Assert.That(info.feedbackAvailable, Is.True);
        Assert.That(info.playerNumberOnGun, Is.EqualTo(2));
        Assert.That(info.productName, Is.EqualTo("Blamcon Lightgun (test)"));
    }

    [Test]
    [Category("Integration")]
    public void Info_OlderFirmwareIsAnRp2040()
    {
        AddGamepad(0, "513"); // 0x0201

        var info = Info(0);

        Assert.That(info.firmwareVersion, Is.EqualTo("2.0.1"));
        Assert.That(info.firmwareVersionNumber, Is.EqualTo(20001));
        Assert.That(info.board, Is.EqualTo(LightgunBoard.RP2040));
        Assert.That(info.feedbackAvailable, Is.True);
    }

    [Test]
    [Category("Integration")]
    public void Info_MouseModeHasNoGunInput()
    {
        AddMouseMode(0);

        var info = Info(0);

        Assert.That(info.connected, Is.True);
        Assert.That(info.hasGunInput, Is.False);
        Assert.That(info.mode, Is.EqualTo(LightgunMode.Mouse));
        Assert.That(info.feedbackAvailable, Is.True);
    }

    [Test]
    [Category("Integration")]
    public void Info_GamepadWinsWhenBothDevicesExist()
    {
        AddMouseMode(0);
        AddGamepad(0);

        var info = Info(0);

        Assert.That(info.mode, Is.EqualTo(LightgunMode.Gamepad));
        Assert.That(info.hasGunInput, Is.True);
    }

    [Test]
    [Category("Integration")]
    public void Info_UnreadableVersionLeavesDetailsUnknown()
    {
        AddGamepad(0, null);

        var info = Info(0);

        Assert.That(info.connected, Is.True);
        Assert.That(info.mode, Is.EqualTo(LightgunMode.Gamepad));
        Assert.That(info.detailsKnown, Is.False);
        Assert.That(info.firmwareVersion, Is.Empty);
        Assert.That(info.firmwareVersionNumber, Is.Zero);
        Assert.That(info.board, Is.EqualTo(LightgunBoard.Unknown));
        // The gun is reachable whatever it calls itself, so don't claim feedback is unavailable.
        Assert.That(info.feedbackAvailable, Is.True);
    }

    [Test]
    [Category("Integration")]
    public void Info_NonBcdVersionIsNotAVersion()
    {
        AddGamepad(0, "4010"); // 0x0FAA: digits above 9 are some other device's numbering.

        var info = Info(0);

        Assert.That(info.detailsKnown, Is.False);
        Assert.That(info.firmwareVersionNumber, Is.Zero);
    }

    [Test]
    [Category("Integration")]
    public void Info_NoGunForPlayer()
    {
        AddGamepad(0);

        var info = Info(3);

        Assert.That(info.connected, Is.False);
        Assert.That(info.playerIndex, Is.EqualTo(-1));
        Assert.That(info.mode, Is.EqualTo(LightgunMode.Unknown));
        Assert.That(info.firmwareVersion, Is.Empty);
    }

    [Test]
    [Category("Integration")]
    public void PackageVersion_MatchesPackageManifest()
    {
#if UNITY_EDITOR
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BlamconLightgunHID).Assembly);
        Assert.That(package, Is.Not.Null, "package not found; is the project using a local copy of the package?");
        Assert.That(BlamconLightguns.version, Is.EqualTo(package.version));
#else
        Assert.Ignore("package.json is only readable in the Editor.");
#endif
    }
}
