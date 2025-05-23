# Simple Lightgun Cursor

This is a sample script that controls a game cursor or crosshair. Attach the crosshair [script](./LightgunCrosshair.cs) to GameObject that represents the cursor component or create a new one. The cursor should be a transform that can be re-positioned based on the reported player position.

The player or controller GameObject can have a `PlayerInput` component added to it. The `PlayerInput` component references the [actions](./LightgunInputActions.inputactions) available to the player which, by means of the control schemes defined in the asset, also determine the devices (and combinations of devices) supported by the game. The actions available to the player are kept simple for this demonstration.

Typical lightgun actions and bindings:
- Fire - Lightgun/buttonWest
- Reload - Lightgun/buttonSouth
- Submit - Lightgun/buttonEast
- Move - Lightgun/position

Add the cursor/crosshair object to the PlayerInput Event for the OnMove callback function to be called appropriately.
