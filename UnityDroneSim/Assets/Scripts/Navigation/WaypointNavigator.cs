using System.Collections.Generic;
using Drone;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


namespace Navigation
{
    /// <summary>
    /// Waypoint navigation manager. Uses <see cref="WaypointPath"/> object as path.
    /// Keeps track of visited and unvisited waypoints.
    /// </summary>
    [DisallowMultipleComponent]
    public class WaypointNavigator : MonoBehaviour
    {
        private readonly List<GameObject> _waypointsObjects = new();
        private          GameObject       _waypointsParent;
        private          LineRenderer     _pathRenderer;


        public Waypoint? CurrentWaypoint =>
            WaypointsCount > 0 && !IsFinished
                ? path[CurrentWaypointIndex]
                : null;

        public int CurrentWaypointIndex { get; private set; }

        public int  WaypointsCount => path?.WaypointsCount ?? 0;
        public bool IsLoopPath     => isLoopPath;
        public bool IsFinished     => CurrentWaypointIndex >= WaypointsCount;


        [Header("Target")]
        [Tooltip("Drone object. Navigator will track its position and change current active waypoint accordingly.")]
        public DroneComputerBase drone;

        [Header("Path Options")]
        [SerializeField]
        [Tooltip("Path object. Several navigators can use same the path. " +
                 "Path itself is never changing, navigator only changes current active waypoint for its target drone.")]
        private WaypointPath path;

        [SerializeField]
        [Tooltip("Path will be repeated after visiting last waypoint.")]
        private bool isLoopPath;


        [Header("Visuals")]
        [SerializeField]
        [Tooltip("Waypoint object. Leave empty if you don't want to render it.")]
        private GameObject waypointPrefab;

        [SerializeField]
        [Tooltip("Material for line between waypoints. Leave empty if you don't want to render it.")]
        private Material pathMaterial;

        [Range(0, 1)]
        [SerializeField]
        [Tooltip("Line between waypoints will have such width.")]
        private float pathWidth = 0.5f;

        [SerializeField]
        [Tooltip("Will show/hide already visited waypoints.")]
        private bool showPreviousWaypoints;

        [Min(0)]
        [SerializeField]
        [Tooltip("Will show N waypoints ahead.")]
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
            if (!enabled || IsFinished || WaypointsCount == 0) 
                return;

            if (path.Waypoints[CurrentWaypointIndex].ComparePosition(drone.transform, out var distance)) 
                NextWaypoint();
        }

        private void InstantiateWaypointObjects()
        {
            if (!waypointPrefab || !path || !drone)
                return;

            if (!_waypointsParent)
            {
                _waypointsParent = new GameObject($"[Waypoints] {path.name}");
                _waypointsParent.transform.SetParent(drone.transform.parent);
            }

            if (pathMaterial)
            {
                _pathRenderer ??= _waypointsParent.AddComponent<LineRenderer>();
                _pathRenderer.startWidth = pathWidth;
                _pathRenderer.endWidth = pathWidth;
                _pathRenderer.material = pathMaterial;
                _pathRenderer.numCornerVertices = 4;
                _pathRenderer.numCapVertices = 5;
                _pathRenderer.shadowCastingMode = ShadowCastingMode.Off;
                _pathRenderer.positionCount = path.WaypointsCount;
            }

            for (var i = 0; i < path.WaypointsCount; i++)
            {
                var waypoint = path.Waypoints[i];

                var waypointObject = Instantiate(waypointPrefab,
                    waypoint.position,
                    Quaternion.identity,
                    _waypointsParent.transform
                );

                waypointObject.name = string.IsNullOrEmpty(waypoint.name)
                    ? $"[Waypoint] {path.name} ({i + 1}/{path.WaypointsCount})"
                    : $"[Waypoint] {path.name} ({i + 1}/{path.WaypointsCount}): {waypoint.name}";

                _waypointsObjects.Add(waypointObject);
            }

            if (_pathRenderer)
            {
                for (var i = 0; i < path.WaypointsCount; i++)
                    _pathRenderer.SetPosition(i, path[i].position);
            }
        }

        private void UpdateWaypointObjects()
        {
            if (!path || !_pathRenderer) return;

            var start = showPreviousWaypoints ? 0 : CurrentWaypointIndex;
            var end = showNextWaypoints > 0 ? CurrentWaypointIndex + showNextWaypoints : path.WaypointsCount;
            start = math.max(0, start);
            end = math.min(path.WaypointsCount, end);

            for (var i = 0; i < path.WaypointsCount; i++)
                _waypointsObjects[i].SetActive(i >= start && i < end);
        }

        private void DestroyWaypointObjects()
        {
            if (!_waypointsParent && _waypointsObjects.Count == 0) return;

            Destroy(_waypointsParent);
            Destroy(_pathRenderer);

            foreach (var waypointsObject in _waypointsObjects)
                Destroy(waypointsObject);

            _waypointsObjects.Clear();
        }


        public bool NextWaypoint(out Waypoint waypoint)
        {
            if (!path || !enabled)
            {
                waypoint = default;
                return false;
            }

            if (!isLoopPath && CurrentWaypointIndex == path.WaypointsCount - 1)
            {
                waypoint = CurrentWaypoint.Value;
                CurrentWaypointIndex = path.WaypointsCount;
                return false;
            }

            if (isLoopPath && CurrentWaypointIndex == WaypointsCount - 1)
                CurrentWaypointIndex = 0;
            else
                CurrentWaypointIndex++;

            waypoint = path.Waypoints[CurrentWaypointIndex];

            UpdateWaypointObjects();
            return true;
        }

        public bool NextWaypoint() => NextWaypoint(out _);

        public void ResetWaypoint() => CurrentWaypointIndex = 0;

        public float GetCurrentDistance(Vector3 position)
        {
            if (!CurrentWaypoint.HasValue) return 0;

            return Vector3.Distance(position, CurrentWaypoint.Value.position);
        }
    }
}