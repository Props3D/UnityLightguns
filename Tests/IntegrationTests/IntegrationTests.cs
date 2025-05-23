using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
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
            InputSystem.QueueStateEvent(lightgun, new BlamconLightgunState().WithButton(LightgunButton.BUTTON_WEST));
            InputSystem.Update();

            Assert.That(lightgun.buttonWest.isPressed, Is.True);
        }
        finally
        {
            InputSystem.RemoveDevice(lightgun);
        }
    }
}
