using UnityEngine;


namespace UtilsDebug
{
    public enum GizmoLabelPlacement { Start, End, Center }


    public struct GizmoOptions
    {
        public Color               Color;
        public Color               LabelColor;
        public float               CapSize;
        public float               VectSize;
        public GizmoLabelPlacement LabelPlacement;
        public FontStyle           LabelStyle;
        public bool                LabelOutline;

        public GizmoOptions(
            Color color,
            float capSize = 0.1f,
            float vectSize = 1f,
            FontStyle labelStyle = FontStyle.Normal,
            GizmoLabelPlacement labelPlacement = GizmoLabelPlacement.Start,
            Color? labelColor = null)
        {
            this.Color = color;
            this.CapSize = capSize;
            this.LabelStyle = labelStyle;
            this.LabelPlacement = labelPlacement;
            this.LabelColor = labelColor ?? color;
            this.VectSize = vectSize;
            LabelOutline = false;
        }
    }
}