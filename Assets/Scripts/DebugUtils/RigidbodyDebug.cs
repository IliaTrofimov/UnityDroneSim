using System;
using System.Linq;
using DebugUtils.VectorDrawer;
using InspectorTools;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;


namespace DebugUtils
{
    [Flags]
    public enum VelocityDebugType
    {
        None = 0,
        CenterOfMass = 1,
        CenterOfMassError = 2,
        Linear = 4,
        Angular = 8,
        Acceleration = 16,
    }
    
    /// <summary>
    /// Helper component that draws physics vectors and print stats for rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class RigidbodyDebug : MonoBehaviour
    {
        private string linVelocityName, angVelocityName, cmPointName, accelerationName, cmErrorName;
        
        private GizmoOptions linVelocityGizmoOptions;
        private GizmoOptions angVelocityGizmoOptions;
        private GizmoOptions accelerationGizmoOptions;
        private GizmoOptions centerOfMassGizmoOptions;

        private Rigidbody rigidBody;
        
        private Vector3 startPosition, lastVelocity, linearAcceleration;
        private Quaternion startRotation;
        
        [Header("Options")]
        public VelocityDebugType debugType = VelocityDebugType.CenterOfMass | VelocityDebugType.Linear;
        
        [Header("Sizes")]
        [Range(0f, 2f)] public float capSize = 0.2f;
        [Range(0f, 2f)] public float vectorSize = 1f;
        private float boundsLength;

        
        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            linVelocityName = $"{gameObject.name}_Vel";
            angVelocityName = $"{gameObject.name}_AngVel";
            cmPointName = $"{gameObject.name}_CoM";
            accelerationName = $"{gameObject.name}_Acc";
            cmErrorName = "cm_err";
            
            gameObject.TryGetDimensions(out boundsLength);
            
            var actualCapSize = boundsLength / 2 * capSize;
            linVelocityGizmoOptions = new GizmoOptions(Color.yellow, actualCapSize, FontStyle.Bold, GizmoLabelPlacement.End);
            angVelocityGizmoOptions = new GizmoOptions(Color.cyan, actualCapSize, FontStyle.Bold, GizmoLabelPlacement.End);
            accelerationGizmoOptions = new GizmoOptions(new(1f, 0.5f, 0.016f), actualCapSize, FontStyle.Bold, GizmoLabelPlacement.End);
            centerOfMassGizmoOptions = new GizmoOptions(Color.grey, actualCapSize, FontStyle.Italic, GizmoLabelPlacement.End);
            
            var maxVectorLength = boundsLength / 2 * vectorSize;
            linVelocityGizmoOptions.maxVectorLength = maxVectorLength;
            angVelocityGizmoOptions.maxVectorLength = maxVectorLength;
            accelerationGizmoOptions.maxVectorLength = maxVectorLength;
            centerOfMassGizmoOptions.maxVectorLength = maxVectorLength;
        }
        
        private void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void OnValidate()
        {
            var actualCapSize = boundsLength / 2 * capSize;
            linVelocityGizmoOptions.capSize = actualCapSize;
            angVelocityGizmoOptions.capSize = actualCapSize;
            accelerationGizmoOptions.capSize = actualCapSize;
            centerOfMassGizmoOptions.capSize = actualCapSize;
            
            var maxVectorLength = boundsLength / 2 * vectorSize;
            linVelocityGizmoOptions.maxVectorLength = maxVectorLength;
            angVelocityGizmoOptions.maxVectorLength = maxVectorLength;
            accelerationGizmoOptions.maxVectorLength = maxVectorLength;
            centerOfMassGizmoOptions.maxVectorLength = maxVectorLength;
        }
        
        private void OnDrawGizmos()
        {
            if (debugType == VelocityDebugType.None) return;

            var p = rigidBody.transform.TransformPoint(rigidBody.centerOfMass);

            if (debugType.HasFlag(VelocityDebugType.CenterOfMassError))
            {
                VectorDrawerLite.DrawLine(rigidBody.transform.TransformPoint(rigidBody.centerOfMass),
                    rigidBody.position,
                    cmErrorName,
                    centerOfMassGizmoOptions);
            }
            else if (debugType.HasFlag(VelocityDebugType.CenterOfMass))
            {
                VectorDrawerLite.DrawPoint(p, cmPointName, centerOfMassGizmoOptions);
            }

            if (debugType.HasFlag(VelocityDebugType.Linear)) 
                VectorDrawerLite.DrawDirection(p, rigidBody.linearVelocity * vectorSize, linVelocityName, linVelocityGizmoOptions);
            
            if (debugType.HasFlag(VelocityDebugType.Angular)) 
                VectorDrawerLite.DrawDirection(p, rigidBody.angularVelocity * vectorSize, angVelocityName, angVelocityGizmoOptions);
            
            if (debugType.HasFlag(VelocityDebugType.Acceleration)) 
                VectorDrawerLite.DrawDirection(p, linearAcceleration * vectorSize, accelerationName, accelerationGizmoOptions);
        }
        
        private void FixedUpdate()
        {
            linearAcceleration = (rigidBody.linearVelocity - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = rigidBody.linearVelocity;
        }

        
        internal void StopMovement(bool stopLinearMovement, bool stopAngularMovement)
        {
            if (stopAngularMovement) rigidBody.angularVelocity = Vector3.zero;
            if (stopLinearMovement) rigidBody.linearVelocity = Vector3.zero;
        }

        internal void Recover()
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            StopMovement(true, true);
            Debug.Log($"{gameObject.name} was recovered to initial position at {startPosition:F2}");
        }

        internal void GetBodyParameters(out Vector3 linVel, out Vector3 angVel, out Vector3 linAcc,
                                        out Vector3 centerOfMass, out Vector3 cmError)
        {
            if (rigidBody)
            {
                linVel = rigidBody.linearVelocity;
                angVel = rigidBody.angularVelocity;
                linAcc = linearAcceleration;
                centerOfMass = rigidBody.transform.TransformPoint(rigidBody.centerOfMass);   
                cmError = rigidBody.position - centerOfMass;
            }
            else
            {
                linVel = Vector3.zero;
                angVel = Vector3.zero;
                linAcc = Vector3.zero;
                centerOfMass = Vector3.zero;
                cmError =  Vector3.zero;
            }
        }
    }
}