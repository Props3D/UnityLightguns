# Blamcon Lightguns for Unity — package specification

Draft 2, 2026-09-14. Package `com.blamcon.lightguns` 1.1.0. Target: Unity 6000.0+, Input System 1.14,
Windows. Companion to the Unreal plugin (`UnrealLightguns/docs/PLUGIN_SPEC.md`).

Unlike the Unreal plugin, this package is already released. This spec describes what is built, what
is known to be wrong or missing, and the roadmap. Public API changes follow semver: additive in minor
versions, deprecate before removing.

---

## 1. What this package is for

Give a Unity developer a lightgun that aims and shoots through the Input System like any other device,
and force feedback (recoil, rumble, RGB, ammo display) behind a few calls, without learning HID.

Secondary goals, shared with the Unreal plugin: feedback for guns in **mouse mode** as well as gamepad
mode, **through the same interface**, and games that can be built and tested on a mouse.

## 2. Constraints that drive the design (verified)

| # | Constraint | Consequence |
|---|---|---|
| 1 | Windows opens mouse/keyboard HID collections **exclusively**; game controllers and vendor-defined collections are **shared**. | Feedback reaches a gun in gamepad mode through its gamepad device, and in mouse mode only through the **vendor-defined collection** (usage page `0xFF00`, usage `0x01`) that current `release-3.0` firmware adds. Tested in Unity on Windows over USB and Bluetooth (2026-09-14). |
| 2 | **Unity doesn't see a gun in mouse mode on older firmware** (tested, 2026-09-14). Its mouse and keyboard collections go to the platform `Mouse` and `Keyboard`, and it has no vendor collection. | The package can't detect or warn about it. The firmware requirement is documented instead (§4.3). |
| 3 | The Input System supports HID directly on **Windows, macOS and UWP only**. On Linux, gamepads arrive through SDL with interface `"Linux"`, not `"HID"` (Input System `HID.md`, `LinuxSupport.cs`). | No layout here can match on Linux, and nothing can service `HIDO` there. Linux is out of scope. |
| 4 | `HIDO` goes through `InputDevice.ExecuteCommand` → `NativeInputSystem.IOCTL`, a closed native call. The payload is the raw report including its ID. Unity's own DualShock/DualSense code sizes every `HIDO` command to `hidDescriptor.outputReportSize`, the device's **largest** output report. | Commands are 40 bytes. The firmware accepts the single-component reports (`0x20`–`0x23`) only at their exact size, so **report `0x10` is the only reliable command**. |
| 5 | The managed Input System has **no feature-report command**. It defines `HIDO` (output) and `HIDD`/`HIDS`/`HIDP` (descriptor queries) only (verified in 1.14.0 source). | Device info (`0x50`) and live state (`0x51`) can't be read from C# (§4.2). |
| 6 | Firmware services **one output report at a time**, and ignores a new recoil while pulses are cycling. | Combine effects into one `0x10` report (`BlamconHIDOutputReport`, already public), and pace recoil like a fire rate. |
| 7 | Desktop Unity has **one `Mouse` device** for all mice ("We do not yet support distinguishing input from multiple pointers", `KnownLimitations.md`). | Guns in mouse mode share one cursor: aim is effectively single-player. Feedback can still target each gun, because each gun's vendor collection is a separate device with its own PID. |
| 8 | After a firmware reflash, Unity kept devices from the gun's previous firmware listed until the Editor restarted; `HIDO` to those returns `-1` (seen on hardware). | Feedback resolves to the **most recently added** matching device, never the first in the list. |
| 9 | Raw reports arrive as state events in format `'HID '`. The package decodes them in place into `BlamconLightgunState`, which **also** uses `'HID '`. | A decoded state queued by code (tests, replays) is decoded a second time. Tests currently work around this by queueing raw firmware reports. |

## 3. Architecture

### As built (1.1.0)

