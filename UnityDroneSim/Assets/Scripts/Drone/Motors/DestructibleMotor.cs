using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;


namespace Drone.Motors
{
    /// <summary>
    /// Motor that have propeller with collider and can be destroyed after heavy impact.
    /// </summary>
    /// <remarks>
    /// This script manages only motor's own colliders and visual effects.
    /// All collision logic is executed in <see cref="DroneState"/> script or in any component attached to drone Rigidbody.
    /// </remarks>
    public class DestructibleMotor : DroneMotor
    {
		[Header("Collisions")]
        [Tooltip("Collider that will be active while propeller speed is slow.")]
        public Collider dynamicRotorCollider;
       
        [Tooltip("Collider that will be active while propeller speed is fast.")]
        public Collider staticRotorCollider;
        
        [Tooltip("Speed (degrees per second) to enable static collider.")]
        [Range(0f, 50f)] public float staticColliderSpeed;
        
        
        private bool isDestroyed;
        private TrailRenderer trailRenderer;

        private void FixedUpdate()
        {
            // If propeller is spinning fast - use static (non-rotating) collider
            if (math.abs(propellerAngleDelta) > staticColliderSpeed) 
            {
                dynamicRotorCollider.enabled = false;
                staticRotorCollider.enabled = true;
            }
            else
            {
                dynamicRotorCollider.enabled = true;
                staticRotorCollider.enabled = false;
            }
        }

        
        /// <summary>Makes motor broken.</summary>
        public void OnMotorBroken()
        {
            if (isDestroyed) return;
            
            isDestroyed = true;
            dynamicRotorCollider.enabled = true;
            staticRotorCollider.enabled = false;
        }

        /// <summary>Repairs broken motor.</summary>
        public void OnMotorRepaired()
        {
            if (!isDestroyed) return;

            isDestroyed = false;
            dynamicRotorCollider.enabled = false;
            staticRotorCollider.enabled = true;
            Destroy(trailRenderer);
        }
    }
    
}