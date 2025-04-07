using System.Collections.Generic;
using Drone;
using Exceptions;
using Unity.Mathematics;
using UnityEngine;


namespace Navigation
{
    /// <summary>
    /// Waypoint navigation manager. Uses <see cref="WaypointPath"/> object as path.
    /// Keeps track of visited and unvisited waypoints.
    /// </summary>
    [DisallowMultipleComponent]
    public class WaypointNavigator : MonoBehaviour
    {
        private readonly List<GameObject> waypointsObjects = new();
        private GameObject waypointsParent;
        private LineRenderer pathRenderer;
        private int currentWaypointIndex;

        
        public Waypoint CurrentWaypoint => path[currentWaypointIndex];
        public int CurrentWaypointIndex => currentWaypointIndex;
        public int WaypointsCount => path?.WaypointsCount ?? 0;
        public bool IsLoopPath => isLoopPath;
        public bool IsFinished => currentWaypointIndex >= WaypointsCount;
        
        
        [Header("Target")]
        [Tooltip("Drone object. Navigator will track its position and change current active waypoint accordingly.")]
        public DroneComputerBase drone;

        [Header("Path Options")]
        [Tooltip("Path object. Several navigators can use same the path. " +
                 "Path itself is never changing, navigator only changes current active waypoint for its target drone.")]
        [SerializeField] 
        private WaypointPath path;
        
        [Tooltip("Path will be repeated after visiting last waypoint.")]
        [SerializeField] 
        private bool isLoopPath;
        
        
        [Header("Visuals")]
        [Tooltip("Waypoint object. Leave empty if you don't want to render it.")]
        [SerializeField] 
        private GameObject waypointPrefab;
        
        [Tooltip("Material for line between waypoints. Leave empty if you don't want to render it.")]
        [SerializeField] private Material pathMaterial;
        
        [Tooltip("Line between waypoints will have such width.")]
        [SerializeField, Range(0, 1)] 
        private float pathWidth = 0.5f;
        
        [Tooltip("Will show/hide already visited waypoints.")]
        [SerializeField] 
        private bool showPreviousWaypoints;
        
        [Tooltip("Will show N waypoints ahead.")]
        [SerializeField, Min(0)] 
        private int showNextWaypoints;
        

        private void Start()
        {
            DestroyWaypointObjects();
            InstantiateWaypointObjects();
            UpdateWaypointObjects();
        }
        
        private void OnDisable() => DestroyWaypointObjects();

        private void FixedUpdate()
        {
            if (!enabled || IsFinished || WaypointsCount == 0) return;

            if (path.Waypoints[currentWaypointIndex].ComparePosition(drone.transform, out var distance))
            {
                NextWaypoint();
            }
        }

        private void InstantiateWaypointObjects()
        {
            if (!waypointPrefab || !path || !drone) 
                return;
            
            if (!waypointsParent)
            {
                waypointsParent = new GameObject($"[Waypoints] {path.name}");
                waypointsParent.transform.SetParent(drone.transform.parent);
            }

            if (pathMaterial)
            {
                pathRenderer ??= waypointsParent.AddComponent<LineRenderer>();
                pathRenderer.startWidth = pathWidth;
                pathRenderer.endWidth = pathWidth;
                pathRenderer.material = pathMaterial;
                pathRenderer.numCornerVertices = 4;
                pathRenderer.numCapVertices = 5;
                pathRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                pathRenderer.positionCount = path.WaypointsCount;    
            }
            
            for (var i = 0; i < path.WaypointsCount; i++)
            {
                var waypoint = path.Waypoints[i];

                var waypointObject = Instantiate(waypointPrefab, waypoint.position, Quaternion.identity,
                    waypointsParent.transform);

                waypointObject.name = string.IsNullOrEmpty(waypoint.name)
                    ? $"[Waypoint] {path.name} ({i + 1}/{path.WaypointsCount})"
                    : $"[Waypoint] {path.name} ({i + 1}/{path.WaypointsCount}): {waypoint.name}";

                waypointsObjects.Add(waypointObject);
                pathRenderer.SetPosition(i, waypoint.position);
            }
        }
        
        private void UpdateWaypointObjects()
        {
            if (!path || !pathRenderer) return;
            
            var start = showPreviousWaypoints ? 0 : currentWaypointIndex;
            var end = showNextWaypoints > 0 ? currentWaypointIndex + showNextWaypoints : path.WaypointsCount;
            start = math.max(0, start);
            end = math.min(path.WaypointsCount, end);

            for (var i = 0; i < path.WaypointsCount; i++)
                waypointsObjects[i].SetActive(i >= start && i < end);
        }
        
        private void DestroyWaypointObjects()
        {
            if (!waypointsParent && waypointsObjects.Count == 0) return;
            
            Destroy(waypointsParent);
            Destroy(pathRenderer);
            
            foreach (var waypointsObject in waypointsObjects)
                Destroy(waypointsObject);   
            
            waypointsObjects.Clear();
        }
        

        public bool NextWaypoint(out Waypoint waypoint)
        {
            if (!path || !enabled)
            {
                waypoint = default;
                return false;       
            }
            
            if (!isLoopPath && currentWaypointIndex == path.WaypointsCount - 1)
            {
                waypoint = CurrentWaypoint;
                currentWaypointIndex = path.WaypointsCount;
                return false;
            }
            
            if (isLoopPath && currentWaypointIndex == WaypointsCount - 1)
                currentWaypointIndex = 0;
            else
                currentWaypointIndex++;
            
            waypoint = path.Waypoints[currentWaypointIndex];

            UpdateWaypointObjects();    
            return true;
        }
        
        public bool NextWaypoint() => NextWaypoint(out _);

        public void ResetWaypoint() => currentWaypointIndex = 0;

        public float GetCurrentDistance(Vector3 position)
        {
            if (IsFinished || WaypointsCount == 0) return 0;
            return Vector3.Distance(position, CurrentWaypoint.position);
        }
    }
}