using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Blamcon.Lightguns;
using Blamcon.Lightguns.LowLevel;

/// <summary>
/// The exact HID output report every feedback call sends, as of 1.1.0. These pin the behaviour so the
/// 2.0 refactor can be checked byte for byte; change an expectation only when the behaviour is meant to
/// change. Offsets follow the firmware's 0x10 report (handleDefaultOutputReport in the firmware's main.cpp).
/// </summary>
public class FeedbackCommandTests
{
    const int kReportSize = 40;

    [SetUp]
    public void Setup()
    {
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
    }

    [TearDown]
    public void TearDown()
    {
        InputSystem.settings.backgroundBehavior = default;
    }

    /// <summary>A 0x10 report: report ID set, the given bytes set, everything else zero.</summary>
    internal static byte[] Report(params (int offset, int value)[] fields)
    {
        var report = new byte[kReportSize];
        report[0] = 0x10;
        foreach (var (offset, value) in fields)
            report[offset] = (byte)value;
        return report;
    }

    static void AssertSends(Func<BlamconLightgunHID, bool> send, byte[] expected)
    {
        var gun = InputSystem.AddDevice<BlamconLightgunHID>();
        try
        {
            using (var capture = new HidoCapture(gun))
            {
                Assert.That(send(gun), Is.True, "feedback call reported failure");
                Assert.That(capture.commands.Count, Is.EqualTo(1), "expected exactly one HIDO command");
                Assert.That(capture.commands[0].sizeInBytes, Is.EqualTo(8 + kReportSize), "command size");
                Assert.That(capture.commands[0].payload, Is.EqualTo(expected));
            }
        }
        finally
        {
            InputSystem.RemoveDevice(gun);
        }
    }

    // --- control ---------------------------------------------------------------------------------
    // Update bytes: 1 rumble, 3 LED, 5 recoil, 7 ammo. Control bytes: 2 rumble, 4 LED, 6 recoil, 8 ammo
    // (3 = take control, 2 = release).

    [Test]
    [Category("Feedback")]
    public void EnableFFBControl_Defaults_TakesRecoilRumbleLedAndReleasesAmmo()
    {
        AssertSends(gun => gun.EnableFFBControl(),
            Report((1, 1), (2, 3), (3, 1), (4, 3), (5, 1), (6, 3), (7, 1), (8, 2)));
    }

    [Test]
    [Category("Feedback")]
    public void EnableFFBControl_AllFalse_ReleasesEverything()
    {
        AssertSends(gun => gun.EnableFFBControl(false, false, false, false),
            Report((1, 1), (2, 2), (3, 1), (4, 2), (5, 1), (6, 2), (7, 1), (8, 2)));
    }

    [Test]
    [Category("Feedback")]
    public void EnableAmmoFFBControl_True_TakesAmmoOnly()
    {
        AssertSends(gun => gun.EnableAmmoFFBControl(true), Report((7, 1), (8, 3)));
    }

    [Test]
    [Category("Feedback")]
    public void EnableAmmoFFBControl_False_ReleasesAmmoOnly()
    {
        AssertSends(gun => gun.EnableAmmoFFBControl(false), Report((7, 1), (8, 2)));
    }

    // --- rumble: count at 15, on period at 16-17, off period at 18-19 (little-endian) ---------------

    [Test]
    [Category("Feedback")]
    public void ActivateRumble_Default_OnePulse()
    {
        AssertSends(gun => gun.ActivateRumble(), Report((1, 1), (15, 1)));
    }

    [Test]
    [Category("Feedback")]
    public void ActivateRumble_Timed()
    {
        // 500 = 0x01F4, 400 = 0x0190
        AssertSends(gun => gun.ActivateRumble(3, 500, 400),
            Report((1, 1), (15, 3), (16, 0xF4), (17, 0x01), (18, 0x90), (19, 0x01)));
    }

    [Test]
    [Category("Feedback")]
    public void ActivateRumble_ClampsPulsesAndPeriods()
    {
        // pulses 50 -> 10, on 50 -> 100 (0x0064), off 9000 -> 2400 (0x0960)
        AssertSends(gun => gun.ActivateRumble(50, 50, 9000),
            Report((1, 1), (15, 10), (16, 0x64), (18, 0x60), (19, 0x09)));
    }

    // --- recoil: count at 29, on period at 30, off period at 31 ----------------------------------------

    [Test]
    [Category("Feedback")]
    public void ActivateRecoil_Default_OnePulse()
    {
        AssertSends(gun => gun.ActivateRecoil(), Report((5, 1), (29, 1)));
    }

    [Test]
    [Category("Feedback")]
    public void ActivateRecoil_Timed()
    {
        AssertSends(gun => gun.ActivateRecoil(2, 45, 60), Report((5, 1), (29, 2), (30, 45), (31, 60)));
    }

    [Test]
    [Category("Feedback")]
    public void ActivateRecoil_ClampsPulsesAndPeriods()
    {
        // pulses 99 -> 10, on 5 -> 15, off 5 -> 45
        AssertSends(gun => gun.ActivateRecoil(99, 5, 5), Report((5, 1), (29, 10), (30, 15), (31, 45)));
    }

    // --- LED: RGB at 20-22, index at 23, flash count at 24 ------------------------------------------

    [Test]
    [Category("Feedback")]
    public void ActivateLED_SolidColour()
    {
        AssertSends(gun => gun.ActivateLED(1, Color.red), Report((3, 1), (20, 255), (23, 1)));
    }

    [Test]
    [Category("Feedback")]
    public void ActivateLED_Flash_TruncatesColourChannels()
    {
        // 0.5 * 255 = 127.5 -> 127, 0.25 * 255 = 63.75 -> 63
        AssertSends(gun => gun.ActivateLED(0, new Color(0.5f, 0.25f, 1f), 3),
            Report((3, 1), (20, 127), (21, 63), (22, 255), (24, 3)));
    }

    [Test]
    [Category("Feedback")]
    public void ActivateLED_Flash_ClampsFlashCount()
    {
        AssertSends(gun => gun.ActivateLED(1, Color.white, 500),
            Report((3, 1), (20, 255), (21, 255), (22, 255), (23, 1), (24, 100)));
    }

    // --- ammo: remaining at 32, used at 33, max at 34 ------------------------------------------------

    [Test]
    [Category("Feedback")]
    public void SendAmmoCount_SetsRemaining()
    {
        AssertSends(gun => gun.SendAmmoCount(42), Report((7, 1), (32, 42)));
    }

    [Test]
    [Category("Feedback")]
    public void SendAmmoCount_ClampsAbove255()
    {
        AssertSends(gun => gun.SendAmmoCount(300), Report((7, 1), (32, 255)));
    }

    [Test]
    [Category("Feedback")]
    public void SendAmmoCount_ClampsBelowZero()
    {
        AssertSends(gun => gun.SendAmmoCount(-5), Report((7, 1)));
    }

    // --- combined report --------------------------------------------------------------------------

    [Test]
    [Category("Feedback")]
    public void SendCommand_CombinedReport_SendsBothEffectsInOneCommand()
    {
        AssertSends(gun =>
        {
            var report = BlamconHIDOutputReport.Create();
            report.SetRecoil(1);
            report.SetAmmo(7);
            return gun.SendCommand(ref report);
        },
        Report((5, 1), (29, 1), (7, 1), (32, 7)));
    }
}
