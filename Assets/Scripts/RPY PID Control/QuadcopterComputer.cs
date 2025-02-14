using System;
using DebugUtils.VectorDrawer;
using Exceptions;
using PID;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;


namespace RPY_PID_Control
{
    /// <summary>
    /// Quadcopter drone flight computer. Manages motors power.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(DroneInputsController))]
    public class QuadcopterComputer : MonoBehaviour
    {
        private Rigidbody rigidBody;
        private DroneInputsController inputController;
        
        [Header("Debug")]
        public bool showForceVectors;
        public bool balanceCenterOfMass;
        
        [Header("Control settings")]
        [Range(0f, 10f)] public float throttleIncrease = 1f;
        [Range(0f, 10f)] public float throttleLimit = 0.2f;
        [Range(0f, 50f)] public float pitchRollLimit = 0.01f; 
        [Range(0f, 50f)] public float yawLimit = 0.01f;
        public bool clampNegativeForce;
        public bool applyTorque;
        public bool useMotorsScript;

        [Header("Motors")]
        public Motor motorFrontLeft;
        public Motor motorFrontRight;
        public Motor motorRearLeft;
        public Motor motorRearRight;

        [Header("Stabilization")]
        public PidController pidThrottle;
        public PidController pidPitch;
        public PidController pidRoll;
        public PidController pidYaw;
        
        [HideInInspector] public float pitchCorrection;
        [HideInInspector] public float yawCorrection;
        [HideInInspector] public float rollCorrection;
        [HideInInspector] public float throttleCorrection;
        
        private Vector3 droneBounds;
        
        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();    
            inputController = GetComponent<DroneInputsController>();
            
            if (balanceCenterOfMass) ResetCenterOfMass();
        }

        private void OnEnable()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(motorFrontLeft, nameof(motorFrontLeft));
            ExceptionHelper.ThrowIfComponentIsMissing(motorFrontRight, nameof(motorFrontRight));
            ExceptionHelper.ThrowIfComponentIsMissing(motorRearLeft, nameof(motorRearLeft));
            ExceptionHelper.ThrowIfComponentIsMissing(motorRearRight, nameof(motorRearRight));
            
            if (balanceCenterOfMass && rigidBody != null) ResetCenterOfMass();
            
