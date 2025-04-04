using System;
using System.Collections.Generic;
using Drone.Motors;
using InspectorTools;
using UnityEngine;

namespace Drone
{
    /// <summary>Script that destroys/repairs drone's motors after them colliding with obstacles.</summary>
    /// <remarks>
    /// Works with simple <see cref="DroneMotor"/> and special <see cref="DestructibleMotor"/> scripts.
    /// Although <see cref="DestructibleMotor"/> provides collider for its propeller and special FX after breaking.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(DroneComputerBase))]
    public class DroneDestruction : MonoBehaviour
    {
        [SerializeField, ReadOnlyField] private int destroyedMotorsCount;
        [SerializeField, ReadOnlyField] private bool landed;
        private DroneComputerBase droneComputer;
        private readonly Dictionary<int, MotorDestructionInfo> motorsLookup = new(4);
        
        public bool AllMotorsDestroyed => destroyedMotorsCount == motorsLookup.Count; 
        public bool AnyMotorsDestroyed => destroyedMotorsCount > 0;
        public bool Landed => landed;

        
        /// <summary>Minimal </summary>
        [Header("Physics")] 
        [Range(0.1f, 1f)] public float landingDotProduct = 0.55f;

        /// <summary>Maximal collision speed (m/s) that drone hull can survive.</summary>
        [Range(0f, 20f)] public float hullBreakSpeed = 7f;
        
        /// <summary>Maximal collision speed (m/s) that motor can survive.</summary>
        [Range(0f, 20f)] public float motorBreakSpeed = 5f;

        /// <summary>Add rigidbodies to destroyed motors.</summary>
        public bool enableBrokenMotorsPhysics = true;
        
        /// <summary>Each broken motor will have a Rigidbody attached with such mass (kg).</summary>
        [Range(0f, 5f)] public float brokenMotorMass = 0.1f;
        
        /// <summary>Each broken motor will have a Rigidbody attached with such interpolation mode.</summary>
        public RigidbodyInterpolation brokenMotorInterpolation = RigidbodyInterpolation.None;
        
        /// <summary>Each broken motor will have a Rigidbody attached with such collision detection mode.</summary>
        public CollisionDetectionMode brokenMotorCollisions = CollisionDetectionMode.Continuous;
        
        
        /// <summary>Broken motor will leave this trail after detaching from drone.</summary>
        [Header("Destruction FX")]
        public bool enableEffects = true;
        public GameObject motorDestructionPrefab;
        public GameObject hullDestructionPrefab;
    
        
        
        private void Awake()
        { 
            droneComputer = GetComponent<QuadcopterComputer>();
        }
       
        private void OnEnable()
        {
            foreach (var motor in droneComputer.GetAllMotors())
                motorsLookup[motor.gameObject.GetInstanceID()] = new MotorDestructionInfo(motor);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!enabled || AllMotorsDestroyed || collision.contactCount == 0) 
                return;
            
            var contact = collision.GetContact(0);
            var contactSpeed = collision.relativeVelocity.magnitude;
            
            if (CheckLanding(contactSpeed, contact.normal)) 
                return;
            
            CheckDestruction(contactSpeed, contact.thisCollider.gameObject, contact.otherCollider.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!Landed) return;

            landed = false;
            Debug.LogFormat("Take-off [{0}]: drone.vel={1:F2} m/s", gameObject.name, collision.relativeVelocity.magnitude);
        }

        private bool CheckLanding(float speed, Vector3 contactNormal)
        {
            var dot = 0f;
            if (speed > hullBreakSpeed || (dot = Vector3.Dot(contactNormal, Vector3.up)) < landingDotProduct)
            {
                landed = false;
                return false;
            }
            if (!landed) // prevent log spamming
            {
                Debug.LogFormat("Landed [{0}]: drone.vel={1:F2} m/s, normal={2:F2}, dot={3:F2}/{4:F2}",
                    gameObject.name, speed, contactNormal, dot, landingDotProduct);
                landed = true;    
            }
            return true;
        }
        
