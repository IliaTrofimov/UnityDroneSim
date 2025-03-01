using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using UtilsDebug;


namespace Drone.Propulsion
{
    public class DestructibleMotor : DroneMotor
    {
        public override Vector3 ForceVector => enabled ? transform.up * liftForce : Vector3.zero;
        
		[Header("Collisions")]
        public Collider dynamicRotorCollider;
        public Collider staticRotorCollider;
        [Range(0f, 50f)] public float staticColliderSpeed;
        
        [Header("Destruction trail effect")]
        public Material trailMaterial;
        public Color trailStartColor = Color.white;
        public Color trailEndColor = Color.gray;
        [Range(0f, 60f)] public float trailTime = 5f;
        
        private bool isDestroyed;
        private TrailRenderer trailRenderer;
        private float trailWidth = 0.1f;
        
        private void Awake()
        { 
            gameObject.TryGetDimensions(out Vector3 dimensions);
            trailWidth = math.cmin(dimensions) * 0.9f;
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

        public void OnMotorCollided()
        {
            if (isDestroyed) 
                return;
            
            isDestroyed = true;
            dynamicRotorCollider.enabled = true;
            staticRotorCollider.enabled = false;
            
            if (trailTime <= 1e-3 || trailWidth <= 1e-4)
                return;
            
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.material = trailMaterial;
            trailRenderer.startWidth = trailWidth * 0.9f;
            trailRenderer.endWidth = trailWidth;
            trailRenderer.startColor = trailStartColor;
            trailRenderer.endColor = trailEndColor;
            trailRenderer.time = trailTime;
            trailRenderer.shadowCastingMode = ShadowCastingMode.Off;
            trailRenderer.generateLightingData = false;
            trailRenderer.autodestruct = false;
            trailRenderer.numCapVertices = 5;
            trailRenderer.numCornerVertices = 5;
        }
    }
    
}