            gameObject.TryGetDimensions(out droneBounds);
        }
        
        private void OnValidate()
        {
            if (rigidBody == null) return;
            if (balanceCenterOfMass) ResetCenterOfMass();
            else rigidBody.ResetCenterOfMass();
        }

        private void OnDrawGizmos()
        {
            if (!showForceVectors) return;

            var options = new GizmoOptions(Color.red, math.cmin(droneBounds) / 2, labelColor: Color.white)
            {
                maxVectorLength = math.cmax(droneBounds)
            };
            
            VectorDrawerLite.DrawDirection(motorFrontLeft.transform.position, 
                motorFrontLeft.ForceVector,
                "motor_FL", options);
            VectorDrawerLite.DrawDirection(motorFrontRight.transform.position, 
                motorFrontRight.ForceVector,
                "motor_FR", options);
            VectorDrawerLite.DrawDirection(motorRearLeft.transform.position, 
                motorRearLeft.ForceVector,
                "motor_RL", options);
            VectorDrawerLite.DrawDirection(motorRearRight.transform.position, 
                motorRearRight.ForceVector,
                "motor_RR", options);
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabRotation))
            {
                var euler = transform.eulerAngles;
                var actualPitch = MathExtensions.ClampAngle(euler.z);
                var actualYaw = MathExtensions.ClampAngle(euler.y);
                var actualRoll = MathExtensions.ClampAngle(euler.x);
                
                pitchCorrection = pidPitch.CalcDeriv(inputController.pitch * pitchRollLimit, actualPitch, dt);
                yawCorrection = pidYaw.CalcDeriv(inputController.yaw * yawLimit, actualYaw, dt);
                rollCorrection = pidRoll.CalcDeriv(inputController.roll * pitchRollLimit, actualRoll, dt);
            }
            else
            {
                pitchCorrection = inputController.pitch * pitchRollLimit;
                yawCorrection = inputController.yaw * yawLimit;
                rollCorrection = inputController.roll * pitchRollLimit;
            }
            
            if (inputController.stabilizerMode.HasFlag(DroneStabilizerMode.StabAltitude))
                throttleCorrection = pidThrottle.CalcDeriv(inputController.throttle * throttleLimit, rigidBody.linearVelocity.y, dt);
            else
                throttleCorrection = inputController.throttle * throttleLimit;

            throttleCorrection *= throttleIncrease;
            
            if (useMotorsScript)
                UpdateMotorsAutomatic();
            else 
                UpdateMotorsManual();
        }
        
        private void UpdateMotorsManual()
        {
            motorFrontLeft.totalForce  = throttleCorrection + pitchCorrection + rollCorrection + yawCorrection;
            motorFrontRight.totalForce = throttleCorrection + pitchCorrection - rollCorrection - yawCorrection;
            motorRearLeft.totalForce   = throttleCorrection - pitchCorrection - rollCorrection + yawCorrection;
            motorRearRight.totalForce  = throttleCorrection - pitchCorrection + rollCorrection - yawCorrection;

            if (clampNegativeForce)
            {
                if (motorFrontLeft.totalForce  < 0f) motorFrontLeft.totalForce = 0f;
                if (motorFrontRight.totalForce < 0f) motorFrontRight.totalForce = 0f;
                if (motorRearLeft.totalForce   < 0f) motorRearLeft.totalForce = 0f;
                if (motorRearRight.totalForce  < 0f) motorRearRight.totalForce = 0f;     
            }
            
            rigidBody.AddForceAtPosition(
                motorFrontLeft.ForceVector,
                motorFrontLeft.transform.position,
                ForceMode.Impulse);
            
            rigidBody.AddForceAtPosition(
                motorFrontRight.ForceVector,
                motorFrontRight.transform.position,
                ForceMode.Impulse);
            
            rigidBody.AddForceAtPosition(
                motorRearLeft.ForceVector,
                motorRearLeft.transform.position,
                ForceMode.Impulse);
            
            rigidBody.AddForceAtPosition(
                motorRearRight.ForceVector,
                motorRearRight.transform.position,
                ForceMode.Impulse);

            if (applyTorque)
            {
                rigidBody.AddTorque(motorFrontLeft.ForceVector
                                    - motorFrontRight.ForceVector
                                    + motorRearLeft.ForceVector
                                    - motorRearLeft.ForceVector, ForceMode.Impulse);
            }
        }
     
        private void UpdateMotorsAutomatic()
        {
            if (applyTorque && clampNegativeForce)
            {
                motorFrontLeft.UpdateForceAndTorqueClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorFrontRight.UpdateForceAndTorqueClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearLeft.UpdateForceAndTorqueClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearRight.UpdateForceAndTorqueClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
            }
            else if (applyTorque)
            {
                motorFrontLeft.UpdateForceAndTorque(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorFrontRight.UpdateForceAndTorque(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearLeft.UpdateForceAndTorque(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearRight.UpdateForceAndTorque(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
            }
            else if (clampNegativeForce)
            {
                motorFrontLeft.UpdateForceClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorFrontRight.UpdateForceClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearLeft.UpdateForceClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearRight.UpdateForceClamp(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
            }
            else
            {
                motorFrontLeft.UpdateForce(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorFrontRight.UpdateForce(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearLeft.UpdateForce(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
                motorRearRight.UpdateForce(rigidBody, throttleCorrection, pitchCorrection, yawCorrection, rollCorrection);
            }
        }
        
        private void ResetCenterOfMass()
        {
            if (motorFrontLeft == null || motorFrontRight == null || motorRearLeft == null || motorRearRight == null)
                return;
            
            var cm = (motorFrontLeft.transform.position + 
                      motorFrontRight.transform.position +
                      motorRearLeft.transform.position + 
                      motorRearRight.transform.position) / 4f;
            rigidBody.centerOfMass = rigidBody.transform.InverseTransformPoint(cm);
            
            Debug.LogFormat("Drone '{0}' center of mass set as midpoint of its motors: {1:F3} [local], {2:F3} [world]",
                gameObject.name, rigidBody.centerOfMass, cm);
        }
    }
}