        private void CheckDestruction(float contactSpeed, GameObject thisCollider, GameObject otherCollider)
        {
            var colliderParent = thisCollider.transform.parent?.gameObject;
            
            if (colliderParent && motorsLookup.TryGetValue(colliderParent.gameObject.GetInstanceID(), out var motorInfo))
            {
                if (motorInfo.Motor.PropellerLinearSpeed + contactSpeed < motorBreakSpeed) return;

                Debug.LogFormat("Collision (Propeller) [{0}.{1}] -> [{2}]: drone.vel={3:F2} m/s, propeller.vel={4:F2} m/s",
                    colliderParent.name, thisCollider.name, otherCollider.name, contactSpeed, motorInfo.Motor.PropellerLinearSpeed + contactSpeed);

                OnMotorCollided(motorInfo);
            }
            else if (motorsLookup.TryGetValue(thisCollider.GetInstanceID(), out motorInfo))
            {
                if (contactSpeed < motorBreakSpeed) return;

                Debug.LogFormat("Collision (Motor) [{0}] -> [{1}]: vel={2:F2} m/s",
                    thisCollider.name, otherCollider.name, contactSpeed);

                OnMotorCollided(motorInfo);
            }   
            else
            {
                if (contactSpeed < hullBreakSpeed) return;
                
                Debug.LogFormat("Collision (Hull) [{0}] -> [{1}]: vel={2:F2} m/s",
                    thisCollider.name, otherCollider.name, contactSpeed);

                BreakAllMotors();
            }
        }
        
        private void OnMotorCollided(MotorDestructionInfo motorInfo, bool addEffects = true)
        {
            if (motorInfo.IsDestroyed) return;

            destroyedMotorsCount++;

            motorInfo.Motor.name = "[X] " + motorInfo.Motor.name;
            motorInfo.Motor.enabled = false;
            motorInfo.IsDestroyed = true;
            
            if (!enableBrokenMotorsPhysics) return;
            
            motorInfo.Motor.transform.parent = null;
            motorInfo.AttachedRigidbody = motorInfo.Motor.gameObject.AddComponent<Rigidbody>();
            if (motorInfo.AttachedRigidbody is not null)
            {
                motorInfo.AttachedRigidbody.mass = brokenMotorMass;
                motorInfo.AttachedRigidbody.interpolation = brokenMotorInterpolation;
                motorInfo.AttachedRigidbody.collisionDetectionMode = brokenMotorCollisions;
                motorInfo.AttachedRigidbody.linearDamping = 0.2f;
                motorInfo.AttachedRigidbody.angularDamping = 0.1f;
                motorInfo.AttachedRigidbody.AddRelativeForce(Vector3.up * motorInfo.Motor.liftForce / 5, ForceMode.Force);
                motorInfo.AttachedRigidbody.AddRelativeTorque(Vector3.up * motorInfo.Motor.liftForce / 5, ForceMode.VelocityChange);
            }

            if (enableEffects && addEffects)
            {
                if (motorDestructionPrefab)
                    AddMotorDestructionFx(motorInfo.Motor.transform);
                if (AllMotorsDestroyed)
                    AddHullDestructionFx();
            }
            
            if (motorInfo.Motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorBroken();
        }
        
        private void RepairMotor(MotorDestructionInfo motorInfo)
        {
            if (!motorInfo.IsDestroyed) return;

            destroyedMotorsCount--;
            motorInfo.Motor.transform.SetParent(motorInfo.InitialParent);
            motorInfo.Motor.transform.localPosition = motorInfo.InitialLocalPosition;
            motorInfo.Motor.transform.localScale = motorInfo.InitialLocalScale;
            motorInfo.Motor.transform.localRotation = motorInfo.InitialLocalRotation;
            motorInfo.Motor.name = motorInfo.Motor.name.Replace("[X] ", "");
            motorInfo.Motor.enabled = true;
            motorInfo.IsDestroyed = false;
            
            if (motorInfo.AttachedRigidbody is not null)
                Destroy(motorInfo.AttachedRigidbody);
           
            if (motorInfo.Motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorRepaired();
            
            Debug.LogFormat("Motor [{0}] was repaired and reattached to drone [{1}]", motorInfo.Motor.name, gameObject.name);
        }

        [ContextMenu("Repair All Motors")]
        public void RepairAllMotors()
        {
            if (!AnyMotorsDestroyed) return;
           
            foreach (var motorInfo in motorsLookup.Values)
                RepairMotor(motorInfo);
        }
        
        [ContextMenu("Break All Motors")]
        public void BreakAllMotors()
        {
            foreach (var motorInfo in motorsLookup.Values)
                OnMotorCollided(motorInfo, addEffects: false);

            if (enableEffects) 
                AddHullDestructionFx();
        }

        private void AddHullDestructionFx()
        {
            if (hullDestructionPrefab)
            {
                var hullFx = Instantiate(hullDestructionPrefab, transform);
                hullFx.name = "[FX] Hull destruction";
            }
            else if (motorDestructionPrefab)
            {
                var hullFx = Instantiate(motorDestructionPrefab, transform);
                hullFx.name = "[FX] Hull destruction";
            }
        }
        
        private void AddMotorDestructionFx(Transform parent)
        {
            var motorFx = Instantiate(motorDestructionPrefab, parent);
            motorFx.name = "[FX] Motor destruction";
        }
    }
}