```
Runtime/Lightguns/
├─ Lightgun.cs                      base InputDevice + generic 22-byte state layout; Lightgun.current
├─ Processors/
│   └─ AbsolutePositionRemapProcessor   0..32767 → screen pixels or 0..1, invertY
├─ Feedback/
│   └─ IForceFeedback               EnableFFBControl, ActivateRecoil/Rumble/LED, SendAmmoCount
└─ Devices/Blamcon/
    ├─ BlamconLightgunHID.cs        4 layouts (P1–P4), raw report decode, IForceFeedback, SendCommand
    └─ commands/
        ├─ BlamconHIDOutputReport   0x10, 40 bytes — the recommended command
        └─ BlamconRecoil/Rumble/LED/AmmoCommand   0x20–0x23 — unreliable (constraint 4)
Samples~/  LightgunCrosshair, LightgunRecoilCommand
Tests/IntegrationTests/  6 tests: raw decode, unknown report IDs, cardinal d-pad, position, 0x10 layout, rumble
```

### Planned additions

```
Runtime/Lightguns/
├─ Devices/Blamcon/
│   ├─ BlamconLightgunHID.cs        + GetForceFeedback(player): gamepad device or mouse-mode collection
│   ├─ BlamconMouseModeDevice.cs    internal: the mouse-mode vendor collection; implements IForceFeedback
│   └─ BlamconForceFeedback.cs      internal: the IForceFeedback implementation both devices share
└─ Feedback/
    └─ LightgunSession.cs           take control on play, release on quit / focus loss / play-mode exit
```

**`IForceFeedback` stays the one feedback interface.** Mouse mode doesn't get its own API: a gun in
either mode is reached through `IForceFeedback`, and the same calls send the same `0x10` report. The
mode is an implementation detail of which Unity device the report goes to.

**Let the Input System own what it already owns:** device creation and removal, per-device identity,
action bindings and control schemes, and command interception through `InputSystem.onDeviceCommand`.
The package adds the HID layouts, the report decode, the commands, player lookup and session lifecycle.

**No standard Unity rumble API.** The gun's recoil, pulsed rumble, RGB and ammo display don't map onto
`IDualMotorRumble`'s two motor speeds, so the package doesn't implement it.

## 4. Device discovery and modes

### As built

Four layouts, one per player, each matching interface `HID`, VID `0x3673`, usage page `0x01`,
usage `0x05` (gamepad), PID `0x0100`–`0x0103`. The player index is `PID - 0x0100`. A gun in gamepad
mode becomes a `BlamconLightgunHID`; in mouse mode it becomes nothing the package knows about.

### 4.1 Mouse mode: the vendor collection (roadmap milestone 1)

Current `release-3.0` firmware adds a third top-level collection to the mouse-mode descriptor, next to
the mouse and keyboard, over USB and Bluetooth Classic.

**Tested in Unity on Windows (2026-09-14, RP2350 gun on `release-3.0`, USB mouse mode then Bluetooth
mouse mode after re-pairing):**
* Unity's native backend reports the vendor collection as **its own HID device**: usage page 65280,
  usage 1, `outputReportSize` 40, `featureReportSize` 13, `inputReportSize` 0, device version 768 (3.0.0).
* A layout matching usage page `0xFF00`, usage `0x01` and VID `0x3673` picked it up. A layout with its
  own matcher bypasses `HIDSupport.supportedHIDUsages`.
* The existing `BlamconHIDOutputReport` sent as `HIDO` returned `1` for every command. Recoil and LED
  (take control, fire or flash, release) worked on the gun.
* Aim and trigger kept arriving through Unity's normal `Mouse`.
* Not tested: rumble and ammo through `0x10`.
* Unity's parsed descriptor elements list every byte of `0x10`/`0x50`/`0x51` with `reportOffsetInBits`
  8. Harmless, because the package sends raw reports, but **don't generate a layout from those
  elements.**

**Design:**
* **`BlamconMouseModeDevice`** (internal) — registered with one matcher per PID (VID `0x3673`, usage
  page `0xFF00`, usage `0x01`). It implements `IForceFeedback` and
  `SendCommand(ref BlamconHIDOutputReport)` through the same code as `BlamconLightgunHID`.
