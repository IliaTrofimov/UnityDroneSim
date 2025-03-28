using UnityEngine;
using UnityEngine.UIElements;


namespace Utils
{
    public static class Painter2DExtensions
    {
        public static void DrawLine(this Painter2D painter, Vector2 start, Vector2 end, Color? color = null)
        {
            if (color.HasValue && painter.strokeColor != color.Value)
                painter.strokeColor = color.Value;
            painter.MoveTo(start);
            painter.LineTo(end);
        }
        
        
        public static void DrawLine(this Painter2D painter, float x1, float y1, float x2, float y2, Color? color = null)
        { 
            painter.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color);
        }
    }
}