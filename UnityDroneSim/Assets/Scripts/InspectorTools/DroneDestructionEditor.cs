using Drone;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>Custom editor for <see cref="DroneDestruction"/> component.</summary>
    [CustomEditor(typeof(DroneDestruction))]
    public class DroneDestructionEditor : Editor
    {
        public override void OnInspectorGUI() 
        {
            var script = (DroneDestruction)target;
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