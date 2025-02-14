using System;
using System.Collections.Generic;
using UnityEngine;


namespace RPY_PID_Control
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneDestruction : MonoBehaviour
    {
        [Range(0.01f, 10f)] public float destructionVelocity;
        public List<Rigidbody> propellersRigidbodies;
        
        private readonly List<Vector3> propellersInitialLocalPositions = new(4);
        private readonly List<Quaternion> propellersInitialLocalRotations = new(4);
        private bool wasDestroyed;

        
        private void Awake() => InitPropellers();
        
        private void OnValidate() => InitPropellers();
        
        private void InitPropellers()
        {
            if (propellersRigidbodies == null || propellersRigidbodies.Count == 0) return;
            
            propellersInitialLocalPositions.Clear();
            propellersInitialLocalRotations.Clear();
            
            for (var i = 0; i < propellersRigidbodies.Count; i++)
            {
                var propTransform = propellersRigidbodies[i].transform;
                propellersInitialLocalPositions.Add(propTransform.localPosition);
                propellersInitialLocalRotations.Add(propTransform.localRotation);
            }
        }

        private void Start() => Recover();
        
        private void OnApplicationQuit() => Recover();
        
        private void OnCollisionEnter(Collision other)
        {
            if (!wasDestroyed || 
                propellersRigidbodies.Count == 0 || 
                other.relativeVelocity.magnitude < destructionVelocity)
                return;

            wasDestroyed = true;
            
            var parent = transform.parent;
            for (var i = 0; i < propellersRigidbodies.Count; i++)
            {
                var propRigidBody = propellersRigidbodies[i];
                propRigidBody.isKinematic = false;
                propRigidBody.useGravity = true;
                propRigidBody.WakeUp();
                propRigidBody.gameObject.transform.SetParent(parent, true);
            }
            
            Debug.LogFormat("Drone '{0}' has collided and destroyed", gameObject.name);
        }
        
        public void Recover()
        {
            if (!wasDestroyed)
            {
                Debug.LogFormat("Drone '{0}' was not destroyed, no need to recover its parts", gameObject.name);
                return;
            }
            
            wasDestroyed = false;
            for (var i = 0; i < propellersRigidbodies.Count; i++)
            {
                var propRigidBody = propellersRigidbodies[i];
                propRigidBody.isKinematic = true;
                propRigidBody.useGravity = false;
                propRigidBody.Sleep();

                var propTransform = propRigidBody.transform;
                propTransform.SetParent(gameObject.transform);
                propTransform.localPosition = propellersInitialLocalPositions[i];
                propTransform.localRotation = propellersInitialLocalRotations[i];
            }
            
            Debug.LogFormat("Drone '{0}' has recovered its parts", gameObject.name);
        }
    }
}