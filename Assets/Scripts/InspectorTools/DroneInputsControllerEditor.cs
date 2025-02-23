using Drone;
using UnityEditor;
using UnityEngine;
using Utils;


namespace InspectorTools
{
    /// <summary>Custom editor for <see cref="DroneInputsController"/> component.</summary>
    [CustomEditor(typeof(DroneInputsController))]
    public class DroneInputsControllerEditor : Editor
    {
        private bool isVisible;
        
        public override void OnInspectorGUI() 
        {
            var script = (DroneInputsController)target;
            DrawDefaultInspector();

            isVisible = EditorGUILayout.BeginFoldoutHeaderGroup(isVisible, "Input values (read only)");
            if (!isVisible)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }
            
            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIHelper.VerticalLabel("Pitch (X)", script.pitch);
                EditorGUIHelper.VerticalLabel("Yaw (Y)", script.yaw);
                EditorGUIHelper.VerticalLabel("Roll (Z)", script.roll);
                EditorGUIHelper.VerticalLabel("Throt.", script.throttle);
            }
            
            GUI.enabled = enabled;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}