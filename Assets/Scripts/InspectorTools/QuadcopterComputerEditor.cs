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
        private bool isStabilizationVisible;
        
        private const int GROUPS_SPACING = 3;
        
        public override void OnInspectorGUI() 
        {
            EditorGUILayout.LabelField("Physics settings", EditorGUIHelper.Bold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.rigidBody)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.clampNegativeForce)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.balanceCenterOfMass)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.showForceVectors)));

            EditorGUILayout.Space(GROUPS_SPACING);
            
            DrawForceMultipliers();
            DrawPidControllers();
            DrawMotors();
            DrawInternalValues((QuadcopterComputer)target);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawForceMultipliers()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.controlParams)));
            EditorGUILayout.Space(GROUPS_SPACING);
        }
        
        private void DrawPidControllers()
        {
            isStabilizationVisible = EditorGUILayout.BeginFoldoutHeaderGroup(isStabilizationVisible, "Stabilization");
            
            if (isStabilizationVisible)
            {
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.pidThrottle)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.pidPitch)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.pidYaw)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.pidRoll)));
                
                EditorGUI.indentLevel = indent;
                EditorGUILayout.Space(GROUPS_SPACING);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawMotors()
        {
            areMotorsVisible = EditorGUILayout.BeginFoldoutHeaderGroup(areMotorsVisible, "Motors");
            if (areMotorsVisible)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.motorFrontLeft)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.motorFrontRight)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.motorRearLeft)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuadcopterComputer.motorRearRight)));
                EditorGUILayout.Space(GROUPS_SPACING);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private void DrawInternalValues(QuadcopterComputer script)
        {
            EditorGUILayout.Space(GROUPS_SPACING);
            
            areInternalValuesVisible = EditorGUILayout.BeginFoldoutHeaderGroup(areInternalValuesVisible, "Internal values (read only)");
            if (!areInternalValuesVisible)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }
            
            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            EditorGUILayout.LabelField("Correction values", EditorGUIHelper.Bold);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIHelper.VerticalLabel("Throt. corr.", script.throttleCorrection);
                EditorGUIHelper.VerticalLabel("Pitch corr.", script.pitchCorrection);
                EditorGUIHelper.VerticalLabel("Yaw corr.", script.yawCorrection);
                EditorGUIHelper.VerticalLabel("Roll corr.", script.rollCorrection);
            }   
            
            EditorGUILayout.Vector3Field($"Result torque {script.torqueVector.magnitude:F3}", script.torqueVector);
            
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