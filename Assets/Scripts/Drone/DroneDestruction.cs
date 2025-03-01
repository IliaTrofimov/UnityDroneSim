using System;
using System.Collections.Generic;
using Drone.Propulsion;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UtilsDebug;


namespace Drone
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(QuadcopterComputer))]
    [RequireComponent(typeof(Rigidbody))]
    public class DroneDestruction : MonoBehaviour
    {
        private QuadcopterComputer droneComputer;
        private readonly Dictionary<int, DroneMotor> motorsLookup = new(4);
        
        [Range(0f, 20f)] public float breakSpeed;
        [Range(0f, 20f)] public float breakImpulse;
        
        
       private void Awake()
       {
           droneComputer = GetComponent<QuadcopterComputer>();
           
           motorsLookup[droneComputer.motorFrontLeft.gameObject.GetInstanceID()] = droneComputer.motorFrontLeft;
           motorsLookup[droneComputer.motorFrontRight.gameObject.GetInstanceID()] = droneComputer.motorFrontRight;
           motorsLookup[droneComputer.motorRearLeft.gameObject.GetInstanceID()] = droneComputer.motorRearLeft;
           motorsLookup[droneComputer.motorRearRight.gameObject.GetInstanceID()] = droneComputer.motorRearRight;
       }

       private void OnCollisionEnter(Collision other)
       {
           var spd = other.relativeVelocity.magnitude;
           var imp = other.impulse.magnitude;
           if (other.contactCount == 0 || (spd < breakSpeed && imp < breakImpulse)) 
               return;

           var thisCollider = other.GetContact(0).thisCollider;
           var colliderParent = thisCollider.transform.parent.gameObject;
          
           if (!motorsLookup.TryGetValue(colliderParent.GetInstanceID(), out var motor))
           {
               Debug.LogFormat("Collision: '{0}' -> '{1}' (v: {2:F3} m/s, i: {3:F3} kg*m/s), motor was not found", 
                   thisCollider.name, other.gameObject.name, spd, imp);
               return;
           }
          
           var rotorRigidbody = colliderParent.AddComponent<Rigidbody>();
           if (rotorRigidbody != null)
           {
               rotorRigidbody.mass = 0.01f;
               rotorRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
               rotorRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
           }
           
           colliderParent.transform.parent = null;
           colliderParent.name = "[X] " + colliderParent.name;
           
           motor.enabled = false;
           rotorRigidbody.AddRelativeForce(Vector3.up * motor.liftForce / 5, ForceMode.Force);
           rotorRigidbody.AddRelativeTorque(Vector3.up * motor.liftForce, ForceMode.Force);
           
           if (motor is DestructibleMotor destructibleMotor)
           {
               destructibleMotor.OnMotorCollided();
           }
           else
           {
               thisCollider.enabled = false;
           }
           
           Debug.LogFormat("Collision: '{0}.{1}' -> '{2}' (v: {3:F3} m/s, i: {4:F3} kg*m/s), motor '{5}' was detached",
               colliderParent.name, thisCollider.name, other.gameObject.name, spd, imp, motor.name);
       }
    }
}