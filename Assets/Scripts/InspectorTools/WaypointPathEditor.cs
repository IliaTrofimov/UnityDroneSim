using Navigation;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    [CustomEditor(typeof(WaypointPath))]
    public class WaypointPathEditor : Editor
    {
        private WaypointPath waypointPath;
        private SerializedProperty pathWaypoints;
        
        private void OnEnable()
        {
            waypointPath = target as WaypointPath;
            pathWaypoints = serializedObject.FindProperty("waypoints");
        }
 
        private void OnSceneGUI()
        {
            Undo.RecordObject(waypointPath, "Movement");
            var pathColor = serializedObject.FindProperty("color").colorValue;
 
            var hasChanges = false;
            for (var i = 0; i < waypointPath.WaypointsCount; i++)
            {
                var waypoint = pathWaypoints.GetArrayElementAtIndex(i);
                var waypointPos = waypoint.FindPropertyRelative("position");
                var waypointRad = waypoint.FindPropertyRelative("radius");
                
                var colorOld = Handles.color;
                Handles.color = pathColor;
                
                var newRad = Handles.RadiusHandle(Quaternion.identity, waypointPos.vector3Value, waypointRad.floatValue, false);
                var posNew = Handles.PositionHandle(waypointPos.vector3Value, Quaternion.identity);
                
                if (posNew != waypointPos.vector3Value)
                {
                    waypointPos.vector3Value = posNew;
                    hasChanges = true;
                }
                if (newRad != waypointRad.floatValue)
                {
                    waypointRad.floatValue = newRad;
                    hasChanges = true;
                }

                Handles.color = colorOld;
            }
            
            if (hasChanges)
                pathWaypoints.serializedObject.ApplyModifiedProperties();
        }
    }
}