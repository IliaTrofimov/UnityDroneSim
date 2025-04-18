using System;
using RL.RewardsSettings;
using Unity.Mathematics;
using UnityEngine;


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

        public bool    IsNearObstacle   { get; private set; }
        public float   ObstacleDistance { get; private set; }
        public Vector3 ObstaclePosition { get; private set; }

        public ObstaclePenaltyProvider(ObstaclePenaltySettings settings, Rigidbody agentRigidBody)
        {
            _settings = settings ??
                        throw new ArgumentNullException(nameof(settings),
                            $"Cannot create {nameof(ObstaclePenaltySettings)} without {nameof(WaypointRewardSettings)} parameter."
                        );

            _agentRigidBody = agentRigidBody;
            _collisionLayerMask = LayerMask.GetMask("Default");
        }


        public override float CalculateReward()
        {
            var position = _agentRigidBody.position;
            var speed = _agentRigidBody.linearVelocity.magnitude;
            var nearRadius = math.clamp(speed * Time.fixedDeltaTime, 0, _settings.freeSpaceRadius);

            if (Physics.CheckSphere(position, nearRadius, _collisionLayerMask))
            {
                IsNearObstacle = true;
                ObstacleDistance = _settings.freeSpaceRadius;
                ObstaclePosition = new Vector3(float.NaN, float.NaN, float.NaN);
                return UpdateRewards(_settings.nearObstaclePenalty);
            }

            IsNearObstacle = false;
            var ray = new Ray(position, _agentRigidBody.linearVelocity);
            var maxDistance = _settings.obstacleDetectionRange;
            if (_settings.reactionTime > 0)
                maxDistance = math.min(maxDistance, speed * _settings.reactionTime);

            if (Physics.SphereCast(ray, _settings.freeSpaceRadius, out var hit, maxDistance, _collisionLayerMask))
            {
                ObstaclePosition = hit.point;
                ObstacleDistance = hit.distance;
                return UpdateRewards(_settings.nearObstaclePenalty / (1 + ObstacleDistance));
            }

            ObstaclePosition = new Vector3(float.NaN, float.NaN, float.NaN);
            ObstacleDistance = -1;
            return UpdateRewards(0);
        }
    }
}