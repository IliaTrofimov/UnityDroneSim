using System;
using Navigation;
using RL.RewardsSettings;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using Utils;
using UtilsDebug;


namespace RL.Rewards
{
    /// <summary>Calculates rewards and penalties for moving towards or away the waypoint and reaching them.</summary>
    public class WaypointRewardProvider : RewardProvider
    {
        private readonly WaypointRewardSettings _settings;
        private readonly WaypointNavigator      _navigator;
        private readonly Agent                  _agent;

        private int     _lastWaypointIndex;
        private float   _lastDistance;
        private float   _waypointDistance;
        private float   _approachSpeed;
        private float   _dotProduct;
        private Vector3 _direction2d;
        
        private GizmoOptions _waypointDistanceGizmo = new()
        {
            CapSize = 0,
            LabelOutline = true,
            LabelPlacement = GizmoLabelPlacement.Center
        };
        
        private GizmoOptions _waypointDirectionGizmo = new()
        {
            CapSize = 0.02f,
            VectSize = 0.25f,
            LabelOutline = true,
            LabelPlacement = GizmoLabelPlacement.End
        };

        public int WaypointsReached => _lastWaypointIndex;
        
        
        public WaypointRewardProvider(WaypointRewardSettings settings, Agent agent, WaypointNavigator navigator)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
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
            
            var waypoint = _navigator.CurrentWaypoint.Value.position;
            var direction3d = waypoint - _agent.transform.position;
            _direction2d = new Vector3(direction3d.x, 0f, direction3d.z).normalized;
            
            _dotProduct = Vector3.Dot(_direction2d, _agent.transform.forward);
            _waypointDistance = direction3d.magnitude;
            _approachSpeed = (_lastDistance - _waypointDistance) / Time.deltaTime;
            _lastDistance = _waypointDistance;
            
            float reward;
            
            if (_dotProduct < _settings.lookAtWaypointAngle)
                reward = _settings.lookAwayPenalty;
            else
                reward = _settings.lookAtReward;
            
            if (_approachSpeed > _settings.minimalApproachSpeed)
            {
                reward += _settings.movingToWaypointReward *
                          math.clamp(_approachSpeed, 0, 1);
            }

            if (_approachSpeed < -_settings.minimalApproachSpeed)
            {
                reward += _settings.movingAwayFromWaypointPenalty *
                          math.clamp(math.abs(_approachSpeed), 0, 1);
            }

            return UpdateRewards(reward);
        }

        public override void DrawGizmos()
        {
            if (_navigator.IsFinished || !_navigator.CurrentWaypoint.HasValue)
                return;

            _waypointDistanceGizmo.Color = _waypointDistanceGizmo.LabelColor =
                _approachSpeed > -_settings.minimalApproachSpeed ? Color.green : Color.magenta;
            
            _waypointDirectionGizmo.Color = _waypointDirectionGizmo.LabelColor = 
                _dotProduct > _settings.lookAtWaypointAngle ? Color.green : Color.magenta;

            VectorDrawer.DrawDirection(
                _agent.transform.position,
                _direction2d,
                $"Dot: {_dotProduct:F2}",
                _waypointDirectionGizmo);
            
            VectorDrawer.DrawLine(
                _agent.transform.position,
                _navigator.CurrentWaypoint.Value.position, 
                $"Wpt_{WaypointsReached}: {_waypointDistance:F1}\nDot: {_dotProduct:F2}\nSpd.: {_approachSpeed:F2}\nR: {LastReward:F3}",
                _waypointDistanceGizmo);

        }

        public override void Reset()
        {
            base.Reset();
            _lastWaypointIndex = 0;
        }
    }
}