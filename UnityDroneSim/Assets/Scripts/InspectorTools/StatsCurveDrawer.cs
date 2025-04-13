using System;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>
    /// Marks property or field that it should be stored over time and displayed in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class StatsCurveAttribute : PropertyAttribute
    {
        /// <summary>Max values count to store and display.</summary>
        public int BufferSize { get; set; }

        public StatsCurveAttribute(int bufferSize = 20) { BufferSize = bufferSize; }
    }


    [CustomPropertyDrawer(typeof(StatsCurveAttribute))]
    public class StatsCurveDrawer : PropertyDrawer
    {
        private          bool           _warningHasFired;
        private          string         _curveShortName;
        private readonly AnimationCurve _plotCurve = new();
        private          Keyframe[]     _plotPoints;
        private          int            _currentPlotPoint, _currentFrame;

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
                width = EditorGUIUtility.labelWidth,
                height = EditorGUIUtility.singleLineHeight
            };

            var curveRect = new Rect
            {
                x = labelRect.xMax,
                y = labelRect.y,
                width = EditorGUIUtility.currentViewWidth - labelRect.xMax - 18,
                height = EditorGUIUtility.singleLineHeight * 1.5f
            };

            EditorGUI.LabelField(labelRect, InitCurveShortName(property) + ": " + property.floatValue);
            EditorGUI.CurveField(curveRect, _plotCurve);

            EditorGUIUtility.wideMode = wide;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            1.8f * (EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing);


        private void UpdateBuffer(SerializedProperty property)
        {
            if (_currentFrame == Time.frameCount) return;

            _currentFrame = Time.frameCount;

            if (_currentPlotPoint < _plotPoints.Length)
                _currentPlotPoint++;

            for (var i = _currentPlotPoint - 1; i >= 1; i--)
                _plotPoints[i].value = _plotPoints[i - 1].value;

            _plotPoints[0].value = property.floatValue;
            _plotCurve.keys = _plotPoints;
        }

        private void InitBuffer()
        {
            if (_currentPlotPoint != 0) return;

            _plotPoints = new Keyframe[(attribute as StatsCurveAttribute)!.BufferSize];
            for (var i = 0; i < _plotPoints.Length; i++)
                _plotPoints[i] = new Keyframe(i, 0);

            _plotCurve.keys = _plotPoints;
        }

        private bool ShowWarning(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isArray || property.numericType == SerializedPropertyNumericType.Unknown)
            {
                if (!_warningHasFired)
                {
                    Debug.LogWarningFormat(
                        "Property {0} in {1} cannot be used with {2}. {2} is only valid on numeric properties. Using default property drawer.",
                        property.name,
                        property.serializedObject.targetObject.name,
                        nameof(StatsCurveAttribute)
                    );

                    _warningHasFired = true;
                }

                EditorGUI.PropertyField(position, property, label);
                return true;
            }

            _warningHasFired = false;
            return false;
        }

        private string InitCurveShortName(SerializedProperty property)
        {
            if (string.IsNullOrEmpty(_curveShortName) || !property.displayName.StartsWith(_curveShortName))
                _curveShortName = property.displayName.Replace("Curve", "", StringComparison.OrdinalIgnoreCase)
                   .TrimEnd();

            return _curveShortName;
        }
    }
}