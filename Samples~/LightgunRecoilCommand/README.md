# Lightgun Recoil on Trigger Action

This is a sample script that demonstrates how to tranlate the OnFire input action callback to issue a recoil command to the Blamcon lightgun. Using an ammo counter, if there is still ammo available a shot object is created, and a recoil command is sent to the lightgun device.

Attach the trigger action [script](./LightgunTriggerAcation.cs) to GameObject that represents the game controller or player object. This script has a configurable transform
object that represents a Shot Object. This shot object will be instantiated at the cursor location when the trigger is actioned on the player input OnFire callback. 

Your `PlayerInput` component should reference the [actions](./LightgunInputActions.inputactions) available to the player which, by means of the control schemes defined in the asset, also determine the devices (and combinations of devices) supported by the game. The actions available to the player are kept simple for this demonstration.

Typical lightgun actions and bindings:
- Fire - Lightgun/buttonWest
- Reload - Lightgun/buttonSouth
- Submit - Lightgun/buttonEast
- Move - Lightgun/position

Add the trigger action game object to the PlayerInput Event for the OnFire callback function to be called appropriately whn the user pressed the trigger.
