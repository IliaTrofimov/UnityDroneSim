using Drone;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>Custom editor for <see cref="DroneStateManager"/> component.</summary>
    [CustomEditor(typeof(DroneStateManager))]
    public class DroneDestructionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (DroneStateManager)target;
            DrawDefaultInspector();

            GUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Repair all motors"))
                    script.RepairAllMotors();

                if (GUILayout.Button("Break all motors"))
                    script.BreakAllMotors();
            }
        }
    }
}