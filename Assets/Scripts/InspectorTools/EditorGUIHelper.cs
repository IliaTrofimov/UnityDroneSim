using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    public static class EditorGUIHelper
    {
        public static readonly GUIStyle Bold = new GUIStyle("label") { fontStyle = FontStyle.Bold };
        public static readonly GUIStyle Italic = new GUIStyle("label") { fontStyle = FontStyle.Italic };

        public static void VerticalLabel(string name, float val)
        {
            using var _ = new EditorGUILayout.VerticalScope();
            GUILayout.Label(name);
            EditorGUILayout.FloatField(val);
        }

        public static void VerticalLabel(string name, Vector3 val)
        {
            using var _ = new EditorGUILayout.VerticalScope();
            GUILayout.Label(name);
            EditorGUILayout.Vector3Field(GUIContent.none, val);
        }

        public static void StartTable(string name, string[] columns, GUIStyle headerStyle = default,
                                      GUIStyle columnHeaderStyle = default)
        {
            GUILayout.Label(name, headerStyle);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            using var _ = new EditorGUILayout.HorizontalScope();
            foreach (var col in columns)
                GUILayout.Label(col, columnHeaderStyle, GUILayout.ExpandWidth(false));
        }
    }
}