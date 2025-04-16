using System;
using System.Collections.Generic;
using Drone.Motors;
using Drone.Stability;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace Drone
{
    /// <summary>
    /// Implementation of PID controllers used in drone.
    /// </summary>
    public enum PidControllerType { Default = 0, ValueDerivative, Debug }


    /// <summary>Abstract drone flight computer.</summary>
    [DisallowMultipleComponent]
    public abstract class DroneComputerBase : MonoBehaviour
    {
        private Vector3 _droneSize;

        [Tooltip("Rigidbody of the drone.")]
        [SerializeField]
        protected Rigidbody rigidBody;

        [Header("Common settings")]
        [SerializeField]
        [Tooltip("Settings that specify drone movement and stabilization.")]
        protected DroneControlSettings controlSettings;

        [SerializeField]
        [Tooltip("Implementation of the PID controller.")]
        protected PidControllerType pidController = PidControllerType.ValueDerivative;

        [SerializeField]
        [Tooltip("Do not apply negative forces to motors.")]
        protected bool clampNegativeForce;

        [SerializeField]
        [Tooltip("Show force vectors for all motors. Uses gizmos for rendering.")]
        protected bool showForceVectors;

        protected BasePidController pidThrottle, pidPitch, pidRoll, pidYaw;


        /// <summary>Rigidbody component of this drone.</summary>
        public Rigidbody Rigidbody => rigidBody;

        /// <summary>Max target vertical speed of the drone when using stabilization (meters per second).</summary>
        public float MaxLiftSpeed => controlSettings?.maxLiftSpeed ?? 0f;

        /// <summary>Max lift force generated by single drone motor in newtons.</summary>
        /// <remarks>Larger values mean that drone will reach target speed faster.</remarks>
        public float MaxLiftForce => controlSettings?.maxLiftForce ?? 0;

        /// <summary>Max torque generated by single drone motor.</summary>
        public float TorqueMultiplier => controlSettings?.torqueMultiplier ?? 0;

        /// <summary>Max pitch angle (X axis rotation) in degrees when using stabilization.</summary>
        public float MaxPitchAngle => controlSettings?.maxPitchAngle ?? 0;

        /// <summary>Max pitch force component value. Must be in range.</summary>
        /// <remarks>Larger values mean that drone will reach target pitch angle faster.</remarks>
        public float MaxPitchForce => controlSettings?.maxPitchForce ?? 0;

        /// <summary>Max yaw rotation speed (Y axis rotation) in degrees per second when using stabilization.</summary>
        public float MaxYawSpeed => controlSettings?.maxYawSpeed ?? 0;

        /// <summary>Max yaw force component value. Must be in range.</summary>
        /// <remarks>Larger values mean that drone will reach target yaw speed faster.</remarks>
        public float MaxYawForce => controlSettings?.maxYawForce ?? 0;

        /// <summary>Max roll angle (Z axis rotation) in degrees when using stabilization.</summary>
        public float MaxRollAngle => controlSettings?.maxRollAngle ?? 0;

        /// <summary>Max yaw force component value. Must be in range.</summary>
        /// <remarks>Larger values mean that drone will reach target roll angle faster.</remarks>
        public float MaxRollForce => controlSettings?.maxRollForce ?? 0;

        /// <summary>PID stabilizer for throttle control. Cast to child PID type to get additional info.</summary>
        public BasePidController PidThrottle => pidThrottle;

        /// <summary>PID stabilizer for pitch control. Cast to child PID type to get additional info.</summary>
        public BasePidController PidPitch => pidPitch;

        /// <summary>PID stabilizer for yaw control. Cast to child PID type to get additional info.</summary>
        public BasePidController PidYaw => pidYaw;

        /// <summary>PID stabilizer for roll control. Cast to child PID type to get additional info.</summary>
        public BasePidController PidRoll => pidRoll;


        /// <summary>Enumerate all drone motors.</summary>
        public abstract IEnumerable<DroneMotor> GetAllMotors();


        /// <summary>Reset all stabilizers state.</summary>
        public virtual void ResetStabilizers()
        {
            if (pidThrottle == null || pidPitch == null || pidYaw == null || pidRoll == null)
            {
                InitPidControllers();
                return;
            }

            pidThrottle.Reset();
            pidPitch.Reset();
            pidYaw.Reset();
            pidRoll.Reset();
        }

        /// <summary>Turn drone on or off by enabling or disabling this component. Also resets all PID stabilizers.</summary>
        public virtual void EnableDone(bool shouldEnable)
        {
            if (shouldEnable != enabled)
            {
                enabled = shouldEnable;
                ResetStabilizers();
            }
        }

        protected virtual void Awake()
        {
            if (!gameObject.TryGetDimensions(out _droneSize))
                _droneSize = Vector3.one;

            InitPidControllers();
        }

        private void InitPidControllers()
        {
            switch (pidController)
            {
            case PidControllerType.Default:
                pidThrottle = new PidController(controlSettings.pidThrottle);
                pidPitch = new PidController(controlSettings.pidPitch);
                pidRoll = new PidController(controlSettings.pidRoll);
                pidYaw = new PidController(controlSettings.pidYaw);
                break;
            case PidControllerType.ValueDerivative:
                pidThrottle = new ValueDerivativePidController(controlSettings.pidThrottle);
                pidPitch = new ValueDerivativePidController(controlSettings.pidPitch);
                pidRoll = new ValueDerivativePidController(controlSettings.pidRoll);
                pidYaw = new ValueDerivativePidController(controlSettings.pidYaw);
                break;
            case PidControllerType.Debug:
                pidThrottle = new DebugPidController(controlSettings.pidThrottle);
                pidPitch = new DebugPidController(controlSettings.pidPitch);
                pidRoll = new DebugPidController(controlSettings.pidRoll);
                pidYaw = new DebugPidController(controlSettings.pidYaw);
                break;
            }
        }

        protected void OnValidate()
        {
            switch (pidController)
            {
            case PidControllerType.Default         when pidThrottle is not PidController:
            case PidControllerType.ValueDerivative when pidThrottle is not ValueDerivativePidController:
            case PidControllerType.Debug           when pidThrottle is not DebugPidController:
                InitPidControllers();
                break;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (!showForceVectors || !enabled) return;

            var labelFmt = $"{{0}} ({{1:F2}} | {controlSettings.maxLiftForce:F0})";
            var options = new GizmoOptions(Color.red, labelColor: Color.red);

            foreach (var motor in GetAllMotors())
            {
                VectorDrawer.DrawLabel(motor.transform.position,
                    string.Format(labelFmt, motor.name, motor.ForceVector.magnitude),
                    options
                );
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!showForceVectors || !enabled) return;

            var vectMult = math.cmax(_droneSize) / controlSettings.maxLiftForce / 2;
            var options = new GizmoOptions(Color.red,
                capSize: math.cmin(_droneSize) / 8,
                vectSize: math.cmax(_droneSize) / 2
            );

            foreach (var motor in GetAllMotors())
            {
                var dot = Vector3.Dot(transform.up, motor.ForceVector.normalized);
                if (math.abs(dot - 1f) >= 1e-5)
                {
                    options.Color = Color.magenta;
                }
                
                VectorDrawer.DrawDirection(motor.transform.position,
                    motor.ForceVector * vectMult,
                    "",
                    options
                );
            }
        }
    }
}