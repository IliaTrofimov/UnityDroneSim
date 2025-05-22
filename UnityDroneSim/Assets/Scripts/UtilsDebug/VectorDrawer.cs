using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


namespace UtilsDebug
{
    public static class VectorDrawer
    {
        public static void DrawCircle(
            Vector3 origin,
            Vector3 normal,
            float radius,
            string label,
            GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            if (radius <= 0.001f || normal == Vector3.zero) return;
            
            var lastColor = Gizmos.color;
            Gizmos.color = options.Color;
            
            Handles.DrawSolidArc(origin, normal, normal, radius, 360);
            
            Gizmos.color = lastColor;
            #endif
        }

        public static void DrawDirection(
            Vector3 origin,
            Vector3 direction,
            string label,
            GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            if (direction == Vector3.zero) return;

            var q = Quaternion.LookRotation(direction.normalized);
            var end = origin + Vector3.ClampMagnitude(direction, options.VectSize);

            var lastColor = Gizmos.color;
            Gizmos.color = options.Color;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawLine(end - q * (Vector3.forward + Vector3.right).normalized * (options.VectSize * options.VectCapSize), end);
            Gizmos.DrawLine(end - q * (Vector3.forward - Vector3.right).normalized * (options.VectSize * options.VectCapSize), end);

            DrawLineLabel(origin, end, label, options);

            if (options.CapSize > 0.001f)
                Gizmos.DrawSphere(origin, options.CapSize);

            Gizmos.color = lastColor;
            #endif
        }

        public static void DrawDirection(Vector3 origin, Vector3 direction, GizmoOptions options = default) 
            => DrawDirection(origin, direction, "", options);
        
        public static void DrawLine(Vector3 from, Vector3 to, string label, GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Gizmos.color;
            Gizmos.color = options.Color;

            if (from - to != Vector3.zero)
                Gizmos.DrawLine(from, to);

            if (options.CapSize > 0.001f)
            {
                Gizmos.DrawSphere(from, options.CapSize);
                Gizmos.DrawSphere(to, options.CapSize);
            }

            DrawLineLabel(from, to, label, options);
            Gizmos.color = lastColor;
            #endif
        }

        public static void DrawLine(Vector3 from, Vector3 to, GizmoOptions options = default) 
            => DrawLine(from, to, "", options);
        
        public static void DrawArc(Vector3 center, Vector3 direction, Vector3 normal, float angle, string label, GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Handles.color;

            Handles.color = options.Color;
            Handles.DrawSolidArc(center, normal, direction, angle, direction.magnitude);

            if (!string.IsNullOrEmpty(label))
            {
                var to = Quaternion.AngleAxis(angle / 2, normal) * direction;
                if (math.abs(angle) <= 0.001f) to /= 2;
                DrawLineLabel(center, center + to, label, options);
            }
            
            Handles.color = lastColor;
            #endif
        }
        
        public static void DrawArc(Vector3 center, Vector3 direction, Vector3 normal, float angle, GizmoOptions options = default)
        {
            DrawArc(center, direction, normal, angle, "", options);
        }
        
        public static void DrawAngleArc(Vector3 center, Vector3 direction, Vector3 normal, float angle, GizmoOptions options = default)
        {
            DrawArc(center, direction, normal, angle, $"{angle:F1}°", options);
        }
        
        public static void DrawAngleArc(Vector3 center, Vector3 direction, Vector3 normal, float angle, string label, GizmoOptions options = default)
        {
            DrawArc(center, direction, normal, angle, $"{label}: {angle:F1}°", options);
        }

        
        public static void DrawPoint(Vector3 origin, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Gizmos.color;
            Gizmos.color = options.Color;
            Gizmos.DrawSphere(origin, options.CapSize);

            if (!string.IsNullOrEmpty(label))
                DrawLabel(origin, label, options);
            
            Gizmos.color = lastColor;
            #endif
        }

        public static void DrawPoint(Vector3 origin, GizmoOptions options = default)
            => DrawPoint(origin, "", options);
        
        public static void DrawPointCube(Vector3 origin, string label = "", GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            var lastColor = Gizmos.color;
            Gizmos.color = options.Color;
            Gizmos.DrawCube(origin, Vector3.one * options.CapSize);

            if (!string.IsNullOrEmpty(label))
                DrawLabel(origin, label, options);

            Gizmos.color = lastColor;
            #endif
        }

        public static void DrawPointCube(Vector3 origin, GizmoOptions options = default) 
            => DrawPointCube(origin, "", options);
        
        public static void DrawLabel(Vector3 origin, string label, GizmoOptions options = default)
        {
            #if UNITY_EDITOR
            if (options.LabelSize <= 0.01f) return;
            
            var camera = Camera.current;
            var point = camera.WorldToScreenPoint(origin);

            if (point.z > 0 && new Rect(0, 0, camera.pixelWidth, camera.pixelHeight).Contains(point))
            {
                GetFontSize(options.LabelSize, out int fontSize, out float xOffset, out float yOffset);
                if (options.LabelOutline)
                {
                    var outlineColor = options.LabelColor.grayscale > 0.5f ? Color.black : Color.white;
                    Handles.Label(origin,
                        label,
                        new GUIStyle()
                        {
                            normal = { textColor = outlineColor }, // shadow effect
                            onHover = { textColor = Color.black }, // shadow always black on hovering
                            hover = { textColor = Color.black },
                            onActive = { textColor = Color.black },
                            contentOffset = new Vector2(xOffset - 0.5f, yOffset),
                            fontSize = fontSize,
                            fontStyle = options.LabelStyle
                        }
                    );
                }

                Handles.Label(origin,
                    label,
                    new GUIStyle()
                    {
                        normal = { textColor = options.LabelColor },
                        contentOffset = new Vector2(xOffset, yOffset - 1),
                        fontSize = fontSize,
                        fontStyle = options.LabelStyle,
                    }
                );
            }
            #endif
        }

        private static void DrawLineLabel(Vector3 from, Vector3 to, string label, GizmoOptions options)
        {
            if (string.IsNullOrEmpty(label)) return;

            switch (options.LabelPlacement)
            {
            case GizmoLabelPlacement.Start:
                DrawLabel(from, label, options);
                break;
            case GizmoLabelPlacement.End:
                DrawLabel(to, label, options);
                break;
            case GizmoLabelPlacement.Center:
                DrawLabel((from + to) / 2f, label, options);
                break;
            }
        }

        private static void GetFontSize(float scale, out int fontSize, out float xOffset, out float yOffset)
        {
            const int size2160 = 52;
            const int size1440 = (int)(size2160 / 2160.0 * 1440);
            const int size1080 = (int)(size2160 / 2160.0 * 1080);
            const int size768 = (int)(size2160 / 2160.0 * 768);
            const int size480 = (int)(size2160 / 2160.0 * 480);

            if (Screen.height >= 2160)
            {
                fontSize = (int)(size2160 * scale);
                xOffset = 20;
                yOffset = -8;
            }
            else if (Screen.height >= 1440)
            {
                fontSize = (int)(size1440 * scale);
                xOffset = 18;
                yOffset = -5;
            }
            else if (Screen.height >= 1080)
            {
                fontSize = (int)(size1080 * scale);
                xOffset = 12;
                yOffset = -2;
            }
            else if (Screen.height >= 768)
            {
                fontSize = (int)(size768 * scale);
                xOffset = 10;
                yOffset = -2;
            }
            else
            {
                fontSize = (int)(size480 * scale);
                xOffset = 8;
                yOffset = -2;
            }
        }
    }
}