* **It is not a `Lightgun`.** The collection has no input reports. As a `Lightgun` it would expose a
  position and buttons that never change, and `<Lightgun>` bindings and `PlayerInput` control schemes
  could pick it up. Keeping it off the `Lightgun` type keeps mouse-mode guns out of lightgun input
  entirely; aim and fire come from `Mouse`.
* **`BlamconLightgunHID.GetForceFeedback(int player)`** returns the `IForceFeedback` for a player: the
  gamepad device if there is one, else the mouse-mode device, and the most recently added among
  duplicates (constraint 8). Returns `null` if the player has neither.
* Existing code that calls `IForceFeedback` on a `BlamconLightgunHID` keeps working unchanged in gamepad
  mode. Covering mouse mode as well is a one-line change to the lookup.

### 4.2 Device info

The firmware answers feature reports `0x50` (device info: version, board, mode, feedback available,
player) and `0x51` (live host-control bits), specified in the Unreal spec §4.1. **Unity can't read
them** (constraint 5).

Decision for this package: **don't read them in C#.** Infer what matters from which device exists,
and log the rest:
* Feedback availability — `GetForceFeedback(player)` returns a device.
* Firmware build — `InputDeviceDescription.version` carries the USB `bcdDevice` (768 = 3.0.0 seen on
  hardware). Log only. Over Bluetooth on Windows it is `0`, as with hidapi.
* Leftover host control from a killed session — can't be detected; mitigated by releasing control on
  every session start before taking it (§6).

A native plugin that reads `0x50`/`0x51` is a deferred milestone (§9).

### 4.3 Mouse mode on older firmware

Unity doesn't see the gun at all (constraint 2), so there is nothing to detect. The package documents
the requirement in the README and the `GetForceFeedback` docs: "a gun in mouse mode needs firmware with
the vendor-defined collection for force feedback; otherwise use Gamepad mode". A `null` from
`GetForceFeedback` for a player who is visibly aiming with a gun is the symptom support should recognise.

## 5. Input

### As built

Raw 22-byte report (firmware `GamepadReport`), decoded in `BlamconLightgunHID.PreProcessEvent`:

| Offset | Field |
|---|---|
| 0 | report ID — 1 (P1), 3 (P2), 4 (P3), 5 (P4); anything else is dropped |
| 1–4 | 32 button bits: 0 trigger (`buttonWest`), 1 `buttonSouth`, 2 `buttonEast`, 3 `buttonNorth`, 4–7 shoulders/triggers, 8 select, 9 start, 10–11 stick presses |
| 5 | hat, low nibble |
| 6–9 / 10–13 | X / Y, int32 LE, 0–32767, Y = 0 at the top. Only the low 16 bits are read, lossless for that range |
| 14–21 | Rx / Ry — always zero in current firmware; exposed as `secondaryMotion`, unused |

* **D-pad maps cardinals only (1/3/5/7).** Diagonals are ignored **by design**; do not change this.
* `position` goes through `AbsolutePositionRemap`: 0..32767 → screen pixels (or 0..1 with
  `normalizeOnly`), Y inverted to Unity's bottom-up convention. No deadzone.
* Each gun in gamepad mode is its own device, so two guns are two players with independent aim.
* In mouse mode, aim and fire come from Unity's `Mouse` (constraint 7).

### Planned

* **Give the decoded state its own format code** (e.g. `'LGVS'`, already commented out in the source)
  so queued decoded states aren't decoded twice (constraint 9). Needs a hardware check: raw events must
  still arrive and convert.
* **Mouse parity sample.** One `Aim` action bound to `<Lightgun>/position` and `<Mouse>/position`, one
  `Fire` action bound to `<Lightgun>/buttonWest` and `<Mouse>/leftButton`, in one Input Actions asset.
  The Input System supports this natively; the current samples only partly do it.

## 6. Feedback

### As built

