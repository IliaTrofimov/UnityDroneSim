using System;
using InspectorTools;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace Drone.Motors
{
    /// <summary>Script that calculates propeller speed and animates it.</summary>
    /// <remarks>
    /// Use <c>ApplyForce</c> and <c>ApplyForceClamp</c> to calculate and apply force and torque to some Rigidbody.
    /// </remarks>
    public class DroneMotor : MonoBehaviour
    {
        /// <summary>Get current force vector. Shortcut for <c>transform.up * totalForce</c>.</summary>
        public virtual Vector3 ForceVector => enabled ? transform.up * liftForce : Vector3.zero;

        public float PropellerAngularSpeed => _propellerAngleDelta / Time.deltaTime;
        public float PropellerLinearSpeed  => PropellerAngularSpeed * PropellerRadius;
        public float PropellerRadius       { get; private set; }


        [Header("Force values")]
        [ReadOnlyField]
        [Tooltip("Total lift force to be applied by this motor.")]
        public float liftForce;
        
        [Range(0f, 5f)]
        [Tooltip("A factor to be applied to torque produced by motor.")]
        public float torqueFactor = 1f;

        [Range(-1, 1)]
        [Tooltip("A factor to be applied to the pitch correction. Front motors must have positive value.")]
        public int pitchFactor;
        
        [Range(-1, 1)]
        [Tooltip(
            "A factor to be applied to the yaw correction. Front-left and rear-right motors must have positive value."
        )]
        public int yawFactor;

        [Range(-1, 1)]
        [Tooltip("A factor to be applied to the roll correction. Left motors must have positive value.")]
        public int rollFactor;

        [Header("Animations")]
        [Range(0f, 1000f)]
        [Tooltip("Propeller rotation speed multiplier. Set 0 to disable all animations.")]
        public float animationSpeed = 900f;
        
        [Range(0f, 10f)]
        [Tooltip("Minimal force required to rotate propeller.")]
        public float forceThreshold = 0.1f;
        
        [Tooltip("The propeller object. Animation will be done here.")]
        public GameObject propeller;

        private float _propellerAngleDelta;
        private float _propellerSpeedFactor;
        
        private void Start() => UpdatePropellerSpeedFactor();

        private void OnValidate()
        {
            UpdatePropellerSpeedFactor();
        }

        private void UpdatePropellerSpeedFactor()
        {
            PropellerRadius = 1f;
            if (propeller != null && propeller.TryGetDimensions(out float diameter))
                PropellerRadius = diameter / 2;

            // Force ~ 0.5*k*w^2*R^4 => w ~ R^(-2)*(2Force/k)^0.5 = sqrt(Force)*C
            // C = R^(-2) * animationSpeed
            _propellerSpeedFactor = math.pow(PropellerRadius, -2) * math.sqrt(animationSpeed);
        }

        protected virtual void Update()
        {
            if (!enabled || !propeller || animationSpeed <= 1e-5f) return;

            var forceSign = math.sign(liftForce) * yawFactor;
            
            if (math.abs(liftForce) >= forceThreshold)
            {
                _propellerAngleDelta = MathExtensions.AbsSqrt(liftForce) * 
                                       _propellerSpeedFactor * 
                                       Time.deltaTime * 
                                       forceSign;
            }
            else
            {
                // make propeller stop gently
                _propellerAngleDelta = Mathf.LerpUnclamped(_propellerAngleDelta, 0, Time.deltaTime);
            }
            
            propeller.transform.Rotate(0, _propellerAngleDelta, 0);
        }

        private void OnDrawGizmosSelected()
        {
            VectorDrawer.DrawPointCube(transform.position, $"{PropellerAngularSpeed/360f:F1} rot/s",
                new GizmoOptions()
                {
                    CapSize = PropellerRadius / 5,
                    Color = Color.red,
                    LabelColor = Color.red,
                    LabelOutline = true
                });
        }


        /// <summary>Apply lift force and create torque for given Rigidbody.</summary>
        /// <returns>Calculated <c>liftForce</c> value.</returns>
        public float ApplyForce(Rigidbody rigidBody, float throttle, float pitch, float yaw, float roll)
        {
            liftForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
            rigidBody.AddForceAtPosition(ForceVector, transform.position, ForceMode.Force);
            rigidBody.AddTorque(ForceVector * torqueFactor, ForceMode.Force);
            return liftForce;
        }

        /// <summary>Apply lift force and create torque for given Rigidbody. Clamp force if less than 0.</summary>
        /// <returns>Calculated <c>liftForce</c> value.</returns>
        public float ApplyForceClamp(Rigidbody rigidBody, float throttle, float pitch, float yaw, float roll)
        {
            liftForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
            MathExtensions.ClampPositive(ref liftForce);
            rigidBody.AddForceAtPosition(ForceVector, transform.position, ForceMode.Force);
            rigidBody.AddTorque(ForceVector * torqueFactor, ForceMode.Force);
            return liftForce;
        }
    }
}