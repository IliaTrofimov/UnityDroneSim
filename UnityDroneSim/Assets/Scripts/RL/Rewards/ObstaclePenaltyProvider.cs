using System;
using RL.RewardsSettings;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UtilsDebug;


namespace RL.Rewards
{
    /// <summary>
    /// Calculates rewards and penalties for moving near obstacles.
    /// </summary>
    public class ObstaclePenaltyProvider : RewardProvider
    {
        private readonly ObstaclePenaltySettings _settings;
        private readonly Rigidbody               _agentRigidBody;
        private readonly int                     _collisionLayerMask;

        private GizmoOptions _debugGizmo = new (Color.red, capSize: 0)
        {
            LabelPlacement = GizmoLabelPlacement.Center,
            LabelSize = 0.75f,
            VectCapSize = 0.5f
        };
        
        public bool IsNearObstacle   { get; private set; }
        public float ObstacleDistance { get; private set; }
        public Vector3 ObstaclePosition { get; private set; }

        
        public ObstaclePenaltyProvider(ObstaclePenaltySettings settings, Rigidbody agentRigidBody)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _agentRigidBody = agentRigidBody ?? throw new ArgumentNullException(nameof(agentRigidBody));
            _collisionLayerMask = LayerMask.GetMask("Default");
        }


        protected override (float, bool) CalculateRewardInternal()
        {
            var position = _agentRigidBody.position;

            if (Physics.CheckSphere(position, _settings.freeSpaceRadius, _collisionLayerMask))
            {
                IsNearObstacle = true;
                ObstacleDistance = _settings.freeSpaceRadius;
                ObstaclePosition = new Vector3(float.NaN, float.NaN, float.NaN);
                return (_settings.nearObstaclePenalty, false);
            }

            IsNearObstacle = false;
            var ray = new Ray(position, _agentRigidBody.linearVelocity);
            var maxDistance = _settings.obstacleDetectionRange;
            if (_settings.reactionTime > 0)
                maxDistance = math.min(maxDistance, _agentRigidBody.linearVelocity.magnitude * _settings.reactionTime);

            if (Physics.SphereCast(ray, _settings.freeSpaceRadius, out var hit, maxDistance, _collisionLayerMask))
            {
                ObstaclePosition = hit.point;
                ObstacleDistance = hit.distance;
                return (_settings.nearObstaclePenalty / (1 + ObstacleDistance), false);
            }

            ObstaclePosition = new Vector3(float.NaN, float.NaN, float.NaN);
            ObstacleDistance = -1;
            return (0, false);
        }

        public override void DrawGizmos()
        {
            if (IsNearObstacle)
            { 
                var color = Gizmos.color;
                Gizmos.color = new Color(1, 0,0, 0.1f);
                Gizmos.DrawSphere(_agentRigidBody.position, _settings.freeSpaceRadius);
                Gizmos.color = new Color(1, 0,0, 1);
                Gizmos.DrawWireSphere(_agentRigidBody.position, _settings.freeSpaceRadius);
                Gizmos.color = color;
                
            }
            else if (ObstacleDistance > 0)
            {
                VectorDrawer.DrawLine(
                    _agentRigidBody.position,
                    ObstaclePosition, 
                    $"Obst.: {ObstacleDistance:F1} m\nR: {LastReward:F3}",
                    _debugGizmo);
            }
        }
    }
}