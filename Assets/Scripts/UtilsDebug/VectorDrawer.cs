using UnityEditor;
using UnityEngine;


namespace UtilsDebug
{
    public static class VectorDrawer
    {
        public static void DrawCircle(Vector3 origin, Vector3 normal, float radius, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            if (radius <= 0.001f || normal == Vector3.zero) return;
            
            if (options.capSize > 0.001f)
                Gizmos.DrawSphere(origin, options.capSize);
            
            if (!string.IsNullOrEmpty(label))
                DrawLabel(origin, label, options);
            
            Handles.DrawWireArc(origin, normal, Vector3.right, 360f, radius);
            #endif
        }
        
        public static void DrawDirection(Vector3 origin, Vector3 direction, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            if (direction == Vector3.zero) return;
            
            var q = Quaternion.LookRotation(direction.normalized);
            var end = origin + Vector3.ClampMagnitude(direction, options.vectSize);
            
            var lastColor = Gizmos.color;
            Gizmos.color = options.color;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawLine(end - q * (Vector3.forward + Vector3.right).normalized * (options.capSize * 2), end);
            Gizmos.DrawLine(end - q * (Vector3.forward - Vector3.right).normalized * (options.capSize * 2), end);
            
            DrawLineLabel(origin, end, label, options);
            
            if (options.capSize > 0.001f)
                Gizmos.DrawSphere(origin, options.capSize);
            
            Gizmos.color = lastColor;
            #endif
        }
        
        public static void DrawLine(Vector3 from, Vector3 to, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Gizmos.color;
            Gizmos.color = options.color;

            if (from - to != Vector3.zero)
                Gizmos.DrawLine(from, to);

            if (options.capSize > 0.001f)
            {
                Gizmos.DrawSphere(from, options.capSize);
                Gizmos.DrawSphere(to, options.capSize);
            }

            DrawLineLabel(from, to, label, options);
            Gizmos.color = lastColor;
            #endif
        }
        
        public static void DrawPoint(Vector3 origin, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Gizmos.color;
            Gizmos.color = options.color;
            Gizmos.DrawSphere(origin, options.capSize);
            
            if (!string.IsNullOrEmpty(label))
                DrawLabel(origin, label, options);
            Gizmos.color = lastColor;
            #endif
        }
        
        public static void DrawPointCube(Vector3 origin, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Gizmos.color;
            Gizmos.color = options.color;
            Gizmos.DrawCube(origin, Vector3.one * options.capSize);
            
            if (!string.IsNullOrEmpty(label))
                DrawLabel(origin, label, options);
            Gizmos.color = lastColor;
            #endif
        }
        
        public static void DrawLabel(Vector3 origin, string label, GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var camera = UnityEngine.Camera.current;
            var point = camera.WorldToScreenPoint(origin);
            
            if (point.z > 0 && new Rect(0, 0, camera.pixelWidth, camera.pixelHeight).Contains(point))
            {
                if (options.labelOutline)
                {
                    var outlineColor = options.labelColor.grayscale > 0.5f ? Color.black : Color.white;
                    Handles.Label(origin, label, new GUIStyle("label")
                    {
                        normal = { textColor = outlineColor }, // shadow effect
                        onHover = { textColor = Color.black }, // shadow always black on hovering
                        hover = { textColor = Color.black },
                        onActive = { textColor = Color.black },
                        contentOffset = new Vector2(0, -8),
                        fontStyle = options.labelStyle
                    });   
                }
                Handles.Label(origin, label, new GUIStyle("label")
                {
                    normal = { textColor = options.labelColor },
                    contentOffset = new Vector2(2, -7),
                    fontStyle = options.labelStyle,
                });
            }
            #endif
        }
        
        private static void DrawLineLabel(Vector3 from, Vector3 to, string label, GizmoOptions options)
        {
            if (string.IsNullOrEmpty(label)) return;

            switch (options.labelPlacement)
            {
                case GizmoLabelPlacement.Start:
                    DrawLabel(from, label, options);
                    break;
                case GizmoLabelPlacement.End:
                    DrawLabel(from, label, options);
                    break;
                case GizmoLabelPlacement.Center:
                    DrawLabel((from + to) / 2f, label, options);
                    break;
            }
        }

    }
}