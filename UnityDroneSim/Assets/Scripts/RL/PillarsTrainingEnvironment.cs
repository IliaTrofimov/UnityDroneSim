using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UtilsDebug;
using Random = UnityEngine.Random;


namespace RL
{
    public class PillarsTrainingEnvironment : DroneTrainingManager
    {
        [Header("Procedural generation")]
        [Tooltip("Waypoints will have random position (except final waypoint).")]
        public bool enableRandomness = true;
        
        [Tooltip("Min X coordinate for procedural waypoint.")]
        [Range(-50, 50)]
        public float waypointMinX;
        
        [Tooltip("Max X coordinate for procedural waypoint.")]
        [Range(-50, 50)]
        public float waypointMaxX;
        
        [Tooltip("Min Y coordinate for procedural waypoint.")]
        [Range(0, 20)]
        public float waypointMinY;
        
        [Tooltip("Max Y coordinate for procedural waypoint.")]
        [Range(0, 20)]
        public float waypointMaxY;
        
        [Tooltip("Min Z coordinate for procedural waypoint.")]
        [Range(-20, 20)]
        public float waypointMinZ;
        
        [Tooltip("Max Z coordinate for procedural waypoint.")]
        [Range(-20, 20)]
        public float waypointMaxZ;
        
        [Tooltip("Waypoints with this indices will have fixed positions.")]
        public List<int> fixedWaypointIndices;
        
        protected override IEnumerator ResetEnvironment()
        {
            if (!enableRandomness || !path || path.WaypointsCount <= 0)
                yield break;
            
            for (int i = 0; i < path.WaypointsCount - 1; i++)
            {
                if (fixedWaypointIndices.Contains(i)) 
                    continue;
                    
                path.Waypoints[i].position = new Vector3
                {
                    x = Random.Range(waypointMinX, waypointMaxX),
                    y = Random.Range(waypointMinY, waypointMaxY),
                    z = Random.Range(waypointMinZ, waypointMaxZ)
                };   
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!enableRandomness || !enabled) return;
            
            var center = new Vector3
            {
                x = (waypointMaxX + waypointMinX) / 2,
                y = (waypointMaxY + waypointMinY) / 2,
                z = (waypointMaxZ + waypointMinZ) / 2,
            };
            var size = new Vector3
            {
                x = math.abs(waypointMaxX - waypointMinX),
                y = math.abs(waypointMaxY - waypointMinY),
                z = math.abs(waypointMaxZ - waypointMinZ),
            };

            VectorDrawer.DrawPointCube(center,
                "Procedural waypoints",
                new GizmoOptions(Color.green, 0.2f)
                {
                    LabelSize = 0.5f
                }
            );
            
            if (size != Vector3.zero)
            {
                Handles.color = Color.green;
                Handles.DrawWireCube(center, size);
            }
        }
    }
}