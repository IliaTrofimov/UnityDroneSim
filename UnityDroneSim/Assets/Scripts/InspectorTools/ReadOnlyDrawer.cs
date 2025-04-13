using System;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>
    /// Makes field non-editable (appears as disabled).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ReadOnlyFieldAttribute : PropertyAttribute
    {
    }


    /// <summary>
    /// This class contain custom drawer for ReadOnlyField attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var previousGUIState = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = previousGUIState;
        }
    }
}