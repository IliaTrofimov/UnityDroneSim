using InspectorTools;
using UnityEditor;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MeasurableObject : MonoBehaviour
    {
        private Vector3 _actualCenter;
        
        [SerializeField, ReadOnlyField]
        [Tooltip("Object or/and its children have MeshFilters components attached.")]
        private bool hasMesh;
        
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
        
        private void OnEnable()
        {
            ResetMeasurements();
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled || !hasMesh) return;
            
            Handles.color = Color.yellow;
            Handles.DrawWireCube(_actualCenter, dimensions);

            var distMax = Vector3.Distance(Camera.current.transform.position, maxPoint);
            var distMin = Vector3.Distance(Camera.current.transform.position, minPoint);
            var distCenter = Vector3.Distance(Camera.current.transform.position, _actualCenter);
            var label = $"{name}: {dimensions.x}x{dimensions.y}x{dimensions.z}";
            var gizmoOptions = new GizmoOptions()
            {
                LabelColor = Color.yellow,
                LabelOutline = true,
                CapSize = 0
            };
            
            if (distMax < distMin && distMax < distCenter)
            {
                VectorDrawer.DrawLabel(maxPoint, label, gizmoOptions);
            }
            else if (distMin < distMax && distMin < distCenter)
            {
                VectorDrawer.DrawLabel(minPoint, label, gizmoOptions);
            }
            else
            {
                VectorDrawer.DrawLabel(_actualCenter, label, gizmoOptions);
            }
        }

        [ContextMenu("Reset measurements")]
        public bool ResetMeasurements()
        {
            hasMesh = gameObject.TryGetDimensions(out dimensions);

            if (!hasMesh)
            {
                dimensions = Vector3.zero;
                minPoint = Vector3.zero;
                maxPoint = Vector3.zero;
                Debug.LogWarningFormat("Cannot measure '{0}' size since this game object is missing MeshFilter.", gameObject.name);   
            }
            else
            {
                gameObject.TryGetDimensions(out minPoint, out maxPoint);
                _actualCenter = (maxPoint + minPoint) / 2f;
            }

            return hasMesh;
        }
    }
}