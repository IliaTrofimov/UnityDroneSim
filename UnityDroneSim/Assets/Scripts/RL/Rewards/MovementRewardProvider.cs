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
            _agentRigidBody = agentRigidBody;
        }

        public override float CalculateReward()
        {
            var speed = math.abs(_agentRigidBody.linearVelocity.magnitude);
            if (speed > _settings.idleSpeed)
                return UpdateRewards(_settings.movementReward * math.clamp(speed, _settings.idleSpeed, 1));

            return UpdateRewards(_settings.noMovementPenalty * math.clamp(speed, 0, _settings.idleSpeed));
        }
    }
}