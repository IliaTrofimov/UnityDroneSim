using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils;


namespace UtilsDebug
{
    /// <summary>
    /// Helper component that draws vectors for transform object.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class TransformDebug : MonoBehaviour
    {
        private GizmoOptions globalXOptions, globalYOptions, globalZOptions;
        private GizmoOptions localXOptions, localYOptions, localZOptions;
        private GizmoOptions velocityOptions;

        private Vector3 lastPosition, velocity;
        
        public bool showGlobalCoordinates = true;
        public bool showVelocity = false;

        private void Awake()
        {
            if (!gameObject.TryGetDimensions(out Vector3 bounds))
                bounds = Vector3.one;

            var capSize = math.cmin(bounds);
            var vectSize = math.cmax(bounds);
            
            globalXOptions = new GizmoOptions(Handles.xAxisColor * 0.7f, capSize, vectSize, FontStyle.Italic);
            globalYOptions = new GizmoOptions(Handles.yAxisColor * 0.7f, capSize, vectSize, FontStyle.Italic);
            globalZOptions = new GizmoOptions(Handles.zAxisColor * 0.7f, capSize, vectSize, FontStyle.Italic);
            
            localXOptions = new GizmoOptions(Handles.xAxisColor, capSize, vectSize);
            localYOptions = new GizmoOptions(Handles.yAxisColor, capSize, vectSize);
            localZOptions = new GizmoOptions(Handles.zAxisColor, capSize, vectSize);
            
            velocityOptions = new GizmoOptions(Color.yellow, capSize, vectSize, FontStyle.Bold);
        }

        private void Update()
        {
            var pos = transform.position;
            var right = transform.right;
            var up = transform.up;
            var fwd = transform.forward;
            
            VectorDrawer.DrawDirection(pos, right, "x'", localXOptions);
            VectorDrawer.DrawDirection(pos, up, "y'", localYOptions);
            VectorDrawer.DrawDirection(pos, fwd,"z'",  localZOptions);

            if (showGlobalCoordinates)
            {
                if (right - Vector3.right != Vector3.zero)
                    VectorDrawer.DrawDirection(pos, Vector3.right, "x", globalXOptions);
                if (up - Vector3.up != Vector3.zero)
                    VectorDrawer.DrawDirection(pos, Vector3.up, "y", globalYOptions);
                if (fwd - Vector3.forward != Vector3.zero)
                    VectorDrawer.DrawDirection(pos, Vector3.forward, "z", globalZOptions);
            }

            if (showVelocity && velocity != Vector3.zero)
                VectorDrawer.DrawDirection(pos, velocity, "vel", velocityOptions);
        }

        private void FixedUpdate()
        {
            velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
            lastPosition = transform.position;
        }
    }
}