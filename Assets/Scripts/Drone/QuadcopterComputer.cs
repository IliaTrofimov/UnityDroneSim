using System.Runtime.CompilerServices;
using Drone.Propulsion;
using Drone.Stability;
using Exceptions;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace Drone
{
    /// <summary>Quadcopter drone flight computer. Manages all motors power.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DroneInputsController))]
    public class QuadcopterComputer : DroneComputerBase
    {
        /// <summary>Settings toggles.</summary>
        public bool showForceVectors, clampNegativeForce, balanceCenterOfMass;
        
        /// <summary>Internal parameters for</summary>
        public DroneControlParams controlParams = DroneControlParams.Default;

        /// <summary>Quadcopter motors.</summary>
        public DroneMotor motorFrontLeft, motorFrontRight, motorRearLeft, motorRearRight;
        
        /// <summary>PID stabilizers for each control value.</summary>
        // Can use DebugPidController to show live PID values stats (POOR PERFORMANCE)
        public PidController pidThrottle, pidPitch, pidRoll, pidYaw; 
       
        /// <summary>Resulting corrected control values.</summary>
        public float pitchCorrection, yawCorrection, rollCorrection, throttleCorrection;
       
        public Vector3 torqueVector;
        private Vector3 droneBounds;

        private void OnEnable()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(motorFrontLeft, nameof(motorFrontLeft));
            ExceptionHelper.ThrowIfComponentIsMissing(motorFrontRight, nameof(motorFrontRight));
            ExceptionHelper.ThrowIfComponentIsMissing(motorRearLeft, nameof(motorRearLeft));
            ExceptionHelper.ThrowIfComponentIsMissing(motorRearRight, nameof(motorRearRight));
            
            if (balanceCenterOfMass && rigidBody != null) ResetCenterOfMass();
            
            gameObject.TryGetDimensions(out droneBounds);
        }

        private void OnDrawGizmos()
        {
            if (!showForceVectors) return;

            var options = new GizmoOptions(Color.red, 
                capSize: math.cmin(droneBounds) / 3,
                vectSize: math.cmax(droneBounds));

            const string labelFmt = "motor_{0} ({1:F2})";
            
            VectorDrawer.DrawDirection(motorFrontLeft.transform.position, 
                motorFrontLeft.ForceVector,
                string.Format(labelFmt, "FL", motorFrontLeft.ForceVector.magnitude), options);
            
            VectorDrawer.DrawDirection(motorFrontRight.transform.position, 
                motorFrontRight.ForceVector,
                string.Format(labelFmt, "FR", motorFrontRight.ForceVector.magnitude), options);
            
            VectorDrawer.DrawDirection(motorRearLeft.transform.position, 
                motorRearLeft.ForceVector,
                string.Format(labelFmt, "RL", motorRearLeft.ForceVector.magnitude), options);
           
            VectorDrawer.DrawDirection(motorRearRight.transform.position, 
                motorRearRight.ForceVector,
                string.Format(labelFmt, "RR", motorRearRight.ForceVector.magnitude), options);
        }

        private void OnValidate()
        {
            // keep PID outputs limited to maxForce values
            pidThrottle.SetClamping(controlParams.maxLiftForce);
            pidPitch.SetClamping(controlParams.maxPitchForce);
            pidYaw.SetClamping(controlParams.maxYawForce);
            pidRoll.SetClamping(controlParams.maxRollForce);
        }

        private void FixedUpdate()
        {
            // When using stabilization target values are limited to maxLiftSpeed, maxPitchAngle etc.
            // Output force will be limited to forceMultiplier, maxPitchForce etc.
            // This restricts drone vertical speed and tilt angles
            
            // Without stabilization forces are directly input * maxForce
            
            var dt = Time.fixedDeltaTime;

            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude))
            {
                throttleCorrection = pidThrottle.Calc(
                    inputController.throttle * controlParams.maxLiftSpeed,
                    rigidBody.linearVelocity.y, 
                    dt);
            }
            else
            {
                throttleCorrection = inputController.throttle * controlParams.maxLiftForce;
            }
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabPitchRoll))
            { 
                var rot = transform.WrapEulerRotation180();
                pitchCorrection = -pidPitch.Calc(inputController.pitch * controlParams.maxPitchAngle, rot.x, dt);
                rollCorrection = -pidRoll.Calc(inputController.roll * controlParams.maxRollAngle, rot.z, dt);
            }
            else
            {  
                // must be inverted to match stabilized version (don't fix what is working)
                pitchCorrection = -inputController.pitch * controlParams.maxRollForce;
                rollCorrection  = -inputController.roll * controlParams.maxRollForce;
            }

            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabYaw))
            {
                var yawSpeed = rigidBody.YawVelocity();
                yawCorrection = pidYaw.Calc(inputController.yaw * controlParams.maxYawSpeed, yawSpeed, dt);
            }
            else
            {
                yawCorrection = inputController.yaw * controlParams.maxYawForce;
            }
            
            UpdateMotorsManual();
        }
        
        
        private void UpdateMotorsManual()
        {
            motorFrontLeft.liftForce  = throttleCorrection + pitchCorrection + rollCorrection + yawCorrection;
            motorFrontRight.liftForce = throttleCorrection + pitchCorrection - rollCorrection - yawCorrection;
            motorRearLeft.liftForce   = throttleCorrection - pitchCorrection + rollCorrection - yawCorrection;
            motorRearRight.liftForce  = throttleCorrection - pitchCorrection - rollCorrection + yawCorrection;

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

            torqueVector = (motorFrontLeft.ForceVector 
                            - motorFrontRight.ForceVector
                            - motorRearLeft.ForceVector
                            + motorRearRight.ForceVector) * controlParams.torqueMultiplier;
            rigidBody.AddTorque(torqueVector, ForceMode.Impulse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyMotorForce(DroneMotor motor)
        {
            // ForceMode.Force so it is more stable
            rigidBody.AddForceAtPosition(motor.ForceVector, motor.transform.position, ForceMode.Force);
        }

        private void ResetCenterOfMass()
        {
            if (motorFrontLeft == null || motorFrontRight == null || motorRearLeft == null || motorRearRight == null)
                return;
            
            var cm = (motorFrontLeft.transform.position + 
                      motorFrontRight.transform.position +
                      motorRearLeft.transform.position + 
                      motorRearRight.transform.position) / 4f;

            cm = rigidBody.transform.InverseTransformPoint(cm);

            if (rigidBody.centerOfMass != cm)
            {
                rigidBody.centerOfMass = cm;
                Debug.LogFormat("Drone '{0}' center of mass set as midpoint of its motors: {1:F3} [local], {2:F3} [world]",
                                gameObject.name, rigidBody.centerOfMass, cm);   
            }
        }


        public override DroneMotor[] GetAllMotors()
        {
            return new []
            {
                motorFrontLeft, motorFrontRight, motorRearLeft, motorRearRight
            };
        }
    }
}