# Changelog

All notable changes to the Blamcon Lightgun package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

Due to package verification, the latest version below is the unpublished version and the date is meaningless.
however, it has to be formatted properly to pass verification tests.

## [2.0.0] - 2026-09-14

Force feedback for guns in mouse mode, feedback addressed by player, an opt-in session that manages
feedback control, and removal of the single-component output commands. See **Upgrading from 1.x** in
the documentation.

### Added
- Force feedback for guns in **mouse mode**, through the same `IForceFeedback` interface. Firmware with
  mouse-mode feedback adds a vendor-defined HID collection (usage page `0xFF00`, usage `0x01`); the
  package matches it as a feedback-only device that isn't a `Lightgun`, so it never appears in lightgun
  bindings. Its only control is a noisy, synthetic placeholder, because the Input System can't build a
  device with no controls. Aim and fire still come from Unity's `Mouse`. Older firmware in mouse mode
  is invisible to Unity.
- `BlamconLightgunHID.GetForceFeedback(player)` returns a player's gun as `IForceFeedback` in either mode:
  the Gamepad-mode device if there is one, otherwise the mouse-mode device, and the most recently added
  if a gun is listed twice (Unity can keep a gun's old devices listed after a firmware update).
- `BlamconLightgunHID.SendCommand(player, ref report)` sends a combined `BlamconHIDOutputReport` to a
  player's gun in either mode.
- `LightgunSession`, an opt-in component that takes control of recoil, rumble and the LED on every gun
  while it is enabled, including guns that connect later, and hands control back when disabled,
  destroyed, when play mode stops, on quit and (by default) while the application lacks focus. It touches
  only the components it is set to, so it never releases ammo control a game is holding.
- `BlamconLightgunHID.playerIndex` is public, so games can tell which player's gun fired.

### Changed
- The Lightgun Recoil Command sample sends feedback to the player whose gun fired, with a configurable
  player for mouse clicks, and relies on `LightgunSession` for recoil control instead of taking control
  itself. It no longer has an `EnableForcedFeedbackControl` method.
- Documentation lists the firmware each feature needs in a firmware compatibility table: force feedback
  over Bluetooth Classic, force feedback in mouse mode, and rumble and LED periods that are exact
  multiples of 256 ms (dropped by older firmware).

### Removed
- **Breaking:** the single-component output commands `BlamconRecoilCommand`, `BlamconRumbleCommand`,
  `BlamconLEDCommand` and `BlamconAmmoCommand`, and their `BlamconLightgunHID.SendCommand` overloads.
  They never reached the gun: Unity pads every HID output command to the device's largest output report
  (40 bytes), and the firmware accepts reports `0x20`-`0x23` only at their exact size. Build the same
  effect with `BlamconHIDOutputReport`, or call the `IForceFeedback` methods (`ActivateRecoil`,
  `ActivateRumble`, `ActivateLED`, `SendAmmoCount`):

  | 1.x | 2.0 |
  |---|---|
  | `BlamconRecoilCommand.Create(pulses[, on, off])` | `var report = BlamconHIDOutputReport.Create();` then `report.SetRecoil(pulses[, on, off])` |
  | `BlamconRumbleCommand.Create(pulses[, on, off])` | `report.SetRumble(pulses[, on, off])` |
  | `BlamconLEDCommand.Create(index, color[, flashes[, on, off]])` | `report.SetColor(index, color[, flashes[, on, off]])` |
  | `BlamconAmmoCommand.Create(remaining)` | `report.SetAmmo(remaining)` |
  | `command.EnableFFBControl(enable)` | `report.EnableFFBControl(recoil, rumble, led, ammo)`, or set that component's `enable...Update` and `enable...FFBControl` fields |
  | `device.SendCommand(ref command)` | `device.SendCommand(ref report)` |

  One report can carry several effects, which the gun handles better than separate commands sent back to back.

## [1.1.0] - 2026-09-12

Verified compatibility with Blamcon release-3.0 firmware (3.0.0).

### Fixed
- Enable unsafe code in the `Blamcon.Lightguns` assembly definition, so the package compiles without a project-level `csc.rsp`.
- Input reports with an unrecognized report ID no longer register a phantom trigger (`buttonWest`) press.
  They are now dropped and the device keeps its last good state. The internal
  `LightgunHIDInputReport.EmptyHIDInputReport()` was removed. It previously replaced the state with
  `buttons = 0x01` and a zeroed position, which also released held buttons and snapped the cursor to
  `(0,0)`. See the comment in `BlamconLightgunHID.PreProcessEvent` to restore a reset-to-neutral
  behavior.
- `BlamconRumbleCommand.SetRumble(pulse, on, off)` ignored `pulse`, so the rumble count was never set.
- Ammo counts are clamped to 0-255 instead of wrapping.
- Samples now compile and send recoil plus ammo in a single output report, with ammo control enabled.

### Changed
- Feedback timing clamps now match the firmware 3.0 component limits: rumble 100-2400 ms (was 100-2000),
  LED flash 20-5000 ms (was 40-2000, or 100-2000 in `BlamconLEDCommand.Create`), recoil on 15-200 ms and
  off 45-200 ms (was 15-255 for both, which the firmware capped/raised anyway).
- Removed a duplicate `size < sizeof(LightgunHIDInputReport)` check in `PreProcessEvent`; the same check
  already runs in the preceding format/size guard. No behavior change.

## [1.0.0] - 2025-10-7

Adding suppport for sending Ammo Count as device feedback.

## [0.9.0] - 2025-05-16

First release from stable branch.
