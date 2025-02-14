using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InspectorTools
{
    internal class TableDrawer
    {
        private readonly GUIStyle headerStyle, columnHeaderStyle, rowHeaderStyle, cellStyle;
        private readonly string tableHeader;
        private readonly string[] columnHeaders;
        private readonly List<float[]> cellValues = new();
        private readonly List<string> rowNames = new();
        
        public bool useIndent = true;
        public bool useFloatFields = true;
        public bool useRowLabels = true;
        public bool useColumnHeaders = true;
        public string floatFormat = "{0:F3}";

        public TableDrawer(string tableHeader, string[] columnHeaders,
                           GUIStyle headerStyle = null,
                           GUIStyle columnHeaderStyle = null, 
                           GUIStyle rowHeaderStyle = null,
                           GUIStyle cellStyle = null)
        {
            this.headerStyle = headerStyle ?? new GUIStyle("label") { fontStyle = FontStyle.Bold };
            this.rowHeaderStyle = rowHeaderStyle ?? new GUIStyle("label")
            {
                fontStyle = FontStyle.Italic,
                border = new(0, 1, 0, 0)
            };
            this.columnHeaderStyle = columnHeaderStyle ?? new GUIStyle("label")
            {
                alignment = TextAnchor.MiddleCenter,
                border = new(0, 0 ,0, 1)
            };
            this.cellStyle = cellStyle ?? new GUIStyle("label");
            this.columnHeaders = columnHeaders;            
            this.tableHeader = tableHeader;
        }

        public void Row(string rowLabel, params float[] values)
        {
            rowNames.Add(rowLabel);
            cellValues.Add(values);
        }
        
        public void Row(params float[] values)
        {
            rowNames.Add("-/-");
            cellValues.Add(values);
        }

        public void Draw()
        {
            if (!string.IsNullOrEmpty(tableHeader))
                GUILayout.Label(tableHeader, headerStyle);
            
            var enabled = GUI.enabled;
            GUI.enabled = false;
            
            using var h = new EditorGUILayout.HorizontalScope();

            if (useRowLabels)
            {
                using var v = new EditorGUILayout.VerticalScope();

                if (useColumnHeaders)
                    GUILayout.Label("", columnHeaderStyle, GUILayout.ExpandWidth(true));
                
                for (var row = 0; row < rowNames.Count; row++)
                    GUILayout.Label(rowNames[row], rowHeaderStyle, GUILayout.ExpandWidth(true));
            }

            for (var col = 0; col < columnHeaders.Length; col++)
            {
                using var v = new EditorGUILayout.VerticalScope();
                
                if (useColumnHeaders)
                    GUILayout.Label(columnHeaders[col], columnHeaderStyle, GUILayout.ExpandWidth(false));
                
                for (var row = 0; row < cellValues.Count; row++)
                {
                    bool hasValue = cellValues[row].Length > col;
                    if (useFloatFields && hasValue)
                    {
                        EditorGUILayout.FloatField(cellValues[row][col], cellStyle, GUILayout.ExpandWidth(false));
                    }
                    else if (useFloatFields && !hasValue)
                    {
                        EditorGUILayout.FloatField(float.NaN, cellStyle, GUILayout.ExpandWidth(false));
                    }
                    else if (!hasValue)
                    {
                        GUILayout.Label(string.Format(floatFormat, cellValues[row][col]), cellStyle, GUILayout.ExpandWidth(false));
                    }
                    else
                    {
                        GUILayout.Label("-/-", cellStyle, GUILayout.ExpandWidth(false));
                    }
                }
            }

            GUI.enabled = enabled;
        }
    }
}