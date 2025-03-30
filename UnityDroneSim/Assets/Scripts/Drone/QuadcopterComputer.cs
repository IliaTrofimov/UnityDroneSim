using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Drone.Motors;
using Drone.Stability;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace Drone
{
    /// <summary>
    /// Quadcopter flight computer.
    /// Reads control values from <see cref="DroneInputsController"/> and manages motors' power and stabilization.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DroneInputsController))]
    public class QuadcopterComputer : DroneComputerBase
    {
        private Vector3 droneSizes;
        private DroneInputsController inputController;

        /// <summary>Rigidbody of the drone.</summary>
        public Rigidbody rigidBody;
        
        /// <summary>Settings toggles.</summary>
        public bool showForceVectors, clampNegativeForce, useVelocityStab;

        /// <summary>Movement and stabilization settings.</summary>
        public DroneControlSettings controlSettings;

        /// <summary>Quadcopter motors.</summary>
        public DroneMotor motorFrontLeft, motorFrontRight, motorRearLeft, motorRearRight;

        /// <summary>PID stabilizers for each control value.</summary>
        [HideInInspector] 
        public BasePidController pidThrottle, pidPitch, pidRoll, pidYaw; 
        
        
        private void Awake()
        {
            inputController = GetComponent<DroneInputsController>();

            pidThrottle = new ValueDerivativePidController(controlSettings.pidThrottle);
            pidPitch = new ValueDerivativePidController(controlSettings.pidPitch);
            pidRoll = new ValueDerivativePidController(controlSettings.pidRoll);
            pidYaw = new ValueDerivativePidController(controlSettings.pidYaw);

            gameObject.TryGetDimensions(out droneSizes);
        }
        
        private void FixedUpdate()
        {
            if (!enabled) return;
            
            // When using stabilization target values are limited to maxLiftSpeed, maxPitchAngle etc.
            // Output force will be limited to forceMultiplier, maxPitchForce etc.
            // This restricts drone vertical speed and tilt angles
            
            // Without stabilization forces are directly input * maxForce
            var dt = Time.fixedDeltaTime;
            float throttleOutput, pitchOutput, rollOutput, yawOutput;
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude))
            {
                throttleOutput = pidThrottle.Calc(
                    inputController.throttle * controlSettings.maxLiftSpeed,
                    rigidBody.linearVelocity.y, 
                    dt);
            }
            else
            {
                throttleOutput = inputController.throttle * controlSettings.maxLiftForce;
            }    
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabPitchRoll))
            { 
                var rot = transform.WrapEulerRotation180();
                pitchOutput = -pidPitch.Calc(inputController.pitch * controlSettings.maxPitchAngle, rot.x, dt);
                rollOutput = -pidRoll.Calc(inputController.roll * controlSettings.maxRollAngle, rot.z, dt);
            }
            else
            {  
                // must be inverted to match stabilized version (don't fix what is working)
                pitchOutput = -inputController.pitch * controlSettings.maxRollForce;
                rollOutput  = -inputController.roll * controlSettings.maxRollForce;
            }
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabYaw))
            {
                var yawSpeed = rigidBody.YawVelocity();
                yawOutput = pidYaw.Calc(inputController.yaw * controlSettings.maxYawSpeed, yawSpeed, dt);
            }
            else
            {
                yawOutput = inputController.yaw * controlSettings.maxYawForce;
            }
            
            CalculateMotorsForces(throttleOutput, pitchOutput, yawOutput, rollOutput);
        }

        private void OnDrawGizmos()
        {
            if (!showForceVectors || !enabled) return;
            
            var labelFmt = $"motor_{{0}} ({{1:F2}} | {controlSettings.maxLiftForce:F0})";
            var vectMult = math.cmax(droneSizes) / controlSettings.maxLiftForce;
            var opts = new GizmoOptions(Color.red,
                capSize: math.cmin(droneSizes) / 3,
                vectSize: math.cmax(droneSizes));
            
            VectorDrawer.DrawDirection(motorFrontLeft.transform.position, 
                motorFrontLeft.ForceVector * vectMult,
                string.Format(labelFmt, "FL", motorFrontLeft.ForceVector.magnitude), opts);
            
            VectorDrawer.DrawDirection(motorFrontRight.transform.position, 
                motorFrontRight.ForceVector * vectMult,
                string.Format(labelFmt, "FR", motorFrontRight.ForceVector.magnitude), opts);
            
            VectorDrawer.DrawDirection(motorRearLeft.transform.position, 
                motorRearLeft.ForceVector * vectMult,
                string.Format(labelFmt, "RL", motorRearLeft.ForceVector.magnitude), opts);
           
            VectorDrawer.DrawDirection(motorRearRight.transform.position, 
                motorRearRight.ForceVector * vectMult,
                string.Format(labelFmt, "RR", motorRearRight.ForceVector.magnitude), opts);
        }

        
        private void CalculateMotorsForces(float throttleOutput, float pitchOutput, float yawOutput, float rollOutput)
        {
            motorFrontLeft.liftForce  = throttleOutput + pitchOutput + rollOutput + yawOutput;
            motorFrontRight.liftForce = throttleOutput + pitchOutput - rollOutput - yawOutput;
            motorRearLeft.liftForce   = throttleOutput - pitchOutput + rollOutput - yawOutput;
            motorRearRight.liftForce  = throttleOutput - pitchOutput - rollOutput + yawOutput;

            if (clampNegativeForce)
            {
                motorFrontLeft.liftForce = MathExtensions.ClampPositive(motorFrontLeft.liftForce);
                motorFrontRight.liftForce = MathExtensions.ClampPositive(motorFrontRight.liftForce);
                motorRearLeft.liftForce = MathExtensions.ClampPositive(motorRearLeft.liftForce);
                motorRearRight.liftForce = MathExtensions.ClampPositive(motorRearRight.liftForce);
            }
            
            ApplyMotorForce(motorFrontLeft);
            ApplyMotorForce(motorFrontRight);
            ApplyMotorForce(motorRearLeft);
            ApplyMotorForce(motorRearRight);

            var torqueVector = (motorFrontLeft.ForceVector 
                            - motorFrontRight.ForceVector
                            - motorRearLeft.ForceVector
                            + motorRearRight.ForceVector) * controlSettings.torqueMultiplier;
            rigidBody.AddTorque(torqueVector, ForceMode.Impulse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyMotorForce(DroneMotor motor)
        {
            // ForceMode.Force so it is more stable
            rigidBody.AddForceAtPosition(motor.ForceVector, motor.transform.position, ForceMode.Force);
        }
        
        public override IEnumerable<DroneMotor> GetAllMotors()
        {
            yield return motorFrontLeft;
            yield return motorFrontRight;
            yield return motorRearLeft;
            yield return motorRearRight;
        }
    }
}