using Drone;
using Exceptions;
using UnityEngine;


namespace Navigation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DroneComputerBase))]
    public class WaypointNavigator : MonoBehaviour
    {
        private DroneComputerBase drone;
        
        public WaypointPath path;
        private bool isLoopPath;
        private int currentWaypointIndex;
        
        public Waypoint CurrentWaypoint => path[currentWaypointIndex];
        public int CurrentWaypointIndex => currentWaypointIndex;
        public int WaypointsCount => path.WaypointsCount;
        public bool IsLoopPath => isLoopPath;
        public bool IsFinished => currentWaypointIndex >= WaypointsCount;
        
        
        private void Awake()
        {
            drone = GetComponent<DroneComputerBase>();
            ExceptionHelper.ThrowIfComponentIsMissing(this, path, nameof(path));
        }

        private void FixedUpdate()
        {
            if (!path || !enabled || IsFinished) return;
            
            if (path.Waypoints[currentWaypointIndex].ComparePosition(drone.transform, out var distance))
                NextWaypoint();
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
            return true;
        }
        
        public bool NextWaypoint() => NextWaypoint(out _);

        public void ResetWaypoint() => currentWaypointIndex = 0;
    }
}