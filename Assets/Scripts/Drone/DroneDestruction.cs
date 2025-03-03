using System.Collections.Generic;
using Drone.Propulsion;
using Unity.VisualScripting;
using UnityEngine;


namespace Drone
{
    /// <summary>Script that destroys/repairs drone's motors after them colliding with obstacles.</summary>
    /// <remarks>
    /// Works with simple <see cref="DroneMotor"/> and special <see cref="DestructibleMotor"/> scripts.
    /// Although <see cref="DestructibleMotor"/> provides collider for its propeller and special FX after breaking.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(QuadcopterComputer))]
    [RequireComponent(typeof(Rigidbody))]
    public class DroneDestruction : MonoBehaviour
    {
        private bool isAnyMotorDestroyed;
        private readonly Dictionary<int, MotorDestructionInfo> motorsLookup = new(4);
        private DroneComputerBase droneComputer;
        
        
        /// <summary>Maximal collision speed that motor can survive.</summary>
        [Range(0f, 20f)] public float breakSpeed = 5f;
        
        /// <summary>Maximal collision impulse that motor can survive.</summary>
        [Range(0f, 20f)] public float breakImpulse = 5f;
        
        /// <summary>Each broken motor will have a Rigidbody attached with such mass (kg).</summary>
        [Range(0f, 5f)] public float brokenMotorMass = 0.1f;
        
        /// <summary>Each broken motor will have a Rigidbody attached with such interpolation mode.</summary>
        public RigidbodyInterpolation brokenMotorInterpolation = RigidbodyInterpolation.None;
        
        /// <summary>Each broken motor will have a Rigidbody attached with such collision detection mode.</summary>
        public CollisionDetectionMode brokenMotorCollisions = CollisionDetectionMode.Continuous;
        
        
        private void Awake()
        { 
            droneComputer = GetComponent<QuadcopterComputer>();
        }
       
        private void OnEnable()
        {
            foreach (var motor in droneComputer.GetAllMotors())
                motorsLookup[motor.gameObject.GetInstanceID()] = new MotorDestructionInfo(motor);
        }
       
        private void OnDisable() => RepairAllMotors();
       
        private void OnCollisionEnter(Collision other)
        {
            if (!enabled) return;
            
            var spd = other.relativeVelocity.magnitude;
            var imp = other.impulse.magnitude;
            if (other.contactCount == 0 || (spd < breakSpeed && imp < breakImpulse)) 
                return;

            var thisCollider = other.GetContact(0).thisCollider;
            var colliderParent = thisCollider.transform.parent?.gameObject;

            if (colliderParent is not null && motorsLookup.TryGetValue(colliderParent.GetInstanceID(), out var motorInfo))
            {
                OnMotorCollided(motorInfo);
                Debug.LogFormat("Collision (Motor) [{0}.{1}] -> [{2}]: vel={3:F2} m/s, imp={4:F2} kg*m/s",
                    colliderParent.name, thisCollider.name, other.gameObject.name, spd, imp);
            }
            else
            {
                Debug.LogFormat("Collision [{0}] -> [{1}]: vel={2:F2} m/s, imp={3:F2} kg*m/s",
                    thisCollider.name, other.gameObject.name, spd, imp);
            }
        }
        
        
        private void OnMotorCollided(MotorDestructionInfo motorInfo)
        {
            if (motorInfo.isDestroyed) return;
            
            motorInfo.attachedRigidbody = motorInfo.motor.AddComponent<Rigidbody>();
            if (motorInfo.attachedRigidbody is not null)
            {
                motorInfo.attachedRigidbody.mass = brokenMotorMass;
                motorInfo.attachedRigidbody.interpolation = brokenMotorInterpolation;
                motorInfo.attachedRigidbody.collisionDetectionMode = brokenMotorCollisions;
                motorInfo.attachedRigidbody.linearDamping = 0.2f;
                motorInfo.attachedRigidbody.angularDamping = 0.1f;
                motorInfo.attachedRigidbody.AddRelativeForce(Vector3.up * motorInfo.motor.liftForce / 5, ForceMode.Force);
                motorInfo.attachedRigidbody.AddRelativeTorque(Vector3.up * motorInfo.motor.liftForce / 5, ForceMode.VelocityChange);
            }

            Debug.LogFormat("Motor [{0}] was broken and detached from drone [{1}]", motorInfo.motor.name, gameObject.name);

            motorInfo.motor.name = "[X] " + motorInfo.motor.name;
            motorInfo.motor.enabled = false;
            motorInfo.motor.transform.parent = null;
            motorInfo.isDestroyed = true;
            isAnyMotorDestroyed = true;
           
            if (motorInfo.motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorBroken();
        }
        
        private void RepairMotor(MotorDestructionInfo motorInfo)
        {
            if (!motorInfo.isDestroyed) return;
               
            motorInfo.motor.transform.SetParent(motorInfo.initialParent);
            motorInfo.motor.transform.localPosition = motorInfo.initialLocalPosition;
            motorInfo.motor.transform.localScale = motorInfo.initialLocalScale;
            motorInfo.motor.transform.localRotation = motorInfo.initialLocalRotation;
            motorInfo.motor.name = motorInfo.motor.name.Replace("[X] ", "");
            motorInfo.motor.enabled = true;
            motorInfo.isDestroyed = false;
            
            if (motorInfo.attachedRigidbody is not null)
                Destroy(motorInfo.attachedRigidbody);
           
            if (motorInfo.motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorRepaired();
            
            Debug.LogFormat("Motor [{0}] was repaired and reattached to drone [{1}]", motorInfo.motor.name, gameObject.name);
        }

        public void RepairAllMotors()
        {
            if (!isAnyMotorDestroyed) return;
           
            isAnyMotorDestroyed = false;
           
            foreach (var motorInfo in motorsLookup.Values)
                RepairMotor(motorInfo);
        }
        
        public void BreakAllMotors()
        {
            foreach (var motorInfo in motorsLookup.Values)
                OnMotorCollided(motorInfo);
        }
    }
}