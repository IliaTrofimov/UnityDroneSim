using Navigation;
using Unity.Mathematics;
using UnityEngine;
using UtilsDebug;


namespace RL
{
    public class ObstacleTracker : MonoBehaviour
    {
        private Rigidbody droneRigidBody;
        private WaypointNavigator navigator;
        private float freeSpaceRadius, obstacleDetectionDistance;
        private int layerMask;
        private Vector3 nextObstacle;
        
        private readonly GizmoOptions obstacleRayGizmo = new()
        {
            color = Color.red,
            labelColor = Color.red,
            labelPlacement = GizmoLabelPlacement.Center,
            labelStyle = FontStyle.Italic,
            capSize = 0
        };
        private readonly GizmoOptions obstacleGizmo = new()
        {
            color = Color.red,
            labelColor = Color.red,
            labelPlacement = GizmoLabelPlacement.Center,
            labelStyle = FontStyle.Italic,
            capSize = 0.1f
        };
        private readonly GizmoOptions waypointRayGizmo = new()
        {
            color = Color.green,
            labelColor = Color.green,
            labelPlacement = GizmoLabelPlacement.Center,
            labelStyle = FontStyle.Bold,
            capSize = 0
        };
        private readonly GizmoOptions waypointGizmo = new()
        {
            color = Color.green,
            labelColor = Color.green,
            labelPlacement = GizmoLabelPlacement.Center,
            labelStyle = FontStyle.Bold,
            capSize = 0.1f
        };
       
        
        public float NextObstacleDistance { get; private set; }
        public bool NearObstacle { get; private set; }
        
        public void Initialize(Rigidbody droneRigidBody, WaypointNavigator navigator, float freeSpaceRadius, float obstacleDetectionDistance)
        {
            this.droneRigidBody = droneRigidBody;  
            this.navigator = navigator;
            this.freeSpaceRadius = freeSpaceRadius;
            this.obstacleDetectionDistance = obstacleDetectionDistance;
       
            layerMask = LayerMask.GetMask("Default");
        } 

        private void FixedUpdate()
        {
            var dR = droneRigidBody.linearVelocity.magnitude;
            var nearestObstacle = math.clamp(dR, 0, freeSpaceRadius);
            
            if (Physics.CheckSphere(droneRigidBody.position, nearestObstacle, layerMask))
            {
                NearObstacle = true;
                NextObstacleDistance = freeSpaceRadius;
                return;
            }
            
            NearObstacle = false;
            var ray = new Ray(droneRigidBody.position, droneRigidBody.linearVelocity);
           
            if (Physics.SphereCast(ray, freeSpaceRadius, out var hit, obstacleDetectionDistance, layerMask))
            {
                nextObstacle = hit.point;
                NextObstacleDistance = hit.distance;
            }
            else
            {
                nextObstacle = new Vector3(float.NaN, float.NaN, float.NaN);
                NextObstacleDistance = -1;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (NearObstacle)
            {
                var color = Gizmos.color;
                Gizmos.color = obstacleRayGizmo.color;
                Gizmos.DrawWireSphere(droneRigidBody.position, freeSpaceRadius);
                Gizmos.color = color;
            }
            else if (NextObstacleDistance >= 0)
            {
                VectorDrawer.DrawLine(droneRigidBody.position, nextObstacle, $"{NextObstacleDistance:F2}", obstacleRayGizmo);
                VectorDrawer.DrawPoint(nextObstacle, "", obstacleGizmo);
            }

            if (!navigator.IsFinished)
            {
                var waypointDistance = (navigator.CurrentWaypoint.position - droneRigidBody.position).magnitude;
                VectorDrawer.DrawLine(droneRigidBody.position, navigator.CurrentWaypoint.position, $"{waypointDistance:F2}", waypointRayGizmo);
                VectorDrawer.DrawPoint(navigator.CurrentWaypoint.position, "", waypointGizmo);   
            }
        }
    }
}