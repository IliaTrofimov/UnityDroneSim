using RL;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>Custom editor for <see cref="DepthCameraHelper"/> component.</summary>
    [CustomEditor(typeof(DepthCameraHelper))]
    public class DepthCameraHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var enabled = GUI.enabled;
            GUI.enabled = !Application.isPlaying;
            DrawDefaultInspector();
            GUI.enabled = enabled;
        }
    }
}