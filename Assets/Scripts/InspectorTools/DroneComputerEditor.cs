using System;
using RPY_PID_Control;
using UnityEditor;
using UnityEngine;
using Utils;


namespace InspectorTools
{
    /// <summary>Custom editor for <see cref="QuadcopterComputer"/> component.</summary>
    [CustomEditor(typeof(QuadcopterComputer))]
    public class DroneComputerEditor : Editor
    {
        private bool isVisible;

        public override void OnInspectorGUI() 
        {
            var script = (QuadcopterComputer)target;
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            isVisible = EditorGUILayout.BeginFoldoutHeaderGroup(isVisible, "Internal values");
            if (!isVisible)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }
            
            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            EditorGUILayout.LabelField("Correction values", EditorGUIHelper.Bold);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIHelper.VerticalLabel("Pitch corr.", script.pitchCorrection);
                EditorGUIHelper.VerticalLabel("Yaw corr.", script.yawCorrection);
                EditorGUIHelper.VerticalLabel("Roll corr.", script.rollCorrection);
                EditorGUIHelper.VerticalLabel("Throt. corr.", script.throttleCorrection);
            }   
            
            EditorGUILayout.Space(10);
            var sumForce = script.motorFrontLeft.totalForce + script.motorFrontRight.totalForce + 
                           script.motorRearLeft.totalForce + script.motorRearRight.totalForce;
            EditorGUILayout.LabelField($"Motors force values (sum {sumForce:F3})", EditorGUIHelper.Bold);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    DrawMotorForce("Front-Left",script.motorFrontLeft.totalForce);
                    DrawMotorForce("Rear-Left",script.motorRearLeft.totalForce );
                }
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    DrawMotorForce("Front-Right",script.motorFrontRight.totalForce);
                    DrawMotorForce("Rear-Right",script.motorRearRight.totalForce );
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
    }
}