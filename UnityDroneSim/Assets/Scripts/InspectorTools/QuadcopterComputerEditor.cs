using System;
using Drone;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>Custom editor for <see cref="QuadcopterComputer"/> component.</summary>
    [CustomEditor(typeof(QuadcopterComputer))]
    public class QuadcopterComputerEditor: Editor
    {
        private bool areInternalValuesVisible;
        private bool areMotorsVisible;
        private bool areControlMultipliersVisible;

        private float GROUPS_SPACING = 8;

        private void OnEnable()
        {
            GROUPS_SPACING = EditorGUIUtility.standardVerticalSpacing * 3;
        }

        public override void OnInspectorGUI()
        {
            DrawBaseSettings();
            DrawMotors();
            DrawInternalValues((QuadcopterComputer)target);
            
                serializedObject.ApplyModifiedProperties();
        }

        private void DrawBaseSettings()
        {
            EditorGUILayout.LabelField("Main settings", EditorGUIHelper.Bold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.rigidBody)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.controlSettings)));

            EditorGUILayout.Space(GROUPS_SPACING * 2);
            
            EditorGUILayout.LabelField("Features", EditorGUIHelper.Bold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.clampNegativeForce)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.showForceVectors)));

            EditorGUILayout.Space(GROUPS_SPACING);
        }

        private void DrawMotors()
        {
            var motorFL = serializedObject.FindProperty(nameof(QuadcopterComputer.motorFrontLeft));
            var motorFR = serializedObject.FindProperty(nameof(QuadcopterComputer.motorFrontRight));
            var motorRL = serializedObject.FindProperty(nameof(QuadcopterComputer.motorRearLeft));
            var motorRR = serializedObject.FindProperty(nameof(QuadcopterComputer.motorRearRight));
           
            var assignedMotors = 0;
            if (motorFL.objectReferenceInstanceIDValue != 0) assignedMotors++;
            if (motorFR.objectReferenceInstanceIDValue != 0) assignedMotors++;
            if (motorRL.objectReferenceInstanceIDValue != 0) assignedMotors++;
            if (motorRR.objectReferenceInstanceIDValue != 0) assignedMotors++;

            areMotorsVisible = EditorGUILayout.BeginFoldoutHeaderGroup(areMotorsVisible, $"Motors ({assignedMotors}/4)");
            if (areMotorsVisible)
            {
                EditorGUILayout.PropertyField(motorFL);
                EditorGUILayout.PropertyField(motorFR);
                EditorGUILayout.PropertyField(motorRL);
                EditorGUILayout.PropertyField(motorRR);
                EditorGUILayout.Space(GROUPS_SPACING);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawInternalValues(QuadcopterComputer script)
        {
            EditorGUILayout.Space(GROUPS_SPACING);
            
            areInternalValuesVisible = EditorGUILayout.BeginFoldoutHeaderGroup(areInternalValuesVisible, "Internal Values (read only)");
            if (!areInternalValuesVisible)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }
            
            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            var front = script.motorFrontLeft.liftForce + script.motorFrontRight.liftForce;
            var rear = script.motorRearLeft.liftForce + script.motorRearRight.liftForce;
            var left = script.motorFrontLeft.liftForce + script.motorRearLeft.liftForce;
            var right = script.motorFrontRight.liftForce + script.motorRearRight.liftForce;
            var frontRearError = front - rear;
            var leftRightError = left - right;
            var sum = front + rear;
            
            EditorGUILayout.Space(GROUPS_SPACING);
            EditorGUILayout.LabelField($"Motors force values (sum {sum:F3})", EditorGUIHelper.Bold);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    DrawMotorForce("Front-Left", script.motorFrontLeft.liftForce);
                    DrawMotorForce("Rear-Left", script.motorRearLeft.liftForce);
                    EditorGUILayout.Space(2);
                    DrawMotorForceColored("Front/Rear err.", frontRearError);
                }
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    DrawMotorForce("Front-Right", script.motorFrontRight.liftForce);
                    DrawMotorForce("Rear-Right", script.motorRearRight.liftForce);
                    EditorGUILayout.Space(2);
                    DrawMotorForceColored("Left/Right err.", leftRightError);
                }
            }   
            
            GUI.enabled = enabled;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawMotorForce(string motorName, float value)
        { 
            GUILayout.Label(motorName, GUILayout.ExpandWidth(true));
            EditorGUILayout.FloatField(value, GUILayout.ExpandWidth(true));
        }
        
        private static void DrawMotorForceColored(string motorName, float value)
        { 
            var style = value switch
            {
                0   => GUI.skin.textField,
                > 0 => new GUIStyle(GUI.skin.textField) { normal = new GUIStyleState { textColor = Color.green } },
                _   => new GUIStyle(GUI.skin.textField) { normal = new GUIStyleState { textColor = Color.red } }
            }; 
            GUILayout.Label(motorName, GUILayout.ExpandWidth(true));
            EditorGUILayout.FloatField(value, style, GUILayout.ExpandWidth(true));
        }
    }
}