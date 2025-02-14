using UnityEngine;


namespace DebugUtils.VectorDrawer
{
    public struct GizmoOptions
    {
        public Color color;
        public Color labelColor;
        public float capSize;
        public float maxVectorLength;
        public GizmoLabelPlacement labelPlacement;
        public FontStyle labelStyle;
        
        public GizmoOptions(Color color, 
                            float capSize = 0.1f, FontStyle labelStyle = FontStyle.Normal,
                            GizmoLabelPlacement labelPlacement = GizmoLabelPlacement.Start, Color? labelColor = null)
        {
            this.color = color;
            this.capSize = capSize;
            this.labelStyle = labelStyle;
            this.labelPlacement = labelPlacement;
            this.labelColor = labelColor ?? color;
            this.maxVectorLength = 1e3f;
        }
    }
}