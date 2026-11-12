using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Blamcon.Lightguns;
using Blamcon.Lightguns.LowLevel;

namespace Samples.LightgunRecoilCommand
{
    /// <summary>
    /// Shoots on the Fire action, with recoil and an ammo count on the gun that fired.
    /// Works with guns in Gamepad mode or mouse mode, and with a plain mouse.
    /// </summary>
    /// <remarks>
    /// Add a <see cref="LightgunSession"/> to the scene as well, so the guns stop firing recoil on every
    /// trigger pull and only recoil when this script asks.
    /// </remarks>
    public class LightgunTriggerAction : MonoBehaviour
    {
        // The bullet/shot that appears when you shoot - references a prefab object
        [SerializeField] private Transform shotObject;

        // A shot from a mouse can't tell which gun fired (a gun in mouse mode is a mouse to Unity),
        // so its feedback goes to this player. 0 is player 1.
        [SerializeField] private int mousePlayer = 0;

        // The number of bullets left
        private int ammoLeft;

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// </summary>
        void Start()
        {
            ammoLeft = 99;

            // The ammo display only takes counts while the game holds ammo control, and taking control
            // zeroes it, so take control and send the starting count in the same report.
            var report = BlamconHIDOutputReport.Create();
            report.EnableAmmoFFBControl(true);
            report.SetAmmo(ammoLeft);
            SendToEveryPlayer(ref report);
        }

        /// <summary>
        /// Hands the ammo display back to the guns when the game ends.
        /// </summary>
        public void EndGame()
        {
            var report = BlamconHIDOutputReport.Create();
            report.EnableAmmoFFBControl(false);
            SendToEveryPlayer(ref report);
        }

        /// <summary>
        /// This is configured on the PlayerInput events
        /// </summary>
        /// <param name="context"></param>
        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ShootGun(context);
            }
        }

        /// <summary>
        /// Shoots!
        /// </summary>
        public void ShootGun(InputAction.CallbackContext context)
        {
            if (ammoLeft > 0 && shotObject)
            {
                // Create a new shot at the position of the mouse/tap
                Transform newShot = Instantiate(shotObject) as Transform;

                // Place the shot at the position of the click
                newShot.transform.position = getCursorScreenPosition(context);

                // Reduce from ammo
                ammoLeft--;

                // Send recoil command
                RecoilCommand(context.control.device);
            }
        }

        /// <summary>
        /// Recoils the gun that fired and updates its ammo display.
        /// </summary>
        void RecoilCommand(InputDevice device)
        {
            // Recoil and the new ammo count go in one report so they take effect together. The gun
            // queues reports and applies them in order, but ignores a recoil that arrives while the
            // previous pulses are still running.
            var report = BlamconHIDOutputReport.Create();
            report.SetRecoil(1);
            report.SetAmmo(ammoLeft);
            BlamconLightgunHID.SendCommand(PlayerFor(device), ref report);
        }

        /// <summary>
        /// The player whose gun fired: a gun in Gamepad mode knows its player; a mouse doesn't.
        /// </summary>
        int PlayerFor(InputDevice device)
        {
            if (device is BlamconLightgunHID gun && gun.playerIndex >= 0)
                return gun.playerIndex;
            return mousePlayer;
        }

        /// <summary>
        /// Sends a report to every connected player's gun. Players without a gun are skipped.
        /// </summary>
        static void SendToEveryPlayer(ref BlamconHIDOutputReport report)
        {
            for (var player = 0; player < 4; player++)
                BlamconLightgunHID.SendCommand(player, ref report);
        }

        /// <summary>
        /// Converts the screen coordinates from device to World position.
        /// </summary>
        /// <returns></returns>
        Vector2 getCursorScreenPosition(InputAction.CallbackContext context)
        {
            Vector2 rawInput = default(Vector2);
            if (context.control is Vector2Control) {
                rawInput = context.ReadValue<Vector2>();
            } else if (context.control.device is Mouse mouse) {
                rawInput = mouse.position.ReadValue();
            } else if (context.control.device is Lightgun lightgun) {
                rawInput = lightgun.position.ReadValue();
            }
            return Camera.main.ScreenToWorldPoint(new Vector3(Mathf.Clamp(rawInput.x, 0, Screen.width), Mathf.Clamp(rawInput.y, 0, Screen.height), Camera.main.nearClipPlane));
        }
    }
}
