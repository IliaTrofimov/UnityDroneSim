using System;
using System.Collections.Generic;
using UnityEngine;
using UtilsDebug;


namespace Navigation
{
    /// <summary>
    /// Path made of several waypoints.
    /// </summary>
    public class WaypointPath : MonoBehaviour
    {
        [Tooltip("Color for waypoints and lines between them. Used only as gizmos.")]
        public Color color = Color.green;

        [Tooltip("Always show waypoints gizmos.")]
        public bool showGizmos = true;
        
        [SerializeField]
        [Tooltip("List of all waypoints. Can be changed only in Inspector.")]
        private List<Waypoint> waypoints = new();
        
        public IReadOnlyList<Waypoint> Waypoints => waypoints;
        public Waypoint this[int index] => waypoints[index];
        public int WaypointsCount => waypoints.Count;


        private void OnDrawGizmos()
        {
            if (showGizmos) DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) DrawGizmos();
        }

        private void DrawGizmos()
        {
            if (waypoints.Count == 0 || !enabled) return;

            var gizmoOptions = new GizmoOptions(color)
            {
                CapSize = 0.2f, 
                LabelColor = color, LabelPlacement = GizmoLabelPlacement.Start, LabelOutline = true,
                LabelSize = 0.7f
            };

            for (var i = 0; i < waypoints.Count - 1; i++)
            {
                VectorDrawer.DrawLine(waypoints[i].position,
                    waypoints[i + 1].position,
                    GetWaypointLabel(i),
                    gizmoOptions
                );
            }

            VectorDrawer.DrawLabel(waypoints[^1].position, GetWaypointLabel(waypoints.Count - 1), gizmoOptions);
        }
        
        private string GetWaypointLabel(int index) =>
            string.IsNullOrWhiteSpace(waypoints[index].name)
                ? $"{name} [{index + 1}/{waypoints.Count}]"
                : $"{name} [{index + 1}/{waypoints.Count}]: {waypoints[index].name}";

        public void AddWaypoint(Waypoint waypoint)
        {
            waypoints.Add(waypoint);
        }
        
        public void AddWaypoint(float x, float y, float z, float radius = 1, string waypointName = "")
        {
            waypoints.Add(new Waypoint(waypointName, radius, new Vector3(x, y, z)));
        }

        [ContextMenu("Clear waypoints")]
        public void ClearWaypoints()
        {
            waypoints.Clear();
        }
    }
}