Per-device API on `BlamconLightgunHID` (`IForceFeedback`): `EnableFFBControl`, `EnableAmmoFFBControl`,
`ActivateRecoil`, `ActivateRumble`, `ActivateLED`, `SendAmmoCount`, plus `SendCommand` overloads for
each command struct. Each call sends its own `0x10` report immediately and returns whether the `HIDO`
succeeded. Clamps match the firmware: rumble 100–2400 ms, LED 20–5000 ms, recoil on 15–200 / off 45–200
ms, ammo 0–255. The LED fields are named by what the gun does (byte 25 = dark, 27 = lit); the firmware's
own labels are reversed.

Gaps:
* Feedback only works in gamepad mode.
* No player lookup: callers find a device themselves, and the sample uses
  `InputSystem.GetDevice<BlamconLightgunHID>()`, which returns one gun and can return a stale device
  (constraint 8).
* No session lifecycle: taking and releasing control is manual, so stopping play mode or crashing
  leaves the gun under app control (recoil no longer fires on the trigger).
* The single-component commands (`0x20`–`0x23`) are public and unreliable (constraint 4).
* Command size is hardcoded to 40 instead of `hidDescriptor.outputReportSize`.

### Planned

Everything goes through the existing interface:

```csharp
IForceFeedback gun = BlamconLightgunHID.GetForceFeedback(player);   // new; gamepad or mouse mode
gun?.ActivateRecoil(1);                                            // unchanged IForceFeedback calls
gun?.SendAmmoCount(ammoLeft);

var report = BlamconHIDOutputReport.Create();                      // unchanged: combine effects
report.SetRecoil(1);
report.SetAmmo(ammoLeft);
BlamconLightgunHID.SendCommand(player, ref report);                // new: by player, either mode
```

Rules:
* **Same contract in both modes.** Each call sends immediately and returns the `HIDO` result. No hidden
  batching; combining effects stays explicit through `BlamconHIDOutputReport` (constraint 6).
* **Routing.** A player resolves to its gamepad device, else its mouse-mode device; among duplicates,
  the most recently added (constraint 8).
* **Session lifecycle (`LightgunSession`).** On play start: release, then take control of recoil,
  rumble and LED. On `Application.quitting`, focus loss (`Application.focusChanged`) and Editor
  play-mode exit (`EditorApplication.playModeStateChanged`): release. Ammo is left to the game, because
  taking ammo control zeroes the display; send the starting count in the same report as taking it.
* **Size from the descriptor.** Build commands with `hidDescriptor.outputReportSize`, as Unity's own
  DualShock code does, falling back to 40.
* **Deprecate `0x20`–`0x23`.** Mark `BlamconRecoilCommand`, `BlamconRumbleCommand`, `BlamconLEDCommand`
  and `BlamconAmmoCommand` `[Obsolete]` with a message pointing at `BlamconHIDOutputReport`; remove in
  the next major version.
* **Samples** switch to `GetForceFeedback(player)`. In mouse mode a shot arrives from `Mouse`, which
  doesn't say which gun fired, so single-player mouse-mode samples target player 0.

## 7. Threading

`ExecuteCommand` runs on the main thread, and the Input System isn't safe to call from others, so there
is **no writer thread** (unlike the Unreal plugin), and feedback calls stay synchronous. How long the
native IOCTL blocks is undocumented; measure it on hardware over USB and Bluetooth (§11).

## 8. Platforms

| | Gamepad mode | Mouse mode (vendor collection) |
|---|---|---|
| **Windows** | Supported. Tested over USB and Bluetooth | Tested over USB and Bluetooth (§4.1); package support is milestone 1 |
| **macOS** | Unity supports HID here, so it should work; **not tested** | Unlikely: macOS makes one HID device per interface, and that device reports the mouse as its main usage |
| **UWP** | Not tested. Unity documents DualShock rumble/lightbar as "not working correctly" on UWP | Not tested |
| **Linux** | Not possible (constraint 3) | Not possible |
| Consoles, mobile, WebGL | Out of scope | Out of scope |

Windows users running HidHide must whitelist the game.

## 9. Milestones

