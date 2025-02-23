using System;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>
    /// Makes field non-editable (appears as disabled).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RealtimeCurveAttribute : PropertyAttribute { }
    
    /// <summary>
    /// This class contain custom drawer for ReadOnlyField attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(RealtimeCurveAttribute))]
    public class RealtimeCurveDrawer : PropertyDrawer
    {
        private bool warningHasFired = false;
        private string curveShortName;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.animationCurveValue == null)
            {
                if (!warningHasFired)
                {
                    Debug.LogWarningFormat("Property {0} in {1} cannot be used with {2}. {2} is only valid on AnimationCurve properties. Using default property drawer.",
                        property.name, property.serializedObject.targetObject.name, nameof(RealtimeCurveAttribute));
                    warningHasFired = true;
                }
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            warningHasFired = false;
            
            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            InitCurveShortName(property);
            
            var curveRect = new Rect(
                position.x, 
                position.y, 
                position.width * 0.85f, 
                position.height);
            
            var valueRect = new Rect(
                position.x + position.width * 0.85f + 1,
                position.y,
                position.width * 0.15f - 2, 
                position.height);
            
            EditorGUI.PropertyField(curveRect, property, new GUIContent(curveShortName));
            EditorGUI.FloatField(valueRect, GetLastCurveValue(property));
            GUI.enabled = enabled;
        }

        private void InitCurveShortName(SerializedProperty property)
        {
            if (string.IsNullOrEmpty(curveShortName) || !property.displayName.StartsWith(curveShortName))
                curveShortName = property.displayName.Replace("Curve", "", StringComparison.OrdinalIgnoreCase).TrimEnd();
        }

        private static float GetLastCurveValue(SerializedProperty property)
        {
            var lastCurveValue = float.NaN;
            if (property.animationCurveValue.length > 0)
                lastCurveValue = property.animationCurveValue[property.animationCurveValue.length - 1].value;

            return lastCurveValue;
        }
    }
}