using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Samples.LightgunCrosshair
{
    /// <summary>
    /// This is a sample script that controls a game cursor.
    /// Attach this script to GameObject that represents the cursor component or create one.
    /// This expects a transform that can be re-positioned based on the reported player 
    /// position.
    ///
    /// Add this script to the PlayerInput Event for the OnMove callback function to be called.
    /// </summary>
    public class LightgunCrosshair : MonoBehaviour
    {
        // The point at which you aim when shooting.
        [SerializeField] private Transform crosshair;

        /// <summary>
        /// Get the current position of the crosshair
        /// </summary>
        public Vector3 Position { get => crosshair.position; }


        /// <summary>
        /// This is configured on the PlayerInput events
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveCursor(context);
            }
        }

        /// <summary>
        /// Start is only called once in the lifetime of the behaviour.
        /// The difference between Awake and Start is that Start is only called if the script instance is enabled.
        /// This allows you to delay any initialization code, until it is really needed.
        /// Awake is always called before any Start functions.
        /// This allows you to order initialization of scripts
        /// </summary>
        void Start()
        {
        }

        /// <summary>
        /// For moving the cursor/crosshair.
        /// </summary>
        /// <param name="context"></param>
        void MoveCursor(InputAction.CallbackContext context)
        {
            Vector3 aimPosition = getCursorScreenPosition(context);

            // Limit the position of the crosshair to the edges of the screen
            // Keep the cursor in the game screen (behavior gets weird out of bounds)       
            // Limit to the left screen edge
            if (aimPosition.x < Camera.main.ScreenToWorldPoint(Vector3.zero).x) aimPosition = new Vector3(Camera.main.ScreenToWorldPoint(Vector3.zero).x, aimPosition.y, aimPosition.z);

            // Limit to the right screen edge
            if (aimPosition.x > Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x) aimPosition = new Vector3(Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x, aimPosition.y, aimPosition.z);

            // Limit to the bottom screen edge
            if (aimPosition.y < Camera.main.ScreenToWorldPoint(Vector3.zero).y) aimPosition = new Vector3(aimPosition.x, Camera.main.ScreenToWorldPoint(Vector3.zero).y, aimPosition.z);

            // Limit to the top screen edge
            if (aimPosition.y > Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height).y) aimPosition = new Vector3(aimPosition.x, Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height).y, aimPosition.z);

            // Place the crosshair at the position of the mouse/tap, with an added offset
            crosshair.position = aimPosition;
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
            } else if (context.control.device is Mouse) {
                rawInput = Mouse.current.position.ReadValue();
            } else if (context.control.device is Lightgun) {
                rawInput = Lightgun.current.position.ReadValue();
            }
            return Camera.main.ScreenToWorldPoint(new Vector3(Mathf.Clamp(rawInput.x, 0, Screen.width), Mathf.Clamp(rawInput.y, 0, Screen.height), Camera.main.nearClipPlane));
        }
    }
}
