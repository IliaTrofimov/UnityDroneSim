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
        
        
        /// <summary>Broken motor will leave this trail after detaching from drone.</summary>
        [Header("Destruction FX")]
        public Material trailMaterial;
        
        /// <summary>Broken motor will leave a trail with this colour after detaching from drone.</summary>
        public Color trailColor = Color.white;
        
        /// <summary>Broken motor will leave a trail fot this time (seconds) after detaching from drone.</summary>
        [Range(0f, 60f)] public float trailTime = 5f;
        
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
            
            if (trailTime <= 1e-3 || trailWidth <= 1e-4)
                return;
            
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.material = trailMaterial;
            trailRenderer.startWidth = trailWidth * 0.9f;
            trailRenderer.endWidth = trailWidth;
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = trailColor * 0.8f;
            trailRenderer.time = trailTime;
            trailRenderer.shadowCastingMode = ShadowCastingMode.Off;
            trailRenderer.generateLightingData = false;
            trailRenderer.autodestruct = false;
            trailRenderer.numCapVertices = 5;
            trailRenderer.numCornerVertices = 5;
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