using System;
using InspectorTools;
using UnityEditor;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace ProceduralRoad
{
    /// <summary>
    /// Game object that can be measured along X, Y and Z axis.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MeasurableObject : MonoBehaviour
    {
        private Vector3 _actualCenter;
        
        [SerializeField]
        [Header("Measurements")]
        [Tooltip("Show measurements gizmos.")]
        protected bool showDebug;
        
        [SerializeField, ReadOnlyField]
        [Tooltip("Object or/and its children have MeshFilters components attached.")]
        private bool hasMesh;
        
        [SerializeField, ReadOnlyField] 
        [Tooltip("Distance between Min and Max points.")]
        private float diagonal;
        
        [SerializeField, ReadOnlyField] 
        [Tooltip("Calculated dimensions of object. Objects without MeshFilter can't calculate their dimensions.")]
        private Vector3 dimensions;
        
        [SerializeField, ReadOnlyField] 
        [Tooltip("Point at the object with minimal coordinates. Objects without MeshFilter can't calculate their dimensions.")]
        private Vector3 minPoint;
        
        [SerializeField, ReadOnlyField] 
        [Tooltip("Point at the object with maximal coordinates. Objects without MeshFilter can't calculate their dimensions.")]
        private Vector3 maxPoint;
        
        public Vector3 Dimensions => dimensions;
        public Vector3 ActualCenter => _actualCenter;
        public Vector3 MinPoint => minPoint;
        public Vector3 MaxPoint => maxPoint;
        public float Diagonal => diagonal;


        private void Awake() => ResetMeasurements();
        
        private void OnEnable() => ResetMeasurements();

        private void OnDrawGizmosSelected()
        {
            if (!enabled || !showDebug || !hasMesh) return;
            
            Handles.color = Color.yellow;
            Handles.DrawWireCube(_actualCenter, Dimensions);
        }


        [ContextMenu("Reset measurements")]
        public bool ResetMeasurements()
        {
            hasMesh = gameObject.TryGetDimensions(out minPoint, out maxPoint);

            if (!hasMesh)
            {
                dimensions = Vector3.zero;
                minPoint = Vector3.zero;
                maxPoint = Vector3.zero;
                Debug.LogWarningFormat("Cannot measure '{0}' size since this game object is missing MeshFilter.", gameObject.name);   
            }
            else
            {
                dimensions = (maxPoint - minPoint).Abs();
                diagonal = dimensions.magnitude;
                _actualCenter = (maxPoint + minPoint) / 2f;
            }

            return hasMesh;
        }
    }
}