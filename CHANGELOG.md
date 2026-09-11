# Changelog

All notable changes to the Blamcon Lightgun package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

Due to package verification, the latest version below is the unpublished version and the date is meaningless.
however, it has to be formatted properly to pass verification tests.

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
