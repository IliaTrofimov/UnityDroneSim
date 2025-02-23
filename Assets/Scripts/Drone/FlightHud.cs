using Exceptions;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;


namespace Drone
{
    public class FlightHud : VisualElement
    {
        public Color hudColor = Color.green;
        
        public Vector3 currentRotation;
        public Vector3 currentVelocity;
        public float curentPitch, currentYaw, currentRoll, currentThrottle;
        
        
        public FlightHud()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            painter.fillColor = hudColor;

            DrawRotationMarker(painter);
            DrawCenterCrossMarker(painter);
        }

        private void DrawRotationMarker(Painter2D painter)
        {
            painter.BeginPath();
            
            
            
            
            painter.ClosePath(); 
        }
        
        private void DrawCenterCrossMarker(Painter2D painter)
        {
            painter.BeginPath();

            painter.lineWidth = 2;
            painter.DrawLine(0, layout.height / 2, layout.width, layout.height / 2);
            painter.DrawLine(layout.width / 2, 0, layout.width / 2, layout.height);

            painter.ClosePath(); 
        }

    }
}