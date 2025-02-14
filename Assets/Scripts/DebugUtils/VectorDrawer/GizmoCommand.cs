using UnityEngine;


namespace DebugUtils.VectorDrawer
{
    internal struct GizmoCommand
    {
        public Color color;
        public Vector3 start;
        public Vector3 end;
        public GizmoFlags flags;
        
        public float capSize;
        public FontStyle labelStyle;
        
        public void Point(Vector3 point, Color? color = null)
        {
            start = end = point;
            flags = GizmoFlags.Point;
            this.color = color ?? Color.white;
        }
        
        public void Vector(Vector3 start, Vector3 end, Color? color = null)
        {
            this.start = start;
            this.end = end;
            this.color = color ?? Color.white;
            flags = GizmoFlags.Vector;
        }
        
        public void Direction(Vector3 origin, Vector3 direction, Color? color = null)
        {
            start = origin;
            end = origin + direction;
            this.color = color ?? Color.white;
            flags = GizmoFlags.Vector;
        }
        
        public void Line(Vector3 start, Vector3 end, Color? color = null)
        {
            this.start = start;
            this.end = end;
            this.color = color ?? Color.white;
            flags = GizmoFlags.Line;
        }
    }
}