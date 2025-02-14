using UnityEngine;


namespace DebugUtils
{
    /// <summary>
    /// Helper component that draws vectors for transform object.
    /// </summary>
    [RequireComponent(typeof(VectorDrawer.VectorDrawer))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class TransformDebug : MonoBehaviour
    {
        private VectorDrawer.VectorDrawer vectorDrawer;
        
        private readonly Color darkRed = Color.red * 0.7f;
        private readonly Color darkBlue = Color.blue * 0.7f;
        private readonly Color darkGreen = Color.green * 0.7f;
        private readonly Color velocityColor = Color.yellow;
       
        private Vector3 lastPosition, velocity;
        
        public bool showGlobalCoordinates = true;
        public bool showVelocity = false;
        
        private void OnEnable() => vectorDrawer ??= GetComponent<VectorDrawer.VectorDrawer>();
        private void OnDisable() => vectorDrawer.ClearGizmos();

        private void Update()
        {
            var pos = transform.position;
            var right = transform.right;
            var up = transform.up;
            var fwd = transform.forward;
            
            vectorDrawer.DrawDirection("x'", pos, right, Color.red);
            vectorDrawer.DrawDirection("y'", pos, up, Color.green);
            vectorDrawer.DrawDirection("z'", pos, fwd, Color.blue);

            if (showGlobalCoordinates)
            {
                if (right - Vector3.right != Vector3.zero)
                    vectorDrawer.DrawDirection("x", pos, Vector3.right, darkRed);
                if (up - Vector3.up != Vector3.zero)
                    vectorDrawer.DrawDirection("y", pos, Vector3.up, darkGreen);
                if (fwd - Vector3.forward != Vector3.zero)
                    vectorDrawer.DrawDirection("z", pos, Vector3.forward, darkBlue);
            }

            if (showVelocity && velocity != Vector3.zero)
                vectorDrawer.DrawDirection("v'", pos, velocity, velocityColor);
        }

        private void FixedUpdate()
        {
            if (!showVelocity) return;

            velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
            lastPosition = transform.position;
        }

        private void OnValidate()
        {
            if (!vectorDrawer) return;
            
            if (!showGlobalCoordinates)
                vectorDrawer.ClearGizmos("x", "y", "z");
            if (!showVelocity)
                vectorDrawer.ClearGizmo("v'");
        }
    }
}