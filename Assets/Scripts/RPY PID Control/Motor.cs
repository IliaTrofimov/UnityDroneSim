using InspectorTools;
using UnityEngine;

namespace RPY_PID_Control
{
	/// <summary>
	/// Basic drone motor class.
	/// Motor can calculate lift force and torque and optionally apply them to Rigidbody via its UpdateForce methods.
	/// </summary>
	public class Motor : MonoBehaviour 
	{
		/// <summary>Total lift force to be applied by this motor.</summary>
		[Header("Force values")]
		[ReadOnlyField] public float totalForce;
		
		/// <summary>Get current force vector. Equals to <c>transform.up * totalForce</c>.</summary>
		public Vector3 ForceVector => transform.up * totalForce;
		
		/// <summary>A factor to be applied to torque produced by motor.</summary>
		[Range(0f, 5f)] public float torqueFactor = 1f;
		
		/// <summary>A factor to be applied to the pitch correction.</summary>
		/// <remarks>Front motors must have positive value.</remarks>
		[Range(-1, 1)] public int pitchFactor;
		
		/// <summary>A factor to be applied to the yaw correction.</summary>
		/// <remarks>Front-left and rear-right motors must have positive value.</remarks>
		[Range(-1, 1)] public int yawFactor;
		
		/// <summary>A factor to be applied to the roll correction.</summary>
		/// <remarks>Left motors must have positive value.</remarks>
		[Range(-1, 1)] public int rollFactor;


		[Header("Physics")] 
		public bool createCollider;
		[Range(0f, 100f)] public float staticColliderSpeed = 100f;
		
		
		/// <summary>Turns on/off propellers animations.</summary>
		[Header("Animations")]
		public bool animatePropellers = true;
		
		/// <summary>Propeller rotation speed multiplier.</summary>
		[Range(0f, 100f)] public float animationSpeed = 1f;
		
		/// <summary>The propeller object. Animation will be done here.</summary>
		public GameObject propeller;
		private float propellerSpeed;
		
		
		private void FixedUpdate()
		{
			if (!animatePropellers) return;

			propellerSpeed = Mathf.Lerp(propellerSpeed, animationSpeed * totalForce * 1000f, Time.deltaTime);
			propeller.transform.Rotate(0, propellerSpeed, 0);
		}

		/// <summary>Calculate current motor's force value.</summary>
		/// <returns>Force vector module.</returns>
		public float UpdateForce(float throttle, float pitch, float yaw, float roll)
		{
			totalForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
			return totalForce;
		}
		
		/// <summary>Calculate current motor's force and apply it to given Rigidbody.</summary>
		/// <returns>Force vector module.</returns>
		public float UpdateForce(Rigidbody rigidBody, float throttle, float pitch, float yaw, float roll)
		{
			totalForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
			rigidBody.AddForceAtPosition(transform.up * totalForce, transform.position, ForceMode.Impulse);
			return totalForce;
		}
		
		/// <summary>Calculate current motor's force and torque. Apply them to given Rigidbody.</summary>
		/// <returns>Force vector module.</returns>
		public float UpdateForceAndTorque(Rigidbody rigidBody, float throttle, float pitch, float yaw, float roll)
		{
			totalForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
			var force = transform.up * totalForce;
			rigidBody.AddForceAtPosition(force, transform.position, ForceMode.Impulse);
			
			var cm = rigidBody.centerOfMass;
			rigidBody.AddTorque(torqueFactor * yawFactor * force, ForceMode.Impulse);
			rigidBody.centerOfMass = cm;
			
			return totalForce;
		}

		
		/// <summary>Calculate current motor's force value (clamp if less than 0).</summary>
		/// <returns>Force vector module.</returns>
		public float UpdateForceClamp(float throttle, float pitch, float yaw, float roll)
		{
			totalForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
			if (totalForce < 0f)
				totalForce = 0f;
			return totalForce;
		}
		
		/// <summary>Calculate current motor's force and apply it to given Rigidbody.</summary>
		/// <returns>Force vector module.</returns>
		public float UpdateForceClamp(Rigidbody rigidBody, float throttle, float pitch, float yaw, float roll)
		{
			totalForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
			if (totalForce < 0f)
				totalForce = 0f;
			
			var force = transform.up * totalForce;
			rigidBody.AddForceAtPosition(force, transform.position, ForceMode.Impulse);
			
			return totalForce;
		}
		
		/// <summary>Calculate current motor's force and apply it to given Rigidbody.</summary>
		/// <returns>Force vector module.</returns>
		public float UpdateForceAndTorqueClamp(Rigidbody rigidBody, float throttle, float pitch, float yaw, float roll)
		{
			totalForce = throttle + pitchFactor * pitch + rollFactor * roll + yawFactor * yaw;
			if (totalForce < 0f)
				totalForce = 0f;
			
			var force = transform.up * totalForce;
			rigidBody.AddForceAtPosition(force, transform.position, ForceMode.Impulse);
			
			var cm = rigidBody.centerOfMass;
			rigidBody.AddTorque(torqueFactor * yawFactor * force, ForceMode.Impulse);
			rigidBody.centerOfMass = cm;
			
			return totalForce;
		}
	}
}
