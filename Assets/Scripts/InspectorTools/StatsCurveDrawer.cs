using System;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class StatsCurveAttribute : PropertyAttribute
    {
        public int BufferSize { get; set; }
        public StatsCurveAttribute(int bufferSize = 20)
        {
            BufferSize = bufferSize;
        }
    }
    
    [CustomPropertyDrawer(typeof(StatsCurveAttribute))]
    public class StatsCurveDrawer : PropertyDrawer
    {
        private bool warningHasFired;
        private string curveShortName;
        private AnimationCurve plotCurve = new();
        private Keyframe[] plotPoints;
        private int currentPlotPoint, currentFrame;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShowWarning(position, property, label)) return;

            InitBuffer();
            UpdateBuffer(property);
            
            var wide = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = false;
            
            var labelRect = new Rect
            {
                x = position.x,
                y = position.y + EditorGUIUtility.singleLineHeight / 2,
                width = EditorGUIUtility.labelWidth * 0.6f,
                height = EditorGUIUtility.singleLineHeight
            };
            var valueRect = new Rect
            {
                x = labelRect.xMax + 1,
                y = position.y + EditorGUIUtility.singleLineHeight / 2,
                width = EditorGUIUtility.labelWidth * 0.4f,
                height = EditorGUIUtility.singleLineHeight
            };
            var curveRect = new Rect
            {
                x = valueRect.xMax,
                y = labelRect.y,
                width = EditorGUIUtility.currentViewWidth - valueRect.xMax - 18,
                height = EditorGUIUtility.singleLineHeight * 1.5f
            };
            
            EditorGUI.LabelField(labelRect, InitCurveShortName(property));
            EditorGUI.LabelField(valueRect, $"{property.floatValue:F3}");
            EditorGUI.CurveField(curveRect, plotCurve);
            
            EditorGUIUtility.wideMode = wide;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 1.8f * (EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing);
        }
        
        
        private void UpdateBuffer(SerializedProperty property)
        {
            if (currentFrame == Time.frameCount) return;
            currentFrame = Time.frameCount;
            
            if (currentPlotPoint < plotPoints.Length)
                currentPlotPoint++;
            
            for (var i = currentPlotPoint - 1; i >= 1; i--)
                plotPoints[i].value = plotPoints[i - 1].value;
            
            plotPoints[0].value = property.floatValue;
            plotCurve.keys = plotPoints;
        }

        private void InitBuffer()
        {
            if (currentPlotPoint != 0) return;

            plotPoints = new Keyframe[(attribute as StatsCurveAttribute)!.BufferSize];
            for (var i = 0; i < plotPoints.Length; i++)
                plotPoints[i] = new Keyframe(i, 0);
                
            plotCurve.keys = plotPoints;
        }
        
        private bool ShowWarning(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isArray || property.numericType == SerializedPropertyNumericType.Unknown)
            {
                if (!warningHasFired)
                {
                    Debug.LogWarningFormat("Property {0} in {1} cannot be used with {2}. {2} is only valid on numeric properties. Using default property drawer.",
                        property.name, property.serializedObject.targetObject.name, nameof(StatsCurveAttribute));
                    warningHasFired = true;
                }
                EditorGUI.PropertyField(position, property, label);
                return true;
            }

            warningHasFired = false;
            return false;
        }

        private string InitCurveShortName(SerializedProperty property)
        {
            if (string.IsNullOrEmpty(curveShortName) || !property.displayName.StartsWith(curveShortName))
                curveShortName = property.displayName.Replace("Curve", "", StringComparison.OrdinalIgnoreCase).TrimEnd();
            return curveShortName;
        }
    }
}