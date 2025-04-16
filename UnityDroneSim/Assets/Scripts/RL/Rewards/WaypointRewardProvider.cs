using System;
using Navigation;
using RL.RewardsSettings;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using UtilsDebug;


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
        private          float                  _waypointDistance;
        private          float                  _approachSpeed;

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

            _waypointDistance = Vector3.Distance(_agent.transform.position, _navigator.CurrentWaypoint.Value.position);
            _approachSpeed = (_lastDistance - _waypointDistance) / Time.deltaTime;
            _lastDistance = _waypointDistance;
            
            if (_approachSpeed > _settings.minimalApproachSpeed)
                return UpdateRewards(_settings.movingToWaypointReward *
                                     math.clamp(_approachSpeed, 0, 1)
                );

            if (_approachSpeed < -_settings.minimalApproachSpeed)
                return UpdateRewards(_settings.movingAwayFromWaypointPenalty *
                                     math.clamp(math.abs(_approachSpeed), 0, 1)
                );

            return UpdateRewards(0);
        }

        public override void DrawGizmos()
        {
            if (_navigator.IsFinished || !_navigator.CurrentWaypoint.HasValue)
                return;
            
            var color = _approachSpeed > -_settings.minimalApproachSpeed ? Color.green : Color.magenta;
            var options = new GizmoOptions()
            {
                CapSize = 0,
                Color = color,
                LabelColor = color,
                LabelOutline = true,
                LabelPlacement = GizmoLabelPlacement.Center
            };
                
            VectorDrawer.DrawLine(
                _agent.transform.position,
                _navigator.CurrentWaypoint.Value.position, 
                $"Wpt.: {_waypointDistance:F1}\nSpd.: {_approachSpeed:F2}\nR: {LastReward:F3}",
                options);
        }
    }
}