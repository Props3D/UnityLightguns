using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using Blamcon.Lightguns;

/// <summary>
/// The session lifecycle: which control reports go to which guns, and when.
/// </summary>
/// <remarks>
/// Tests drive <see cref="LightgunSessionController"/> directly, looking only at the devices each test
/// adds, so real guns connected during a run are never sent a command.
/// </remarks>
public class LightgunSessionTests
{
    readonly List<InputDevice> m_Added = new List<InputDevice>();

    // Update bytes: 1 rumble, 3 LED, 5 recoil, 7 ammo. Control bytes: 2, 4, 6, 8 (3 = take, 2 = release).
    static readonly byte[] kTakeRecoilRumbleLed = FeedbackCommandTests.Report((1, 1), (2, 3), (3, 1), (4, 3), (5, 1), (6, 3));
    static readonly byte[] kReleaseRecoilRumbleLed = FeedbackCommandTests.Report((1, 1), (2, 2), (3, 1), (4, 2), (5, 1), (6, 2));

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

    InputDevice AddMouseMode(int player) => Add(MouseModeFeedbackTests.HidDescription(
        MouseModeFeedbackTests.kVendorDefined, 0x01, MouseModeFeedbackTests.kFirstProductId + player));

    InputDevice AddGamepad(int player) => Add(MouseModeFeedbackTests.HidDescription(
        MouseModeFeedbackTests.kGenericDesktop, 0x05, MouseModeFeedbackTests.kFirstProductId + player));

    InputDevice Add(InputDeviceDescription description)
    {
        var device = InputSystem.AddDevice(description);
        m_Added.Add(device);
        return device;
    }

    LightgunSessionController Controller() => new LightgunSessionController(() => m_Added);

    static byte[] Payload(HidoCapture capture, int index) => capture.commands[index].payload;

    // --- begin and end -----------------------------------------------------------------------------

    [Test]
    [Category("Session")]
    public void Begin_TakesRecoilRumbleAndLedOnEveryPlayersGun_LeavingAmmoAlone()
    {
        var p1MouseMode = AddMouseMode(0);
        var p2Gamepad = AddGamepad(1);
        using (var p1 = new HidoCapture(p1MouseMode))
        using (var p2 = new HidoCapture(p2Gamepad))
        {
            Controller().Begin();

            Assert.That(p1.commands.Count, Is.EqualTo(1));
            Assert.That(Payload(p1, 0), Is.EqualTo(kTakeRecoilRumbleLed));
            Assert.That(p2.commands.Count, Is.EqualTo(1));
            Assert.That(Payload(p2, 0), Is.EqualTo(kTakeRecoilRumbleLed));
        }
    }

    [Test]
    [Category("Session")]
    public void Begin_WithAmmo_AlsoTakesAmmo()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.ammo = true;
            session.Begin();

