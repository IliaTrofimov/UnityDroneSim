using System;
using UnityEngine;
using Utils;


namespace UtilsDebug
{
    [Flags]
    public enum VelocityDebugType
    {
        None              = 0,
        CenterOfMass      = 1,
        CenterOfMassError = 2,
        Linear            = 4,
        Angular           = 8,
        Acceleration      = 16
    }


    /// <summary>
    /// Helper component that draws physics vectors and print stats for rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class RigidbodyDebug : MonoBehaviour
    {
        private string _linVelocityName, _angVelocityName, _cmPointName, _accelerationName, _cmErrorName;

        private GizmoOptions _linVelocityGizmoOptions;
        private GizmoOptions _angVelocityGizmoOptions;
        private GizmoOptions _accelerationGizmoOptions;
        private GizmoOptions _centerOfMassGizmoOptions;

        private Rigidbody _rigidBody;

        private Vector3    _startPosition, _lastVelocity, _linearAcceleration;
        private Quaternion _startRotation;

        [Header("Options")]
        public VelocityDebugType debugType = VelocityDebugType.CenterOfMass | VelocityDebugType.Linear;

        [Header("Sizes")] 
        [Range(0f, 2f)] 
        public float capSize = 0.2f;
       
        [Range(0f, 2f)]                   
        public float vectorSize = 1f;
        private float _boundsLength;


        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _linVelocityName = $"{gameObject.name}_Vel";
            _angVelocityName = $"{gameObject.name}_AngVel";
            _cmPointName = $"{gameObject.name}_CoM";
            _accelerationName = $"{gameObject.name}_Acc";
            _cmErrorName = "cm_err";

            gameObject.TryGetDimensions(out _boundsLength);

            var capSize = _boundsLength / 2 * this.capSize;
            var vectSize = _boundsLength / 2 * vectorSize;

            _linVelocityGizmoOptions =
                new GizmoOptions(Color.yellow, capSize, vectSize, FontStyle.Bold, GizmoLabelPlacement.End);

            _angVelocityGizmoOptions =
                new GizmoOptions(Color.cyan, capSize, vectSize, FontStyle.Bold, GizmoLabelPlacement.End);

            _accelerationGizmoOptions = new GizmoOptions(new Color(1f, 0.5f, 0.016f),
                capSize,
                vectSize,
                FontStyle.Bold,
                GizmoLabelPlacement.End
            );

            _centerOfMassGizmoOptions =
                new GizmoOptions(Color.grey, capSize, vectSize, FontStyle.Italic, GizmoLabelPlacement.End);
        }

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        private void OnValidate()
        {
            var capSize = _boundsLength / 2 * this.capSize;
            var vectSize = _boundsLength / 2 * vectorSize;

            _linVelocityGizmoOptions.CapSize = capSize;
            _angVelocityGizmoOptions.CapSize = capSize;
            _accelerationGizmoOptions.CapSize = capSize;
            _centerOfMassGizmoOptions.CapSize = capSize;
            _linVelocityGizmoOptions.VectSize = vectSize;
            _angVelocityGizmoOptions.VectSize = vectSize;
            _accelerationGizmoOptions.VectSize = vectSize;
            _centerOfMassGizmoOptions.VectSize = vectSize;
        }

        private void OnDrawGizmos()
        {
            if (debugType == VelocityDebugType.None) return;

            var p = _rigidBody.transform.TransformPoint(_rigidBody.centerOfMass);

            if (debugType.HasFlag(VelocityDebugType.CenterOfMassError))
                VectorDrawer.DrawLine(_rigidBody.transform.TransformPoint(_rigidBody.centerOfMass),
                    _rigidBody.position,
                    _cmErrorName,
                    _centerOfMassGizmoOptions
                );
            else if (debugType.HasFlag(VelocityDebugType.CenterOfMass))
                VectorDrawer.DrawPoint(p, _cmPointName, _centerOfMassGizmoOptions);

            if (debugType.HasFlag(VelocityDebugType.Linear))
                VectorDrawer.DrawDirection(p,
                    _rigidBody.linearVelocity * vectorSize,
                    _linVelocityName,
                    _linVelocityGizmoOptions
                );

            if (debugType.HasFlag(VelocityDebugType.Angular))
                VectorDrawer.DrawDirection(p,
                    _rigidBody.angularVelocity * vectorSize,
                    _angVelocityName,
                    _angVelocityGizmoOptions
                );

            if (debugType.HasFlag(VelocityDebugType.Acceleration))
                VectorDrawer.DrawDirection(p,
                    _linearAcceleration * vectorSize,
                    _accelerationName,
                    _accelerationGizmoOptions
                );
        }

        private void FixedUpdate()
        {
            _linearAcceleration = (_rigidBody.linearVelocity - _lastVelocity) / Time.fixedDeltaTime;
            _lastVelocity = _rigidBody.linearVelocity;
        }


        internal void StopMovement(bool stopLinearMovement, bool stopAngularMovement)
        {
            if (stopAngularMovement) _rigidBody.angularVelocity = Vector3.zero;
            if (stopLinearMovement) _rigidBody.linearVelocity = Vector3.zero;
        }

        internal void Recover()
        {
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            StopMovement(true, true);
            Debug.Log($"{gameObject.name} was recovered to initial position at {_startPosition:F2}");
        }

        internal void GetBodyParameters(
            out Vector3 linVel,
            out Vector3 angVel,
            out Vector3 linAcc,
            out Vector3 centerOfMass,
            out Vector3 cmError)
        {
            if (_rigidBody)
            {
                linVel = _rigidBody.linearVelocity;
                angVel = _rigidBody.angularVelocity;
                linAcc = _linearAcceleration;
                centerOfMass = _rigidBody.centerOfMass;
                cmError = _rigidBody.centerOfMass;
            }
            else
            {
                linVel = Vector3.zero;
                angVel = Vector3.zero;
                linAcc = Vector3.zero;
                centerOfMass = Vector3.zero;
                cmError = Vector3.zero;
            }
        }
    }
}