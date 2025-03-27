using System.Collections.Generic;
using Drone;
using Exceptions;
using Unity.Mathematics;
using UnityEngine;


namespace Navigation
{
    [DisallowMultipleComponent]
    public class WaypointNavigator : MonoBehaviour
    {
        private List<GameObject> waypointsObjects = new();
        private GameObject waypointsParent;
        private LineRenderer pathRenderer;
        private int currentWaypointIndex;

        [Header("Target")]
        public DroneComputerBase drone;

        [Header("Path Options")]
        [SerializeField] private WaypointPath path;
        [SerializeField] private bool isLoopPath;
        
        [Header("Visuals")]
        [SerializeField] 
        private GameObject waypointPrefab;
        
        [SerializeField] private Material pathMaterial;
        
        [SerializeField, Range(0, 1)] 
        private float pathWidth = 0.5f;
        
        [SerializeField] private bool showPreviousWaypoints;
        
        [SerializeField, Min(0)] 
        private int showNextWaypoints;
        
        public Waypoint CurrentWaypoint => path[currentWaypointIndex];
        public int CurrentWaypointIndex => currentWaypointIndex;
        public int WaypointsCount => path?.WaypointsCount ?? 0;
        public bool IsLoopPath => isLoopPath;
        public bool IsFinished => currentWaypointIndex >= WaypointsCount;
        
        
        private void Awake()
        {
            ExceptionHelper.ThrowIfComponentIsMissing(this, drone, nameof(drone));
        }

        private void OnEnable()
        {
            InstantiateWaypointObjects();
            UpdateWaypointObjects();
        }

        private void OnValidate()
        {
            if (waypointsObjects.Count == 0)
            {
                if (path && drone && waypointPrefab)
                {
                    InstantiateWaypointObjects();
                    UpdateWaypointObjects();
                }
            }
            else
            {
                if (waypointsObjects.Count != path?.WaypointsCount)
                {
                    DestroyWaypointObjects();
                    if (path) InstantiateWaypointObjects();
                }
                if (path && waypointsObjects.Count != path.WaypointsCount)
                {
                    InstantiateWaypointObjects();
                }
                if (path) UpdateWaypointObjects();
            }
        }

        private void OnDisable() => DestroyWaypointObjects();

        private void FixedUpdate()
        {
            if (!path || !enabled || IsFinished) return;

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

            pathRenderer ??= waypointsParent.AddComponent<LineRenderer>();
            pathRenderer.startWidth = pathWidth;
            pathRenderer.endWidth = pathWidth;
            pathRenderer.material = pathMaterial;
            pathRenderer.numCornerVertices = 4;
            pathRenderer.numCapVertices = 5;
            pathRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            pathRenderer.positionCount = path.WaypointsCount;
            
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
            if (!path || !enabled || (currentWaypointIndex >= WaypointsCount && !isLoopPath))
            {
                waypoint = default;
                return false;       
            }
            
            if (currentWaypointIndex == WaypointsCount - 1)
                currentWaypointIndex = 0;
            else
                currentWaypointIndex++;
            
            waypoint = path.Waypoints[currentWaypointIndex];

            UpdateWaypointObjects();    
            
            return true;
        }
        
        public bool NextWaypoint() => NextWaypoint(out _);

        public void ResetWaypoint() => currentWaypointIndex = 0;
    }
}