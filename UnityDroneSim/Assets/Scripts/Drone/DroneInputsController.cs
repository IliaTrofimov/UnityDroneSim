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

        [Tooltip("Lower output control values to make more precise movements.")]
        public bool preciseMovement;
        
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
            if (_controls.Default.FullStabilization.WasPressedThisFrame())
                ToggleStabilization();
            if (_controls.Default.PreciseMode.WasPressedThisFrame())
                preciseMovement = !preciseMovement;
            
            var rotation = _controls.Default.Rotation.ReadValue<Vector3>();
            throttle = _controls.Default.Throttle.ReadValue<float>();
            
            if (preciseMovement)
            {
                pitch = rotation.x / 2f;
                yaw = rotation.y / 2f;
                roll = rotation.z / 2f;   
            }
            else
            {
                pitch = rotation.x;
                yaw = rotation.y;
                roll = rotation.z;
            }
        }

        private void ReadLegacyInputs()
        {
            if (Input.GetKey(KeyCode.Space))
                ToggleStabilization();
            if (Input.GetKey(KeyCode.LeftShift))
                preciseMovement = !preciseMovement;
            
            throttle = Input.GetAxis("Throttle");   
            if (preciseMovement)
            {
                pitch = Input.GetAxis("Pitch") / 2f;
                yaw = Input.GetAxis("Yaw") / 2f;
                roll = Input.GetAxis("Roll") / 2f;
            }
            else
            {
                pitch = Input.GetAxis("Pitch");
                yaw = Input.GetAxis("Yaw");
                roll = Input.GetAxis("Roll");
            }
        }

        private void ToggleStabilization()
        {
            if (stabilizerMode == DroneStabilizerMode.None)
                stabilizerMode = FULL_STAB;
            else
                stabilizerMode = DroneStabilizerMode.None;
        }

        /// <summary>Set control inputs manually.</summary>
        /// <remarks>Values will always be clamped in [-1, 1] range.</remarks>
        public void SetInputs(float throttle, float pitch, float yaw, float roll)
        {
            var clamp = preciseMovement ? 0.5f : 1.0f;
            this.throttle = math.clamp(throttle, -clamp, clamp);
            this.pitch = math.clamp(pitch, -clamp, clamp);
            this.yaw = math.clamp(yaw, -clamp, clamp);
            this.roll = math.clamp(roll, -clamp, clamp);
        }

        public bool IsFullStabilization() => stabilizerMode == FULL_STAB;
    }
}