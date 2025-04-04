using InspectorTools;
using Unity.Mathematics;
using UnityEngine;
using Utils;


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
	    public float PropellerAngularSpeed => propellerAngleDelta;
	    public float PropellerLinearSpeed => propellerAngleDelta * propellerAngleDelta;
        public float PropellerRadius { get; private set; }
        
	    
        [Tooltip("A factor to be applied to torque produced by motor.")]
        [Header("Force values"), Range(0f, 5f)]
        public float torqueFactor = 1f;
		
        [Tooltip("A factor to be applied to the pitch correction. Front motors must have positive value.")]
        [Range(-1, 1)] public int pitchFactor;
		
        [Tooltip("A factor to be applied to the yaw correction. Front-left and rear-right motors must have positive value.")]
        [Range(-1, 1)] public int yawFactor;
		
        [Tooltip("A factor to be applied to the roll correction. Left motors must have positive value.")]
        [Range(-1, 1)] public int rollFactor;
		
        [Tooltip("Total lift force to be applied by this motor.")]
        [ReadOnlyField] public float liftForce;
        

        [Header("Animations")]
        [Tooltip("Turns on/off propellers animations.")]
        public bool animatePropellers = true;

        [Tooltip("Propeller rotation speed multiplier.")]
        [Range(0f, 100f)] public float animationSpeed = 0.5f;
        
        [Tooltip("Minimal propeller's rotation speed (degrees per frame) before stopping.")]
        [Range(0f, 10f)] public float idleRotationSpeed = 1f;

        [Tooltip("The propeller object. Animation will be done here.")]
        public GameObject propeller;

        [SerializeField, ReadOnlyField] protected float propellerAngleDelta;
        private float propellerInertia;
        private float propellerSpeedFactor;
        
        private void Start() => UpdatePropellerSpeedFactor();
        
        private void OnValidate()
        {
	        if (!animatePropellers) return;
	        UpdatePropellerSpeedFactor();
        }

        private void UpdatePropellerSpeedFactor()
        {
	        var radius = 0.5f;
	        if (propeller != null &&  propeller.TryGetDimensions(out float diameter))
	        {
		        PropellerRadius = radius = diameter / 2; 
	        }
	        
	        // Force ~ 0.5*k*w^2*R^4 => w ~ R^(-2)*(2Force/k)^0.5 = sqrt(Force)*C
	        // C = R^(-2) * animationSpeed
	        propellerSpeedFactor = math.pow(radius, -2) * math.sqrt(animationSpeed);
        }
        
        protected virtual void Update()
        {
            if (!animatePropellers || !enabled) return;

            var forceSign = math.sign(liftForce) * yawFactor;
            propellerInertia = Mathf.Lerp(propellerInertia, idleRotationSpeed * forceSign, Time.deltaTime);
            
            if (liftForce != 0)	
	            propellerAngleDelta = MathExtensions.AbsSqrt(liftForce) * propellerSpeedFactor * Time.deltaTime * forceSign;
            else
	            propellerAngleDelta = 0;

            propellerAngleDelta += propellerInertia;
            propeller.transform.Rotate(0, propellerAngleDelta, 0);
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