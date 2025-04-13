using Drone.Stability;
using Inputs;
using Unity.Mathematics;
using UnityEngine;


namespace Drone
{
    /// <summary>Drone control inputs manager.</summary>
    /// <remarks>Reads manual inputs with <see cref="DroneControls"/> at Update function, or takes AI inputs from SetInputs on demand.</remarks>
    [DisallowMultipleComponent]
    public class DroneInputsController : MonoBehaviour
    {
        public const DroneStabilizerMode FULL_STAB =
            DroneStabilizerMode.StabAltitude |
            DroneStabilizerMode.StabPitchRoll |
            DroneStabilizerMode.StabYaw;

        /* y (yaw)
           |
           |  z|fwd (roll)
           |/
           +------x (pitch - nose up/down) */

        private DroneControls _controls;

        [Tooltip("Turn on/off reading inputs from user.")]
        public bool manualInput = true;

        [Tooltip("Use legacy Unity Input manager instead of new Inputs system.")]
        public bool useLegacyInputs;

        [Tooltip("Flight stabilization mode.")]
        public DroneStabilizerMode stabilizerMode =
            DroneStabilizerMode.StabAltitude | DroneStabilizerMode.StabPitchRoll;

        /// <summary>Desired throttle value. Range [-1, 1].</summary>
        [HideInInspector]
        public float throttle;

        /// <summary>Desired pitch value. Rotation along X (right) axis. Range [-1, 1].</summary>
        [HideInInspector]
        public float pitch;

        /// <summary>Desired yaw value. Rotation along Y (up) axis. Range [-1, 1].</summary>
        [HideInInspector]
        public float yaw;

        /// <summary>Desired roll value. Rotation along Z (forward) axis. Range [-1, 1].</summary>
        [HideInInspector]
        public float roll;


        private void Awake() => _controls = new DroneControls();

        private void OnEnable() => _controls?.Enable();

        private void OnDisable() => _controls?.Disable();

        private void OnValidate()
        {
            if (manualInput) _controls?.Enable();
            else _controls?.Disable();
        }

        private void Update()
        {
            if (!manualInput) return;

            if (useLegacyInputs) ReadLegacyInputs();
            else ReadInputs();
        }

        private void ReadInputs()
        {
            var rotation = _controls.Default.Rotation.ReadValue<Vector3>();
            pitch = rotation.x;
            yaw = rotation.y;
            roll = rotation.z;
            throttle = _controls.Default.Throttle.ReadValue<float>();

            var lastStab = stabilizerMode;
            if (_controls.Default.FullStabilization.WasPressedThisFrame())
                ToggleStabilization();

            if (lastStab != stabilizerMode)
                Debug.LogFormat("Drone {0} stabilizer mode: {1}", name, stabilizerMode);
        }

        private void ReadLegacyInputs()
        {
            pitch = Input.GetAxis("Pitch");
            yaw = Input.GetAxis("Yaw");
            roll = Input.GetAxis("Roll");
            throttle = Input.GetAxis("Throttle");

            var lastStab = stabilizerMode;
            if (Input.GetKey(KeyCode.Space))
                ToggleStabilization();

            if (lastStab != stabilizerMode)
                Debug.LogFormat("Drone {0} stabilizer mode: {1}", name, stabilizerMode);
        }

        private void ToggleStabilization()
        {
            if (stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude))
                stabilizerMode = DroneStabilizerMode.None;
            else
                stabilizerMode = DroneStabilizerMode.StabAltitude | DroneStabilizerMode.StabPitchRoll |
                                 DroneStabilizerMode.StabYaw;
        }

        /// <summary>Set control inputs manually.</summary>
        /// <remarks>Values will always be clamped in [-1, 1] range.</remarks>
        public void SetInputs(float throttle, float pitch, float yaw, float roll)
        {
            this.throttle = math.clamp(throttle, -1, 1);
            this.pitch = math.clamp(pitch, -1, 1);
            this.yaw = math.clamp(yaw, -1, 1);
            this.roll = math.clamp(roll, -1, 1);
        }

        public bool IsFullStabilization() => stabilizerMode == FULL_STAB;

        public void SetFullStabilization() { stabilizerMode = FULL_STAB; }
    }
}