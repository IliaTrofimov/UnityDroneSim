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
    /// All collision logic is executed in <see cref="DroneDestruction"/> script or in any component attached to drone Rigidbody.
    /// </remarks>
    public class DestructibleMotor : DroneMotor
    {
        /// <summary>Collider that will be active while propeller speed is slow.</summary>
		[Header("Collisions")]
        public Collider dynamicRotorCollider;
       
        /// <summary>Collider that will be active while propeller speed is fast.</summary>
        public Collider staticRotorCollider;
        
        /// <summary>Speed (degrees per second) to enable static collider.</summary>
        [Range(0f, 50f)] public float staticColliderSpeed;
        
        
        private bool isDestroyed;
        private float trailWidth = 0.1f;
        private TrailRenderer trailRenderer;
        
        
        private void Awake()
        { 
            gameObject.TryGetDimensions(out Vector3 dimensions);
            trailWidth = math.cmin(dimensions) * 0.8f;
        }

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