1. **Mouse-mode feedback through `IForceFeedback`.** `BlamconMouseModeDevice` with the vendor-collection
   matchers, the shared feedback implementation, `BlamconLightgunHID.GetForceFeedback(player)` and
   `SendCommand(player, ref report)` with gamepad-first, newest-device routing, and docs for the
   firmware requirement.
   *Exit: the same `IForceFeedback` calls fire recoil, rumble, LED and ammo on a gun in gamepad mode and
   on a gun in mouse mode, over USB and Bluetooth, while the system mouse aims and fires.*
2. **Session lifecycle and samples.** `LightgunSession`; samples rewritten around
   `GetForceFeedback(player)` with mouse parity.
   *Exit: two guns in one scene, feedback reaches the right gun, and stopping play mode returns recoil
   to firing on the trigger.*
3. **Correctness debt.** Separate decoded-state format code, commands sized from the descriptor,
   single-component commands deprecated.
   *Exit: a test can queue `BlamconLightgunState.WithButton(...)` directly, and all existing tests pass.*

### Future milestones (deferred)

* **Device info through a native plugin** reading `0x50`/`0x51`, possibly sharing the Unreal plugin's
  engine-free `LightgunCore`. Gives version, board, feedback availability and leftover-control release.
* **Serial feedback** for other brands, behind `IForceFeedback`. Check `System.IO.Ports` availability
  for the project's API compatibility level first.
* **macOS** gamepad mode, once tested on hardware.

## 10. Acceptance tests

Automated (no hardware; `InputSystem.onDeviceCommand` intercepts `HIDO` so tests can assert the exact
bytes sent):
* Existing 6 tests stay green.
* A vendor-collection device description creates `BlamconMouseModeDevice`, which is **not** a
  `Lightgun`; the gamepad layouts don't claim it, and a `<Lightgun>/position` binding doesn't resolve
  to it.
* The same `IForceFeedback` call sends identical `0x10` bytes to `BlamconLightgunHID` and to
  `BlamconMouseModeDevice`.
* `GetForceFeedback(player)` prefers the gamepad device over the mouse-mode device, picks the newest of
  two same-PID devices, and returns `null` for a player with neither.
* Play-mode exit and `Application.quitting` send a release report.

On hardware:
* Gamepad mode, USB and Bluetooth: recoil, rumble, LED, ammo; aim reaches every screen edge.
* Mouse mode with the vendor collection, USB and Bluetooth: the same four effects through the same
  calls, and the mouse keeps aiming and firing while the device is in use.
* Two guns: feedback goes to the right gun; in gamepad mode aim is independent.
* Stop play mode mid-session: recoil returns to firing on the trigger.
* Reflash the gun without restarting the Editor: feedback reaches the new device, not the stale one.
* Mouse-only play works with the mouse-parity sample.

## 11. Open questions

Decided (2026-09-14):
* **Distribution:** Unity Package Manager from GitHub, as today.
* **Floor:** Unity 6000.0, Input System 1.14 (unchanged).
* **Primary platform:** Windows. macOS gamepad mode is best-effort until tested.
* **Feature reports:** not read from C# (§4.2).
* **Mouse-mode feedback goes through `IForceFeedback`,** not a separate API (§4.1).
* **Older firmware in mouse mode:** Unity doesn't see the gun; documented, not detected (§4.3).
* **No `IDualMotorRumble`:** the gun's feedback doesn't fit a two-motor rumble API.

Still open:
* **How long does `HIDO` block,** over USB and over Bluetooth? Decides whether commands need spacing.
* **Rumble and ammo through the vendor collection** — only recoil and LED were tested.
* **Should `LightgunSession` be automatic** (created on load) **or opt-in** (a component the developer
  adds)? Automatic is friendlier; opt-in avoids surprising games that already manage control.
* **Does changing the decoded-state format code break anything saved or recorded** (input recordings,
  `InputEventTrace` files)?
* **Emulators and front ends with the extra collection** — shared with the Unreal plugin; unchecked.
