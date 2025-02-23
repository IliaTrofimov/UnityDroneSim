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
    public class QuadcopterComputer : MonoBehaviour
    {
        public Rigidbody rigidBody;
        private DroneInputsController inputController;
        
        /// <summary>Settings toggles.</summary>
        public bool showForceVectors, clampNegativeForce, balanceCenterOfMass;
        
        /// <summary>Internal parameters for</summary>
        public DroneControlParams controlParams = DroneControlParams.Default;

        /// <summary>Quadcopter motors.</summary>
        public DroneMotor motorFrontLeft, motorFrontRight, motorRearLeft, motorRearRight;
        
        /// <summary>PID stabilizers for each control value.</summary>
        public PidController pidThrottle, pidPitch, pidRoll, pidYaw;
       
        /// <summary>Resulting corrected control values.</summary>
        public float pitchCorrection, yawCorrection, rollCorrection, throttleCorrection;
       
        public Vector3 torqueVector;
        private Vector3 droneBounds;

        private void Start()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(rigidBody, nameof(rigidBody));
            ExceptionHelper.ThrowIfComponentIsMissing(motorFrontLeft, nameof(motorFrontLeft));
            ExceptionHelper.ThrowIfComponentIsMissing(motorFrontRight, nameof(motorFrontRight));
            ExceptionHelper.ThrowIfComponentIsMissing(motorRearLeft, nameof(motorRearLeft));
            ExceptionHelper.ThrowIfComponentIsMissing(motorRearRight, nameof(motorRearRight));
         
            inputController = GetComponent<DroneInputsController>();
            
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

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;

            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude))
            {
                throttleCorrection = pidThrottle.CalcDeriv(
                    inputController.throttle * controlParams.maxLiftSpeed,
                    rigidBody.linearVelocity.y, 
                    dt);
            }
            else
            {
                throttleCorrection = inputController.throttle;
            }
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabPitchRoll))
            { 
                var rot = transform.WrapEulerRotation180();
                pitchCorrection = -pidPitch.CalcDeriv(inputController.pitch * controlParams.maxPitchAngle, rot.x, dt);
                rollCorrection  = -pidRoll.CalcDeriv(inputController.roll * controlParams.maxRollAngle, rot.z, dt);
            }
            else
            {
                pitchCorrection = inputController.pitch;
                rollCorrection  = inputController.roll;
            }

            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabYaw))
            {
                var yawVelocity = rigidBody.AxialAngularVelocity().y;
                yawCorrection = pidYaw.CalcDeriv(inputController.yaw * controlParams.maxYawAngle, yawVelocity, dt);
            }
            else
            {
                yawCorrection = inputController.yaw;
            }

            throttleCorrection *= controlParams.forceMultiplier;
            pitchCorrection    *= controlParams.maxPitchForce;
            yawCorrection      *= controlParams.maxYawForce;
            rollCorrection     *= controlParams.maxRollForce;
            
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
                            + motorRearRight.ForceVector) 
                           * controlParams.torqueMultiplier;
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
    }
}