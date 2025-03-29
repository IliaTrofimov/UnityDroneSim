using System;
using System.Collections.Generic;
using UnityEngine;
using UtilsDebug;


namespace Navigation
{
    public class WaypointPath : MonoBehaviour
    {
        [SerializeField]
        public Color color = Color.green;
        
        [SerializeField]
        private List<Waypoint> waypoints = new();
        
        public IReadOnlyList<Waypoint> Waypoints => waypoints;
        public Waypoint this[int index] => waypoints[index];
        public int WaypointsCount => waypoints.Count;


        private void OnDrawGizmosSelected()
        {
            if (waypoints.Count == 0 || !enabled) return;
            
            var gizmoOptions = new GizmoOptions(color)
            {
                capSize = 0.1f,
                labelColor = color,
                labelPlacement = GizmoLabelPlacement.End,
                labelOutline = true
            };

            for (var i = 0; i < waypoints.Count - 1; i++)
                VectorDrawer.DrawLine(waypoints[i].position, waypoints[i + 1].position, GetWaypointLabel(i), gizmoOptions);
            
            VectorDrawer.DrawLabel(waypoints[^1].position, GetWaypointLabel(waypoints.Count - 1), gizmoOptions);
        }

        private string GetWaypointLabel(int index)
        {
            return string.IsNullOrEmpty(waypoints[index].name)
                ? $"{name} [{index + 1}/{waypoints.Count}]"
                : $"{name} [{index + 1}/{waypoints.Count}]: {waypoints[index].name}";
        } 
    }
}