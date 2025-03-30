using UnityEngine;


namespace UtilsDebug
{
    public enum GizmoLabelPlacement
    {
        Start, End, Center
    }

    public struct GizmoOptions
    {
        public Color color;
        public Color labelColor;
        public float capSize;
        public float vectSize;
        public GizmoLabelPlacement labelPlacement;
        public FontStyle labelStyle;
        public bool labelOutline;
        
        public GizmoOptions(Color color,
                            float capSize = 0.1f, float vectSize = 1f, 
                            FontStyle labelStyle = FontStyle.Normal,
                            GizmoLabelPlacement labelPlacement = GizmoLabelPlacement.Start,
                            Color? labelColor = null)
        {
            this.color = color;
            this.capSize = capSize;
            this.labelStyle = labelStyle;
            this.labelPlacement = labelPlacement;
            this.labelColor = labelColor ?? color;
            this.vectSize = vectSize;
            this.labelOutline = false;
        }
    }
}