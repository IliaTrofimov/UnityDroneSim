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
    // TODO: move important parts of drone as interface here


    public enum PidControllerType
    {
        Default = 0, ValueDerivative, Debug 
    }
    
    /// <summary>Abstract drone flight computer.</summary>
    [DisallowMultipleComponent]
    public abstract class DroneComputerBase : MonoBehaviour
    {
        private Vector3 droneSize;
        
        [Tooltip("Rigidbody of the drone.")]
        [SerializeField] protected Rigidbody rigidBody;
        
        [Header("Common settings")]
        [Tooltip("Settings that specify drone movement and stabilization.")]
        [SerializeField] protected DroneControlSettings controlSettings;
        
        [Tooltip("Implementation of the PID controller.")]
        [SerializeField] protected PidControllerType pidController = PidControllerType.ValueDerivative;
        
        [Tooltip("Do not apply negative forces to motors.")]
        [SerializeField] protected bool clampNegativeForce;
        
        [Tooltip("Show force vectors for all motors. Uses gizmos for rendering.")] 
        [SerializeField] protected bool showForceVectors;
        
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
            pidThrottle.Reset();   
            pidPitch.Reset();
            pidYaw.Reset();
            pidRoll.Reset();
        }
        
        protected virtual void Awake()
        {
            if (!gameObject.TryGetDimensions(out droneSize))
                droneSize = Vector3.one;

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
        
        protected virtual void OnDrawGizmos()
        {
            if (!showForceVectors || !enabled) return;
            
            var labelFmt = $"{{0}} ({{1:F2}} | {controlSettings.maxLiftForce:F0})";
            var options = new GizmoOptions(Color.red, labelColor: Color.red);

            foreach (var motor in GetAllMotors())
            {
                VectorDrawer.DrawLabel(motor.transform.position, 
                    string.Format(labelFmt, motor.name, motor.ForceVector.magnitude),
                    options);
            }
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            if (!showForceVectors || !enabled) return;
            
            var vectMult = math.cmax(droneSize) / controlSettings.maxLiftForce;
            var options = new GizmoOptions(Color.red,
                capSize: math.cmin(droneSize) / 3,
                vectSize: math.cmax(droneSize));

            foreach (var motor in GetAllMotors())
            {
                VectorDrawer.DrawDirection(motor.transform.position, 
                    motor.ForceVector * vectMult,
                    "",
                    options);
            }
        }
    }
}