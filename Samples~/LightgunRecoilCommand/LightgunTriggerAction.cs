using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Blamcon.Lightguns;
using Blamcon.Lightguns.LowLevel;

namespace Samples.LightgunRecoilCommand
{
    /// <summary>
    /// This script controls the game cursor.
    /// </summary>
    public class LightgunTriggerAction : MonoBehaviour
    {
        // The bullet/shot that appears when you shoot - references a prefab object
        [SerializeField] private Transform shotObject;
        // The number of bullets left
        private int ammoLeft;


        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Start()
        {
            ammoLeft = 99;
            // enable forced feedback control so that only game commands cause recoil actions.
            EnableForcedFeedbackControl(true);
        }

        /// <summary>
        /// Disable the forced feedback control when game ends.
        /// </summary>
        public void EndGame()
        {
            EnableForcedFeedbackControl(false);
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
        /// Attempts to enable (or release) the forced feedback control to the game application.
        /// </summary>
        public void EnableForcedFeedbackControl(bool enable)
        {
            BlamconLightgunHID device = InputSystem.GetDevice<BlamconLightgunHID>();
            if (device != null)
            {
                // Ammo control must be enabled for the device to accept ammo counts.
                // Send the enable and the initial ammo count together in one report.
                var command = BlamconHIDOutputReport.Create(enable, enable, enable, enable);
                if (enable)
                    command.SetAmmo(ammoLeft);
                device.SendCommand(ref command);
            }
        }

        /// <summary>
        /// Recoils the device that fired and updates its ammo display.
        /// </summary>
        void RecoilCommand(InputDevice device) {
            if (device is BlamconLightgunHID lightgun) {
                // Combine recoil and ammo count into a single output report, since
                // back-to-back commands may be dropped by the device.
                var command = BlamconHIDOutputReport.Create();
                command.SetRecoil(1);
                command.SetAmmo(ammoLeft);
                lightgun.SendCommand(ref command);
            }
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
