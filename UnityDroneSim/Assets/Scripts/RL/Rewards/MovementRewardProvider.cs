using System;
using RL.RewardsSettings;
using Unity.Mathematics;
using UnityEngine;
using Utils;


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
            _agentRigidBody = agentRigidBody ?? throw new ArgumentNullException(nameof(agentRigidBody));
        }

        public override float CalculateReward()
        {
            var speed = _agentRigidBody.linearVelocity.magnitude;
            var angularSpeed = math.abs(_agentRigidBody.YawVelocity());

            var reward = speed > _settings.idleLinearSpeed
                ? _settings.movementReward
                : _settings.noMovementPenalty;

            if (angularSpeed > _settings.maxAngularSpeed)
                reward += _settings.angularSpeedPenalty;
           
            return UpdateRewards(reward);
        }
    }
}