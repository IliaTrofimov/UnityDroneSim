using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Drone.Motors;
using Drone.Stability;
using UnityEngine;
using Utils;


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
        private Vector3               droneSizes;
        private DroneInputsController inputController;

        [Header("Motors")] 
        public DroneMotor motorFrontLeft;
        public DroneMotor motorFrontRight;
        public DroneMotor motorRearLeft;
        public DroneMotor motorRearRight;


        protected override void Awake()
        {
            base.Awake();
            inputController = GetComponent<DroneInputsController>();
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
                    dt
                );
            }
            else
            {
                throttleOutput = inputController.throttle * controlSettings.maxLiftForce;
                pidThrottle.Reset();
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
                rollOutput = -inputController.roll * controlSettings.maxRollForce;
                pidPitch.Reset();
                pidRoll.Reset();
            }

            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabYaw))
            {
                var yawSpeed = rigidBody.YawVelocity();
                yawOutput = pidYaw.Calc(inputController.yaw * controlSettings.maxYawSpeed, yawSpeed, dt);
            }
            else
            {
                yawOutput = inputController.yaw * controlSettings.maxYawForce;
                pidYaw.Reset();
            }

            CalculateMotorsForces(throttleOutput, pitchOutput, yawOutput, rollOutput);
        }

        private void CalculateMotorsForces(float throttleOutput, float pitchOutput, float yawOutput, float rollOutput)
        {
            motorFrontLeft.liftForce = throttleOutput + pitchOutput + rollOutput + yawOutput;
            motorFrontRight.liftForce = throttleOutput + pitchOutput - rollOutput - yawOutput;
            motorRearLeft.liftForce = throttleOutput - pitchOutput + rollOutput - yawOutput;
            motorRearRight.liftForce = throttleOutput - pitchOutput - rollOutput + yawOutput;

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