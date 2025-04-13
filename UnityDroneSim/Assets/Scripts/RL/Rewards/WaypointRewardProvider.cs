using System;
using Navigation;
using RL.RewardsSettings;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;


namespace RL.Rewards
{
    /// <summary>Calculates rewards and penalties for moving towards or away the waypoint and reaching them.</summary>
    public class WaypointRewardProvider : RewardProvider
    {
        private readonly WaypointRewardSettings _settings;
        private readonly WaypointNavigator      _navigator;
        private readonly Agent                  _agent;
        private          float                  _lastDistance;
        private          int                    _lastWaypointIndex;

        public WaypointRewardProvider(WaypointRewardSettings settings, Agent agent, WaypointNavigator navigator)
        {
            _settings = settings ??
                        throw new ArgumentNullException(nameof(settings),
                            $"Cannot create {nameof(WaypointRewardProvider)} without {nameof(WaypointRewardSettings)} parameter."
                        );

            _agent = agent;
            _navigator = navigator;
        }


        public override float CalculateReward()
        {
            if (_navigator.IsFinished)
                return UpdateRewards(_settings.finishReward, true);

            if (!_navigator.CurrentWaypoint.HasValue)
                return UpdateRewards(0);

            if (_navigator.CurrentWaypointIndex != _lastWaypointIndex)
            {
                _lastWaypointIndex = _navigator.CurrentWaypointIndex;
                UpdateRewards(_settings.waypointReward);
            }

            var distance = Vector3.Distance(_agent.transform.position, _navigator.CurrentWaypoint.Value.position);
            var approachSpeed = (distance - _lastDistance) / Time.deltaTime;

            if (approachSpeed > 0.01)
                return UpdateRewards(_settings.movingToWaypointReward *
                                     math.clamp(approachSpeed, 0, 1)
                );

            if (approachSpeed < -0.01)
                return UpdateRewards(_settings.movingAwayFromWaypointPenalty *
                                     math.clamp(math.abs(approachSpeed), 0, 1)
                );

            return UpdateRewards(0);
        }
    }
}