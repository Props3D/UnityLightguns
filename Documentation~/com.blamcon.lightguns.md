---
uid: input-system-lightgun
---
# Lightgun Support

- [Controls](#controls)
  - [Remapping Absolute Positions](#remapping-absolute-positions)
- [Blamcon Lightguns](#blamcon-lightguns)
  - [Forced Feedback Commands](#forced-feedback-commands)
    - [Firmware compatibility](#firmware-compatibility)
    - [Lightgun Session](#lightgun-session)
    - [Reading a gun's state](#reading-a-guns-state)
- [Upgrading from 1.x](#upgrading-from-1x)

Physically, Lightguns represent input devices attached to the computer through USB, which a user can use to control the app. All lightguns are built on the HID interface, and have the ability to connect to the computer as either:
 - Mouse with Keyboard (Composite Device)
 - Gamepad
 - Joystick

This package is to provide better support for lightguns when they are configured as Gamepad or Joystick devices, so that it's treated as a [`Pointer`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Pointer.html) device such as a Mouse.

A [`Lightgun`]() can be defined as an [`InputDevices`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.InputDevice.html) that tracks a position on a 2D surface. All lightguns have a common set of button controls for trigger, A, and B buttons. Additionally, some lightguns may also have extra buttons, or a d-pad with start and select buttons. A lightgun can have additional Controls, such as a solenoid, vibration motor, or LED(s) which the Device can expose. However, all lightguns are guaranteed to have at least the minimum set of Controls described above.

Lightgun support is intended to be similar to gamepad support, providing the correct location and functioning of Controls across platforms and hardware. For example, a Blamcon lightgun layout should look identical regardless of which platform it is supported on.

> NOTE: Similar to Gamepads, generic [HID](./HID.md) lightguns will __not__ be surfaced as [`Lightgun`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Lightgun.html) devices but rather be created as generic [joysticks](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Joystick.html). This is because the Input System cannot guarantee correct mapping of buttons and axes on the controller (the information is simply not available at the HID level). Only HID lightguns that are explicitly supported by the Input System (like the Blamcon lightgun) will come out as lightguns. Note that you can set up the same kind of support for specific HID lightguns yourself (see ["Overriding the HID Fallback"](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/HID.html#creating-a-custom-device-layout)).

## Controls

Most modern lightguns have a fixed set of physical buttons. However, these lightguns usually have the ability to remap or re-assign the physical buttons to different controls. This is typically done using a proprietary application that is specific to each lightgun. For example, you can use [Blamcon ARC (Advanced Remote Console)](https://blamcon.com/manual/blamcon-arc) to remap all button assignments.

These are the most common set of Controls used by Lightguns when set to Gamepad mode:

|Control|Type|Description|
|-------|----|-----------|
|[`position`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Pointer.html#UnityEngine_InputSystem_Pointer_position)|[`Vector2Control`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.Vector2Control.html)|The current pointer coordinates in window space.|
|[`buttonWest`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_buttonWest)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|This is generally designated as the primary trigger for the lightgun. Compared to Gamepads, it's usually labelled "X" on Xbox controllers and "Square" on PlayStation controllers.|
|[`buttonSouth`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_buttonSouth)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|This is usually the defined as the secondary action for the lightgun. Comparing to Gamepads, it's usually labelled "A" on Xbox controllers and "Cross" on PlayStation controllers.|
|[`buttonEast`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_buttonEast)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|This is usually the defined as the tertiary action for the lightgun. Comparing to Gamepads, it's usually labelled "B" on Xbox controllers and "Circle" on PlayStation controllers.|
|[`buttonNorth`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_buttonNorth)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|This button is generally not mapped to any physical button. Comparing to Gamepads, it's usually labelled "Y" on Xbox controllers and "Triangle" on PlayStation controllers.|
|[`dpad`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_dpad)|[`DpadControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.DpadControl.html)|The optional D-pad on the lightgun.|
|[`leftShoulder`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_leftShoulder)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|The left shoulder button. This button is generally not mapped to any physical button.|
|[`rightShoulder`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_rightShoulder)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|The right shoulder button. This button is generally not mapped to any physical button.|
|[`leftTrigger`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_leftTrigger)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|The left trigger button. This button is generally not mapped to any physical button.|
|[`rightTrigger`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_rightTrigger)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|The right trigger button. This button is generally not mapped to any physical button.|
|[`startButton`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_startButton)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|The start button. This is mapped to the start button on the optional joystick d-pad.|
|[`selectButton`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_selectButton)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|The select button.  This is mapped to the select button on the optional joystick d-pad.|
|[`leftStickButton`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_leftStickButton)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|This button is generally not mapped to any physical button.|
|[`rightStickButton`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Gamepad.html#UnityEngine_InputSystem_Gamepad_rightStickButton)|[`ButtonControl`](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.Controls.ButtonControl.html)|This button is generally not mapped to any physical button.|


### Remapping Absolute Positions

Modern lightguns are driven by using an IR Camera and IR emitters to determine the absolute coordinates of the cursor position. The input range of absolute positional coordinates vary between different lightguns. Blamcon lightguns advertise a range of values between [0 - 32767].

In order to translate these inputs, a generic input processor is used that remaps the Vector2 value from a configurable input range into screen pixel coordinates or normalized [0–1] viewport coordinates. This is commonly used for absolute-position input devices like lightguns or tablets where incoming values range from fixed bounds (e.g., 0–32767, 0–65535, or even signed ranges).

To remap absolute coordinates for lightgun position, specify an [Absolute Position Remap Processor] on the position, like this:

```JSON
     {
        "name" : "MyLightgun",
        "extend" : "Lightgun",
        "controls" : [
            {
                "name" : "position",
                "processors" : "AbsolutePositionRemap"
            }
        ]
    }
```

OR You can do the same in your C# state structs.

```CSharp
    public struct MyDeviceState
    {
        [InputControl(processors = "AbsolutePositionRemap")]
        public Vector2 position;
    }
```

## Blamcon Lightguns

Blamcon lightguns are well supported on different Devices. This plugin implements these as derived types of `Lightgun`.

* [`BlamconLightgunHID`]: A Blamcon Lightgun connected to a desktop computer using the HID interface and advertised as a Gamepad device. 

A Blamcon lightgun in mouse mode aims and fires through Unity's normal `Mouse`, so it isn't a `Lightgun` device. Firmware with mouse-mode feedback adds a vendor-defined HID collection that the package uses for force feedback only; you reach it through `BlamconLightgunHID.GetForceFeedback`, the same as a gun in Gamepad mode.

### Forced Feedback Commands
Blamcon Lightguns provide the ability to control different feedback components, when available. Typically, a lightgun may have a solenoid, vibration motor, or an addressable LED.


> NOTE: Due to limitations in the USB driver and/or the hardware, only one IOCTL (input/output control) command can be serviced at a time. Feedback functionality is implemented using IOCTL commands, and so if different methods are called in quick succession, it is likely that only the first command will successfully complete. The other commands will be dropped.

Get a player's gun with `BlamconLightgunHID.GetForceFeedback(player)`, where `player` is 0 for player 1. It returns the gun's `IForceFeedback` whether the gun is in Gamepad mode or mouse mode, or `null` if that player has no gun that can take feedback. If a player has a gun listed twice, for example after a firmware update without restarting the Editor, the most recently added device is used.

If there is a need to activate recoil, rumble, or LED at the same time, use the [`BlamconHIDOutputReport`] struct, set the state for each component, then send it with `BlamconLightgunHID.SendCommand(player, ref report)`. Alternatively, setup coroutines to send multiple commands with enough of a delay between each one. See the samples for examples to follow. 

#### Firmware compatibility

Some features need newer firmware. Update the gun's firmware with [Blamcon ARC](https://blamcon.com/manual/blamcon-arc).

| Feature | Firmware |
|---|---|
| Aim and buttons in Gamepad mode | 1.0.16 or later |
| Force feedback in Gamepad mode over USB | 1.0.16 or later |
| Force feedback in mouse mode over USB | 2.1.0 or later |
| Rumble and LED periods that are exact multiples of 256 ms | 2.1.0 or later |
| Force feedback in Gamepad mode over Bluetooth Classic | 4.0.0 or later |
| Force feedback in mouse mode over Bluetooth Classic | 4.0.0 or later |

#### Firmware behaviour notes

* Force feedback is only processed while the lightgun is in play mode.
* A gun in mouse mode on firmware older than 2.1.0 is invisible to Unity, so `GetForceFeedback` returns `null`. Update the firmware, or switch the gun to Gamepad mode in Blamcon ARC.
* `SendAmmoCount` only takes effect after ammo control has been enabled, e.g. `EnableFFBControl(ammo: true)` or `EnableAmmoFFBControl(true)`. Enabling ammo control resets the display, so send the starting count in the same report (`command.SetAmmo(n)`).
* The LED `index` parameter is currently ignored by the firmware.
* `BlamconHIDOutputReport` (report `0x10`) is the only output command. It carries every component in one 40-byte report, which is the size Unity sends and the size the firmware expects. Package 2.0 removed the single-component commands (reports `0x20`–`0x23`): Unity pads every HID output command to the device's largest output report, and the firmware accepts those reports only at their exact size, so they never reached the gun.
* Timing limits (enforced by the firmware and clamped by this package): rumble 100-2400 ms on and off, LED flash 20-5000 ms on and off, recoil 15-200 ms on and 45-200 ms off. If you send no timings, the device defaults are used.
* Firmware older than 2.1.0 reads the rumble and LED on/off periods only when the low byte is non-zero, so it silently drops periods that are exact multiples of 256 ms (256, 512, 768, 1024, 1280, 1536, 1792). Avoid those values if you need to support older firmware.


```CSharp
IForceFeedback gun = BlamconLightgunHID.GetForceFeedback(0); // player 1, Gamepad or mouse mode
if (gun != null)
{
    // recoil once
    gun.ActivateRecoil(1);
}

// Several effects in one report
var report = BlamconHIDOutputReport.Create();
report.SetRecoil(1);
report.SetAmmo(ammoLeft);
BlamconLightgunHID.SendCommand(0, ref report);
```

#### Lightgun Session

Until a game takes control, a Blamcon lightgun drives its own feedback: recoil fires on every trigger pull, and the LED and ammo display follow the gun's settings. Add a **Lightgun Session** component to a scene (**Add Component → Blamcon → Lightgun Session**) to take control for as long as the component is enabled.

* It takes control of recoil, rumble and the LED on every connected gun, in Gamepad or mouse mode, and of guns that connect later.
* It hands control back when the component is disabled or destroyed, when play mode stops, and when the application quits, so a gun is never left under game control.
* With **Release On Focus Loss** ticked (the default), it also hands control back while the application doesn't have focus, and takes it again when focus returns.
* Only ticked components are touched. Taking or handing back control turns the LED off and stops rumble.
* **Ammo** is off by default, because taking ammo control zeroes the display. To manage the display yourself, take ammo control and send the starting count in one report:

```CSharp
var report = BlamconHIDOutputReport.Create();
report.EnableAmmoFFBControl(true);
report.SetAmmo(99);
BlamconLightgunHID.SendCommand(0, ref report);
```

Keep one enabled session at a time; two send every command twice. To find which player a Gamepad-mode gun belongs to, use `BlamconLightgunHID.playerIndex` (0 is player 1). A gun in mouse mode arrives as Unity's `Mouse`, which can't identify the gun.

#### Reading a gun's state

`BlamconLightgunHID.GetInfo(player)` describes one player's gun, for a settings screen or to check what a gun can do before offering it:

```CSharp
var info = BlamconLightgunHID.GetInfo(0);
if (info.connected && info.feedbackAvailable)
    Debug.Log($"Player 1: {info.productName}, firmware {info.firmwareVersion}, {info.mode} mode");
```

| Field | Meaning |
|---|---|
| `connected` | A gun has this player index. Every other field is default when false |
| `playerIndex` | 0-based, as passed to the other calls. -1 when no gun is connected |
| `hasGunInput` | Sends aim and buttons. False in mouse mode, where aim comes from Unity's `Mouse` |
| `feedbackAvailable` | The firmware takes force feedback in this mode |
| `detailsKnown` | The firmware version could be read. False leaves the three fields below empty |
| `firmwareVersion` | `"3.0.0"`, or empty when it couldn't be read |
| `firmwareVersionNumber` | 30000 for 3.0.0, so versions compare with `>=` |
| `board` | `RP2040` below firmware 3.0.0, `RP2350` from 3.0.0, the first firmware on that board |
| `mode` | `Gamepad` or `Mouse` |
| `playerNumberOnGun` | 1-4, set on the gun itself. `playerIndex` is this minus one |
| `productName` | The gun's USB product name |

The struct holds device facts only. Game preferences, such as the player's chosen LED colour or whether they want rumble, belong in your own settings.

`feedbackAvailable` says the firmware supports feedback in this mode, not that a solenoid or motor is fitted; Unity can't read the gun's hardware inventory. The firmware version comes from the USB `bcdDevice`, so a gun that doesn't report one reads as `detailsKnown` false and still works.

`BlamconLightguns.version` is the package's own version, for bug reports or an about screen.

## Upgrading from 1.x

Version 2.0 keeps `IForceFeedback` and `BlamconHIDOutputReport`, so most feedback code carries over. Work through the changes below.

### The single-component commands are removed

`BlamconRecoilCommand`, `BlamconRumbleCommand`, `BlamconLEDCommand` and `BlamconAmmoCommand` are gone, along with their `SendCommand` overloads. They never reached the gun in 1.x: Unity pads every HID output command to 40 bytes, and the firmware accepted those reports only at their exact size. Build the same effect with `BlamconHIDOutputReport`:

| 1.x | 2.0 |
|---|---|
| `BlamconRecoilCommand.Create(pulses[, on, off])` | `var report = BlamconHIDOutputReport.Create();` then `report.SetRecoil(pulses[, on, off])` |
| `BlamconRumbleCommand.Create(pulses[, on, off])` | `report.SetRumble(pulses[, on, off])` |
| `BlamconLEDCommand.Create(index, color[, flashes[, on, off]])` | `report.SetColor(index, color[, flashes[, on, off]])` |
| `BlamconAmmoCommand.Create(remaining)` | `report.SetAmmo(remaining)` |
| `device.SendCommand(ref command)` | `device.SendCommand(ref report)` |

> NOTE: Because the removed commands never reached the gun, effects you migrate will start happening for the first time: LED colours, rumble and ammo counts that were silent in 1.x.

### Find guns by player instead of `GetDevice`

1.x code usually found a gun with `InputSystem.GetDevice<BlamconLightgunHID>()`. That still compiles, but it returns only the first gun, only in Gamepad mode, and can return a device left over from before a firmware update, which fails every command. Look guns up by player instead; it works in Gamepad and mouse mode:

```CSharp
// 1.x
BlamconLightgunHID device = InputSystem.GetDevice<BlamconLightgunHID>();
if (device != null)
    device.SendCommand(ref report);

// 2.0
if (BlamconLightgunHID.GetForceFeedback(player) != null)
    BlamconLightgunHID.SendCommand(player, ref report);
```

To send feedback to the gun that fired, take the player from the device that triggered the action. A shot from a mouse, including a gun in mouse mode, can't identify the gun, so fall back to a player you choose:

```CSharp
int PlayerFor(InputDevice device, int mousePlayer = 0) =>
    device is BlamconLightgunHID gun && gun.playerIndex >= 0 ? gun.playerIndex : mousePlayer;
```

Drop checks like `if (device is BlamconLightgunHID)` before sending feedback. In mouse mode shots arrive from `Mouse`, so those checks silently skip feedback.

### Feedback control: keep yours, or use Lightgun Session

If your game takes and releases control itself with `EnableFFBControl`, it keeps working. You can replace it with a [Lightgun Session](#lightgun-session) component, which also releases control while the application lacks focus and takes control of guns that connect later. Don't use both: they send duplicate control commands, and the session's focus-loss release hands control back while your code thinks it still holds it.

### Samples

Re-import the **Lightgun Recoil Command** sample to get the 2.0 version. It sends feedback to the gun that fired, relies on a Lightgun Session for recoil control, and no longer has an `EnableForcedFeedbackControl` method.