            Assert.That(capture.commands.Count, Is.EqualTo(1));
            Assert.That(Payload(capture, 0),
                Is.EqualTo(FeedbackCommandTests.Report((1, 1), (2, 3), (3, 1), (4, 3), (5, 1), (6, 3), (7, 1), (8, 3))));
        }
    }

    [Test]
    [Category("Session")]
    public void Begin_TouchesOnlyTheChosenComponents()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.rumble = false;
            session.led = false;
            session.Begin();

            Assert.That(capture.commands.Count, Is.EqualTo(1));
            Assert.That(Payload(capture, 0), Is.EqualTo(FeedbackCommandTests.Report((5, 1), (6, 3))));
        }
    }

    [Test]
    [Category("Session")]
    public void Begin_WithNothingChosen_SendsNothing()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.recoil = false;
            session.rumble = false;
            session.led = false;
            session.Begin();

            Assert.That(capture.commands, Is.Empty);
        }
    }

    [Test]
    [Category("Session")]
    public void End_HandsControlBack()
    {
        var gun = AddMouseMode(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.Begin();
            session.End();

            Assert.That(capture.commands.Count, Is.EqualTo(2));
            Assert.That(Payload(capture, 1), Is.EqualTo(kReleaseRecoilRumbleLed));
            Assert.That(session.active, Is.False);
        }
    }

    [Test]
    [Category("Session")]
    public void End_WithoutBegin_SendsNothing()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            Controller().End();

            Assert.That(capture.commands, Is.Empty);
        }
    }

    [Test]
    [Category("Session")]
    public void Begin_Twice_TakesControlOnce()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.Begin();
            session.Begin();

            Assert.That(capture.commands.Count, Is.EqualTo(1));
        }
    }

    // --- pause and resume (focus) ---------------------------------------------------------------

    [Test]
    [Category("Session")]
    public void PauseAndResume_HandBackThenRetakeControl()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.Begin();
            session.Pause();
            session.Pause();
            session.Resume();
            session.Resume();

            Assert.That(capture.commands.Count, Is.EqualTo(3));
            Assert.That(Payload(capture, 0), Is.EqualTo(kTakeRecoilRumbleLed));
            Assert.That(Payload(capture, 1), Is.EqualTo(kReleaseRecoilRumbleLed));
            Assert.That(Payload(capture, 2), Is.EqualTo(kTakeRecoilRumbleLed));
        }
    }

    [Test]
    [Category("Session")]
    public void End_WhilePaused_DoesNotReleaseAgain()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.Begin();
            session.Pause();
            session.End();

            Assert.That(capture.commands.Count, Is.EqualTo(2));
        }
    }

    [Test]
    [Category("Session")]
    public void PauseAndResume_WithoutBegin_SendNothing()
    {
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            var session = Controller();
            session.Pause();
            session.Resume();

            Assert.That(capture.commands, Is.Empty);
        }
    }

    // --- guns connecting during a session ----------------------------------------------------------

    [Test]
    [Category("Session")]
    public void DeviceAdded_WhileActive_TakesControlOfThatGun()
    {
        var session = Controller();
        session.Begin();

        var gun = AddMouseMode(2);
        using (var capture = new HidoCapture(gun))
        {
            session.DeviceAdded(gun);

            Assert.That(capture.commands.Count, Is.EqualTo(1));
            Assert.That(Payload(capture, 0), Is.EqualTo(kTakeRecoilRumbleLed));
        }
    }

    [Test]
    [Category("Session")]
    public void DeviceAdded_WhilePausedOrInactive_SendsNothing()
    {
        var session = Controller();
        var gun = AddGamepad(0);
        using (var capture = new HidoCapture(gun))
        {
            session.DeviceAdded(gun);
            session.Begin();
            session.Pause();
            session.DeviceAdded(gun);

            // Begin sent one take and Pause one release; neither DeviceAdded sent anything.
            Assert.That(capture.commands.Count, Is.EqualTo(2));
        }
    }

    [Test]
    [Category("Session")]
    public void DeviceAdded_IgnoresGunsWithoutAPlayerIndex()
    {
        var session = Controller();
        session.Begin();

        var gun = InputSystem.AddDevice<BlamconLightgunHID>();
        m_Added.Add(gun);
        using (var capture = new HidoCapture(gun))
        {
            session.DeviceAdded(gun);

            Assert.That(capture.commands, Is.Empty);
        }
    }

    // --- stale devices -----------------------------------------------------------------------------

    [Test]
    [Category("Session")]
    public void Begin_SendsOnlyToTheMostRecentlyAddedDuplicate()
    {
        var older = AddMouseMode(0);
        var newer = AddMouseMode(0);
        using (var olderCapture = new HidoCapture(older))
        using (var newerCapture = new HidoCapture(newer))
        {
            Controller().Begin();

            Assert.That(olderCapture.commands, Is.Empty);
            Assert.That(newerCapture.commands.Count, Is.EqualTo(1));
        }
    }
}
