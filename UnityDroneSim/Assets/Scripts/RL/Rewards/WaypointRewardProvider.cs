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
        
        private GizmoOptions _waypointDistanceGizmo = new()
        {
            CapSize = 0,
            LabelSize = 0.75f,
            LabelOutline = true,
            LabelPlacement = GizmoLabelPlacement.Center,
        };
        
        private GizmoOptions _waypointDirectionGizmo = new()
        {
            CapSize = 0.02f,
            VectSize = 0.25f,
            LabelSize = 0.75f,
            LabelOutline = true,
            LabelPlacement = GizmoLabelPlacement.End,
            VectCapSize = 0.1f
        };
        
        private int _lastWaypointIndex;
        private float _lastDistance;
        
        public float WaypointDistance { get; private set; }
        public float ApproachSpeed { get; private set; }
        public Vector2 Heading { get; private set; }
        public bool IsLookingAtWaypoint { get; private set; }
        public int WaypointsReached { get; private set; }
        public bool IsWaypointReachedAtThisStep {get; private set;}
        
        public WaypointRewardProvider(WaypointRewardSettings settings, Agent agent, WaypointNavigator navigator)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }


        public override float CalculateReward()
        {
            if (_navigator.IsFinished)
            {
                if (!IsWaypointReachedAtThisStep)
                {
                    IsWaypointReachedAtThisStep = true;
                    WaypointsReached++;
                }
                return UpdateRewards(_settings.finishReward, true);
            }

            if (_navigator.CurrentWaypoint == null)
                return UpdateRewards(0);

            if (_navigator.CurrentWaypointIndex != _lastWaypointIndex)
            {
                WaypointsReached++;
                IsWaypointReachedAtThisStep = true;
                _lastWaypointIndex = _navigator.CurrentWaypointIndex;
                UpdateRewards(_settings.waypointReward);
            }
            else
            {
                IsWaypointReachedAtThisStep = false;
            }
            
            Heading = _navigator.GetCurrentHeading(_agent.transform, normalized: false);
            WaypointDistance = _navigator.GetCurrentDistance();
            ApproachSpeed = (_lastDistance - WaypointDistance) / Time.deltaTime;
            _lastDistance = WaypointDistance;
            
            float reward;

            IsLookingAtWaypoint = math.abs(Heading.x) <= _settings.lookAtWaypointAngle / 2;
            if (!IsLookingAtWaypoint)
                reward = _settings.lookAwayPenalty;
            else
                reward = _settings.lookAtReward;
            
            if (ApproachSpeed > _settings.minimalApproachSpeed)
            {
                reward += _settings.movingToWaypointReward *
                          math.clamp(ApproachSpeed, 0, 1);
            }

            if (ApproachSpeed < -_settings.minimalApproachSpeed)
            {
                reward += _settings.movingAwayFromWaypointPenalty *
                          math.clamp(math.abs(ApproachSpeed), 0, 1);
            }

            return UpdateRewards(reward);
        }

        public override void DrawGizmos()
        {
            if (_navigator.IsFinished || _navigator.CurrentWaypoint == null)
                return;

            var waypoint = _navigator.CurrentWaypoint.position;
            var direction3d = waypoint - _agent.transform.position;
            var direction2d = new Vector3(direction3d.x, 0f, direction3d.z).normalized;
            var dotProduct = Vector3.Dot(direction2d, _agent.transform.forward);
            
            _waypointDistanceGizmo.Color = _waypointDistanceGizmo.LabelColor =
                ApproachSpeed > -_settings.minimalApproachSpeed ? Color.green : Color.magenta;
            
            _waypointDirectionGizmo.Color = _waypointDirectionGizmo.LabelColor =
                IsLookingAtWaypoint ? Color.green : Color.magenta;
            
            const string waypointLabel = "Waypoint_{0}: {1:F1} m\nDot: {2:F2}\nSpd.: {3:F2} m/s\nR: {4:F3}";
            const string directionLabel = "Dot: {0:F2}\nH: {1:F0}° | V: {2:F0}°";
            
            VectorDrawer.DrawLine(
                _agent.transform.position,
                _navigator.CurrentWaypoint.position,
                string.Format(waypointLabel,
                    WaypointsReached,
                    WaypointDistance,
                    dotProduct,
                    ApproachSpeed,
                    LastReward
                ),
                _waypointDistanceGizmo);

            VectorDrawer.DrawDirection(
                _agent.transform.position,
                direction2d,
                string.Format(directionLabel, dotProduct, Heading.x, Heading.y),
                _waypointDirectionGizmo);
            
            _waypointDirectionGizmo.Color.a = 0.1f;
            VectorDrawer.DrawArc(_agent.transform.position,
                _agent.transform.forward, 
                _agent.transform.up, 
                -_settings.lookAtWaypointAngle / 2f,
                _waypointDirectionGizmo);

            VectorDrawer.DrawArc(_agent.transform.position,
                _agent.transform.forward, 
                _agent.transform.up, 
                _settings.lookAtWaypointAngle / 2f,
                _waypointDirectionGizmo);
        }

        public override void Reset()
        {
            base.Reset();
            _lastWaypointIndex = 0;
            IsWaypointReachedAtThisStep = false;
            WaypointsReached = 0;
        }
    }
}