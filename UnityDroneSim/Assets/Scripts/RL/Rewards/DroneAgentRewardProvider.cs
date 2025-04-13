using System;
using System.Collections.Generic;
using Drone;
using RL.RewardsSettings;
using UnityEngine;


namespace RL.Rewards
{
    /// <summary>
    /// Composite reward signal for <see cref="DroneAgent"/>.
    /// Uses <see cref="WaypointRewardProvider"/>, <see cref="MovementRewardProvider"/>, <see cref="ObstaclePenaltyProvider"/>
    /// and <see cref="DroneStateRewardProvider"/> for calculating total reward. Also keeps track of episode time.
    /// </summary>
    public class DroneAgentRewardProvider : RewardProvider, ICompositeRewardProvider
    {
        private readonly TrainingSettings         _settings;
        private readonly WaypointRewardProvider   _waypointReward;
        private readonly MovementRewardProvider   _movementReward;
        private readonly ObstaclePenaltyProvider  _obstaclePenalty;
        private readonly DroneStateRewardProvider _stateReward;
        
        private float _startTime;

        public int RewardsCount => 4;

        /// <summary>Time left to force restart of the episode.</summary>
        /// <remarks>Episode length is set int <see cref="TrainingSettings"/>.<see cref="TerminationConditions"/>.</remarks>
        public float TimeLeft
        {
            get
            {
                if (_settings?.termination.trainingEpisodeTime > 0)
                {
                    var dt = Time.fixedTime - _startTime;
                    return dt > 0 ? dt : 0;
                }
                return -1;
            }
        }

        public DroneAgentRewardProvider(
            TrainingSettings settings,
            DroneAgent agent,
            DroneStateManager droneState)
            : base("DroneSumReward")
        {
            if (!settings)
                throw new ArgumentNullException(nameof(settings));

            _waypointReward = new WaypointRewardProvider(settings.waypointRewardSettings, agent, agent.navigator);
            _movementReward = new MovementRewardProvider(settings.movementRewardSettings, agent.drone.Rigidbody);
            _obstaclePenalty = new ObstaclePenaltyProvider(settings.obstaclePenaltySettings, agent.drone.Rigidbody);
            _stateReward = new DroneStateRewardProvider(settings.stateRewardSettings, droneState);
        }

        public override float CalculateReward()
        {
            float reward = 0;
            if (_settings.movementRewardEnabled)
                reward += _movementReward.CalculateReward();

            if (_settings.waypointRewardEnabled)
                reward += _waypointReward.CalculateReward();

            if (_settings.obstaclePenaltyEnabled)
                reward += _obstaclePenalty.CalculateReward();

            if (_settings.stateRewardEnabled)
                reward += _stateReward.CalculateReward();

            if (TimeLeft == 0)
                return UpdateRewards(reward, true);
            
            if (_waypointReward.IsFinalReward || _obstaclePenalty.IsFinalReward || _movementReward.IsFinalReward ||
                _stateReward.IsFinalReward)
                return UpdateRewards(reward, true);

            if (_settings.termination.maxPenalty != 0 && CumulativeReward + reward < _settings.termination.maxPenalty)
                return UpdateRewards(reward, true);

            return UpdateRewards(reward);
        }

        public override void Reset()
        {
            _startTime = Time.fixedTime;
            _waypointReward.Reset();
            _movementReward.Reset();
            _obstaclePenalty.Reset();
            _stateReward.Reset();
            base.Reset();
        }

        public IEnumerable<RewardProvider> GetRewards()
        {
            yield return _waypointReward;
            yield return _movementReward;
            yield return _obstaclePenalty;
            yield return _stateReward;
        }

        public override string ToString()
        {
            const string format =
                "{0}: waypoint {1:F2}, mov {2:F2}, obs {3:F2}, state {4:F2} (cur.sum {5:F2}, total.sum {6:F2}){7}";

            var finalRewardLabel = IsFinalReward ? ", final" : "";
            return string.Format(format,
                RewardName,
                _waypointReward.LastReward,
                _movementReward.LastReward,
                _obstaclePenalty.LastReward,
                _stateReward.LastReward,
                LastReward,
                CumulativeReward,
                finalRewardLabel
            );
        }
    }
}