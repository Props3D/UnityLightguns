using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Processors;


namespace Blamcon.Lightguns.Processors
{

    /// <summary>
    /// A generic input processor that remaps a Vector2 value from a configurable input range
    /// into screen pixel coordinates or normalized [0–1] viewport coordinates.
    /// </summary>
    /// <remarks>
    /// This is commonly used for absolute-position input devices like lightguns or tablets
    /// where incoming values range from fixed bounds (e.g., 0–32767, 0–65535, or even signed ranges).
    /// </remarks>
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] // Make sure static constructor is called during startup.
    #endif
    public class AbsolutePositionRemapProcessor : InputProcessor<Vector2>
    {
        static AbsolutePositionRemapProcessor() {
            InputSystem.RegisterProcessor<AbsolutePositionRemapProcessor>("AbsolutePositionRemap");
        }
        // In the Player, to trigger the calling of the static constructor,
        // create an empty method annotated with RuntimeInitializeOnLoadMethod.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {}

        /// <summary>
        /// Minimum raw input value expected on the X axis.
        /// </summary>
        [Tooltip("Minimum raw input value expected on the X axis.")]
        public float inputMinX = 0f;

        /// <summary>
        /// Maximum raw input value expected on the X axis.
        /// </summary>
        [Tooltip("Maximum raw input value expected on the X axis.")]
        public float inputMaxX = 32767f;

        /// <summary>
        /// Minimum raw input value expected on the Y axis.
        /// </summary>
        [Tooltip("Minimum raw input value expected on the Y axis.")]
        public float inputMinY = 0f;

        /// <summary>
        /// Maximum raw input value expected on the Y axis.
        /// </summary>
        [Tooltip("Maximum raw input value expected on the Y axis.")]
        public float inputMaxY = 32767f;

        /// <summary>
        /// If true, outputs values in normalized [0–1] space instead of screen pixel coordinates.
        /// </summary>
        [Tooltip("If true, outputs values in normalized [0–1] space instead of screen pixel coordinates.")]
        public bool normalizeOnly = false;

        /// <summary>
        /// If true, flips the Y-axis so that 0 maps to the bottom instead of the top.
        /// </summary>
        [Tooltip("If true, flips the Y-axis so that 0 maps to the bottom instead of the top.")]
        public bool invertY = true;

        /// <summary>
        /// Converts raw input coordinates to either screen-space pixels or normalized viewport values,
        /// with optional Y-axis inversion.
        /// </summary>
        /// <param name="value">The raw Vector2 value from the input device.</param>
        /// <param name="control">The control associated with the input binding.</param>
        /// <returns>Mapped Vector2 in screen or normalized space.</returns>
        public override Vector2 Process(Vector2 value, InputControl control)
        {
            short x = (short)value.x;
            short y = (short)value.y;
            float normalizedX = Mathf.InverseLerp(inputMinX, inputMaxX, x);
            float normalizedY = Mathf.InverseLerp(inputMinY, inputMaxY, y);

            if (invertY)
                normalizedY = 1f - normalizedY;

            if (normalizeOnly)
                return new Vector2(normalizedX, normalizedY);

            return new Vector2(normalizedX * Screen.width, normalizedY * Screen.height);
        }
    }
}
