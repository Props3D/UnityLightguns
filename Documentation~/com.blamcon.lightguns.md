---
uid: input-system-lightgun
---
# Lightgun Support

- [Controls](#controls)
  - [Remapping Absolute Positions](#remapping-absolute-positions)
- [Blamcon Lightguns](#blamcon-lightguns)
  - [Forced Feedback Commands](#forced-feedback-commands)

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

Modern lightguns are driven by using an IR Camera and IR emitters to determine the absolute coordinates of the cursor position. The input range of absolute positional coordinates vary between different lightguns. Blamcon lightguns advertise a range of values between [0 - 32768].

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
        [InputControl(processors = "AbsolutePositionRemap"]
        public Vector2 position;
    }
```

## Blamcon Lightguns

Blamcon lightguns are well supported on different Devices. This plugin implements these as derived types of `Lightgun`.

* [`BlamconLightgunHID`]: A Blamcon Lightgun connected to a desktop computer using the HID interface and advertised as a Gamepad device. 

### Forced Feedback Commands
Blamcon Lightguns provide the ability to control different feedback components, when available. Typically, a lightgun may have a solenoid, vibration motor, or an addressable LED.


> NOTE: Due to limitations in the USB driver and/or the hardware, only one IOCTL (input/output control) command can be serviced at a time. Feedback functionality is implemented using IOCTL commands, and so if different methods are called in quick succession, it is likely that only the first command will successfully complete. The other commands will be dropped.

If there is a need to activate recoil, rumble, or LED at the same time, use the [`BlamconHIDOutputReport`] struct and set the state for each component then use `BlamconLightgunHID.SendCommand(ref cmd)`. Alternatively, setup coroutines to send multiple commands with enough of a delay between each one. See the samples for examples to follow. 


```CSharp
BlamconLightgunHID device = InputSystem.GetDevice<BlamconLightgunHID>();
if (device != null)
{
    // recoil once
    device.ActivateRecoil(1);
}
```