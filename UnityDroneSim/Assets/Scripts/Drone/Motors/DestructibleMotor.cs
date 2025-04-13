using Unity.Mathematics;
using UnityEngine;


namespace Drone.Motors
{
    /// <summary>
    /// Motor that have propeller with collider and can be destroyed after heavy impact.
    /// </summary>
    /// <remarks>
    /// This script manages only motor's own colliders and visual effects.
    /// All collision logic is executed in <see cref="DroneStateManager"/> script or in any component attached to drone Rigidbody.
    /// </remarks>
    public class DestructibleMotor : DroneMotor
    {
        [Header("Collisions")]
        [Tooltip("Collider that will be active while propeller speed is slow.")]
        public Collider dynamicRotorCollider;

        [Tooltip("Collider that will be active while propeller speed is fast.")]
        public Collider staticRotorCollider;

        [Range(0f, 50f)]
        [Tooltip("Speed (degrees per second) to enable static collider.")]
        public float staticColliderSpeed;


        private bool _isDestroyed;
        private TrailRenderer _trailRenderer;

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
            if (_isDestroyed) return;

            _isDestroyed = true;
            dynamicRotorCollider.enabled = true;
            staticRotorCollider.enabled = false;
        }

        /// <summary>Repairs broken motor.</summary>
        public void OnMotorRepaired()
        {
            if (!_isDestroyed) return;

            _isDestroyed = false;
            dynamicRotorCollider.enabled = false;
            staticRotorCollider.enabled = true;
            Destroy(_trailRenderer);
        }
    }
}