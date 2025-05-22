using System;
using Drone;
using Drone.Stability;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    [CustomEditor(typeof(QuadcopterComputer))]
    public class QuadcopterComputerEditor : Editor
    {
        private DroneComputerBase _drone;
        private bool _pidsAreVisible;
        private void OnEnable()
        {//sss
            _drone = (DroneComputerBase)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var canDisplay = _drone.PidThrottle is DebugPidController &&
                             _drone.PidPitch is DebugPidController &&
                             _drone.PidYaw is DebugPidController &&
                             _drone.PidRoll is DebugPidController;
            if (!canDisplay) return;
            
            _pidsAreVisible = EditorGUILayout.BeginFoldoutHeaderGroup(_pidsAreVisible, "PID controllers");
            if (!_pidsAreVisible)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            EditorGUILayout.LabelField(nameof(_drone.PidThrottle), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pidThrottle"));
            
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(nameof(_drone.PidPitch), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pidPitch"));
            
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(nameof(_drone.PidYaw), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pidYaw"));
            
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(nameof(_drone.PidRoll), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pidRoll"));
            
            GUI.enabled = enabled;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}