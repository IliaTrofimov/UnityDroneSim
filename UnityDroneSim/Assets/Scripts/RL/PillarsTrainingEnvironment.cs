using System.Collections;
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
        [Tooltip("Waypoint with given index will have random position each environment epoch.")]
        [Min(-1)]
        public int proceduralWaypointIndex = -1;
        
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
        
        
        protected override IEnumerator ResetEnvironment()
        {
            if (proceduralWaypointIndex < 0)
                yield break;

            if (!path || path.WaypointsCount <= proceduralWaypointIndex)
            {
                Debug.LogErrorFormat("ProceduralRoadEnvironment '{0}': cannot generate Waypoint {1} position.",
                    name, proceduralWaypointIndex);
            }
            else
            {
                var position = new Vector3
                {
                    x = Random.Range(waypointMinX, waypointMaxX),
                    y = Random.Range(waypointMinY, waypointMaxY),
                    z = Random.Range(waypointMinZ, waypointMaxZ)
                };
                path.Waypoints[proceduralWaypointIndex].position = position;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (proceduralWaypointIndex < 0 || !enabled)return;
            
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
                "Procedural waypoint area",
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