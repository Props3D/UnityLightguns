# Lightgun Recoil on Trigger Action

This is a sample script that demonstrates how to translate the OnFire input action callback into recoil and an ammo count on a Blamcon lightgun. Using an ammo counter, if there is still ammo available a shot object is created, and recoil plus the new ammo count are sent to the gun that fired.

It works with a gun in Gamepad mode or in mouse mode, and with a plain mouse:

- **Gamepad mode:** the shot comes from the gun itself, so feedback goes to that gun's player.
- **Mouse mode, or a mouse:** Unity sees a mouse click, which can't say which gun fired, so feedback goes to the player set in **Mouse Player** (0 is player 1). Mouse-mode feedback needs Blamcon firmware 2.1.0 or later over USB, or 4.0.0 or later over Bluetooth Classic.

## Setup

1. Attach the trigger action [script](./LightgunTriggerAction.cs) to a GameObject that represents the game controller or player object. Its **Shot Object** is instantiated at the cursor location each time the trigger fires.
2. Add a **Lightgun Session** component (**Add Component → Blamcon → Lightgun Session**) to the same GameObject or another one in the scene. It takes control of recoil, rumble and the LED while the scene runs, so the guns only recoil when the script asks rather than on every trigger pull, and hands control back when play stops.
3. Your `PlayerInput` component should reference the [actions](./LightgunInputActions.inputactions) available to the player. By means of the control schemes defined in the asset, these also determine the devices supported by the game. The actions are kept simple for this demonstration.
4. Add the trigger action GameObject to the PlayerInput Event for the OnFire callback, so it is called when the player pulls the trigger.

Each action is bound to both the lightgun and the mouse, so the same scene works with either:

- Fire - `Lightgun/buttonWest`, `Mouse/leftButton`
- Reload - `Lightgun/buttonSouth`, `Mouse/rightButton`
- Submit - `Lightgun/buttonEast`, `Mouse/middleButton`
- Move - `Lightgun/position`, `Mouse/position`

The script takes control of the ammo display itself at start, because taking ammo control zeroes the display and the starting count has to go in the same report. Call `EndGame()` to hand the display back.
