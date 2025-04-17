using System;
using RL.RewardsSettings;
using Unity.Mathematics;
using UnityEngine;


namespace RL.Rewards
{
    /// <summary>
    /// Calculates simple reward for moving or not moving.
    /// </summary>
    public class MovementRewardProvider : RewardProvider
    {
        private readonly MovementRewardSettings _settings;
        private readonly Rigidbody              _agentRigidBody;

        public MovementRewardProvider(MovementRewardSettings settings, Rigidbody agentRigidBody)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _agentRigidBody = agentRigidBody ?? throw new ArgumentNullException(nameof(_agentRigidBody));
        }

        public override float CalculateReward()
        {
            var speed = math.abs(_agentRigidBody.linearVelocity.magnitude);
            return UpdateRewards(
                speed > _settings.idleSpeed 
                ? _settings.movementReward 
                : _settings.noMovementPenalty
            );
        }
    }
}