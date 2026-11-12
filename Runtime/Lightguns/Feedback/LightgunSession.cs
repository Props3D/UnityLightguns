using UnityEngine;
using UnityEngine.InputSystem;
using Blamcon.Lightguns.LowLevel;

namespace Blamcon.Lightguns
{
    /// <summary>
    /// Takes control of lightgun feedback while this component is enabled, so the guns stop driving
    /// recoil, rumble and LEDs themselves (for example, firing recoil on every trigger pull) and only do
    /// what the game asks. Opt-in: add one to a scene, or to a persistent object to cover every scene.
    /// </summary>
    /// <remarks>
    /// Takes control when enabled, and of guns that connect while it is enabled. Hands control back when
    /// disabled or destroyed, which covers leaving play mode and quitting, and while the application
    /// doesn't have focus. Guns in Gamepad mode and in mouse mode are both covered.
    ///
    /// Only the ticked components are touched. Taking or handing back control turns the LED off and stops
    /// rumble. Taking ammo control zeroes the ammo display, so ammo is off by default; to manage the display,
    /// take ammo control and send the starting count in one report
    /// (<see cref="BlamconHIDOutputReport.EnableAmmoFFBControl"/> then <see cref="BlamconHIDOutputReport.SetAmmo"/>).
    ///
    /// Keep one enabled session at a time: two send every command twice.
    /// </remarks>
    [AddComponentMenu("Blamcon/Lightgun Session")]
    [DisallowMultipleComponent]
    public class LightgunSession : MonoBehaviour
    {
        [Tooltip("Take control of the recoil solenoid.")]
        [SerializeField] bool m_Recoil = true;

        [Tooltip("Take control of the rumble motor.")]
        [SerializeField] bool m_Rumble = true;

        [Tooltip("Take control of the LED.")]
        [SerializeField] bool m_Led = true;

        [Tooltip("Take control of the ammo display. This zeroes the display, so send the starting count yourself.")]
        [SerializeField] bool m_Ammo;

        [Tooltip("Hand control back while the application doesn't have focus, and take it again when focus returns.")]
        [SerializeField] bool m_ReleaseOnFocusLoss = true;

        LightgunSessionController m_Controller;

        /// <summary>True while this session holds control of the guns.</summary>
        public bool isActive => m_Controller != null && m_Controller.active && !m_Controller.paused;

        void OnEnable()
        {
            m_Controller = new LightgunSessionController(() => InputSystem.devices)
            {
                recoil = m_Recoil,
                rumble = m_Rumble,
                led = m_Led,
                ammo = m_Ammo,
            };
            InputSystem.onDeviceChange += OnDeviceChange;
            m_Controller.Begin();
        }

        void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            m_Controller?.End();
            m_Controller = null;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!m_ReleaseOnFocusLoss || m_Controller == null)
                return;
            if (hasFocus)
                m_Controller.Resume();
            else
                m_Controller.Pause();
        }

        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
                m_Controller?.DeviceAdded(device);
        }
    }
}
