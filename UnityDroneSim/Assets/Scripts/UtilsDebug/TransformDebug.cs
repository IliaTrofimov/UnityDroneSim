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
        private GizmoOptions _globalXOptions, _globalYOptions, _globalZOptions;
        private GizmoOptions _localXOptions,  _localYOptions,  _localZOptions;
        private GizmoOptions _velocityOptions;

        private Vector3 _lastPosition, _velocity;

        public bool showGlobalCoordinates = true;
        public bool showVelocity;

        private void Awake()
        {
            if (!gameObject.TryGetDimensions(out Vector3 bounds))
                bounds = Vector3.one;

            var capSize = math.cmin(bounds);
            var vectSize = math.cmax(bounds);

            _globalXOptions = new GizmoOptions(Handles.xAxisColor * 0.7f, capSize, vectSize, FontStyle.Italic);
            _globalYOptions = new GizmoOptions(Handles.yAxisColor * 0.7f, capSize, vectSize, FontStyle.Italic);
            _globalZOptions = new GizmoOptions(Handles.zAxisColor * 0.7f, capSize, vectSize, FontStyle.Italic);

            _localXOptions = new GizmoOptions(Handles.xAxisColor, capSize, vectSize);
            _localYOptions = new GizmoOptions(Handles.yAxisColor, capSize, vectSize);
            _localZOptions = new GizmoOptions(Handles.zAxisColor, capSize, vectSize);

            _velocityOptions = new GizmoOptions(Color.yellow, capSize, vectSize, FontStyle.Bold);
        }

        private void Update()
        {
            var pos = transform.position;
            var right = transform.right;
            var up = transform.up;
            var fwd = transform.forward;

            VectorDrawer.DrawDirection(pos, right, "x'", _localXOptions);
            VectorDrawer.DrawDirection(pos, up, "y'", _localYOptions);
            VectorDrawer.DrawDirection(pos, fwd, "z'", _localZOptions);

            if (showGlobalCoordinates)
            {
                if (right - Vector3.right != Vector3.zero)
                    VectorDrawer.DrawDirection(pos, Vector3.right, "x", _globalXOptions);

                if (up - Vector3.up != Vector3.zero)
                    VectorDrawer.DrawDirection(pos, Vector3.up, "y", _globalYOptions);

                if (fwd - Vector3.forward != Vector3.zero)
                    VectorDrawer.DrawDirection(pos, Vector3.forward, "z", _globalZOptions);
            }

            if (showVelocity && _velocity != Vector3.zero)
                VectorDrawer.DrawDirection(pos, _velocity, "vel", _velocityOptions);
        }

        private void FixedUpdate()
        {
            _velocity = (transform.position - _lastPosition) / Time.fixedDeltaTime;
            _lastPosition = transform.position;
        }
    }
}