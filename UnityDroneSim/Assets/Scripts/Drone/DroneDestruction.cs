using System.Collections.Generic;
using Drone.Propulsion;
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
        private bool isAnyMotorDestroyed;
        private DroneComputerBase droneComputer;
        private readonly Dictionary<int, MotorDestructionInfo> motorsLookup = new(4);
        
        
        /// <summary>Maximal collision speed (m/s) that motor can survive.</summary>
        [Range(0f, 20f)] public float breakSpeed = 5f;
        
        /// <summary>Maximal collision impulse (kg*m/s) that motor can survive.</summary>
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
        
        private void OnCollisionEnter(Collision other)
        {
            if (!enabled) return;
            
            var spd = other.relativeVelocity.magnitude;
            var imp = other.impulse.magnitude;
            if (other.contactCount == 0 || (spd < breakSpeed && imp < breakImpulse)) 
                return;

            var thisCollider = other.GetContact(0).thisCollider;
            var colliderParent = thisCollider.transform.parent?.gameObject;

            if (colliderParent is not null && 
                (motorsLookup.TryGetValue(colliderParent.GetInstanceID(), out var motorInfo) || 
                 motorsLookup.TryGetValue(thisCollider.gameObject.GetInstanceID(), out motorInfo)))
            {
                OnMotorCollided(motorInfo);
                Debug.LogFormat("Collision (Motor) [{0}.{1}] -> [{2}]: vel={3:F2} m/s, imp={4:F2} kg*m/s",
                    colliderParent.name, thisCollider.name, other.gameObject.name, spd, imp);
            }
        }
        
        
        private void OnMotorCollided(MotorDestructionInfo motorInfo)
        {
            if (motorInfo.IsDestroyed) return;
            
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
            
            motorInfo.Motor.name = "[X] " + motorInfo.Motor.name;
            motorInfo.Motor.enabled = false;
            motorInfo.Motor.transform.parent = null;
            motorInfo.IsDestroyed = true;
            isAnyMotorDestroyed = true;
           
            if (motorInfo.Motor is DestructibleMotor destructibleMotor)
                destructibleMotor.OnMotorBroken();
        }
        
        private void RepairMotor(MotorDestructionInfo motorInfo)
        {
            if (!motorInfo.IsDestroyed) return;
               
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
            if (!isAnyMotorDestroyed) return;
           
            isAnyMotorDestroyed = false;
            foreach (var motorInfo in motorsLookup.Values)
                RepairMotor(motorInfo);
        }
        
        [ContextMenu("Break All Motors")]
        public void BreakAllMotors()
        {
            foreach (var motorInfo in motorsLookup.Values)
                OnMotorCollided(motorInfo);
        }
    }
}