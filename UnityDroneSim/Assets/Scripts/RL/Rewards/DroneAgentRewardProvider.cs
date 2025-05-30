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
        private string x;
        private readonly TrainingSettings         _settings;
        private readonly WaypointRewardProvider   _waypointReward;
        private readonly MovementRewardProvider   _movementReward;
        private readonly ObstaclePenaltyProvider  _obstaclePenalty;
        private readonly DroneStateRewardProvider _stateReward;
        private readonly HeightRewardProvider     _heightReward;
        private readonly bool _logsEnabled;
        private readonly string _agentName;
        
        private float _startTime;
        
        public int RewardsCount => 5;

        /// <summary>Time left to force restart of the episode.</summary>
        /// <remarks>Episode length is set int <see cref="TrainingSettings"/>.<see cref="TerminationConditions"/>.</remarks>
        public float TimeLeft
        {
            get
            {
                if (_settings?.termination.trainingEpisodeTime > 0)
                {
                    var dt = _settings.termination.trainingEpisodeTime - (Time.fixedTime - _startTime);
                    return dt > 0 ? dt : 0;
                }
                return float.PositiveInfinity;
            }
        }
        
        public DroneAgentRewardProvider(
            TrainingSettings settings,
            DroneAgent agent,
            DroneStateManager droneState,
            bool logsEnabled,
            string agentName)
            : base("DroneSumReward")
        {
            if (!settings)
                throw new ArgumentNullException(nameof(settings));

            _settings = settings;
            _waypointReward = new WaypointRewardProvider(settings.waypointRewardSettings, agent, agent.navigator);
            _movementReward = new MovementRewardProvider(settings.movementRewardSettings, agent.drone.Rigidbody);
            _obstaclePenalty = new ObstaclePenaltyProvider(settings.obstaclePenaltySettings, agent.drone.Rigidbody);
            _stateReward = new DroneStateRewardProvider(settings.stateRewardSettings, droneState);
            _heightReward = new HeightRewardProvider(settings.heightRewardSettings, agent);
            _logsEnabled = logsEnabled;
            _agentName = agentName;
        }
        
        public DroneAgentRewardProvider(
            TrainingSettings settings,
            DroneAgent agent,
            DroneStateManager droneState)
            : this(settings, agent, droneState, false, agent.gameObject.name)
        {
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
            
            if (_settings.heightRewardEnabled)
                reward += _heightReward.CalculateReward();
            
            var isFinal = _waypointReward.IsFinalReward ||
                          _obstaclePenalty.IsFinalReward ||
                          _movementReward.IsFinalReward ||
                          _stateReward.IsFinalReward ||
                          _heightReward.IsFinalReward;

            var lowBalance = _settings.termination.maxPenalty != 0 &&
                             CumulativeReward + reward < _settings.termination.maxPenalty;
            var timeout = TimeLeft <= 0;
            
            var isFinalReward = timeout || isFinal || lowBalance;
            UpdateRewards(reward, isFinalReward);
            
            if (_logsEnabled && isFinalReward)
            {
                Debug.LogFormat("[Agent Reward] '{0}': final reward {1:F2} | {2}{3}{4}{5}{6}{7}",
                    _agentName, CumulativeReward,
                    _waypointReward.IsFinalReward ? "WP " : "",
                    _obstaclePenalty.IsFinalReward ? "OBS " : "",
                    _movementReward.IsFinalReward ? "MOV " : "",
                    _stateReward.IsFinalReward ? "ST " : "",
                    lowBalance ? "LOW-REW " : "",
                    timeout ? "TIME" : "");
            }
            
            return LastReward;
        }

        public override void Reset()
        {
            _startTime = Time.fixedTime;
            _waypointReward.Reset();
            _movementReward.Reset();
            _obstaclePenalty.Reset();
            _stateReward.Reset();
            _heightReward.Reset();
            base.Reset();
        }

        public IEnumerable<RewardProvider> GetRewards()
        {
            yield return _waypointReward;
            yield return _movementReward;
            yield return _obstaclePenalty;
            yield return _stateReward;
            yield return _heightReward;
        }

        public override string ToString()
        {
            const string format =
                "{0}: waypoint {1:F2}, mov {2:F2}, obs {3:F2}, state {4:F2}, height {5:F2} (cur.sum {6:F2}, total.sum {7:F2}){8}";

            var finalRewardLabel = IsFinalReward ? ", final" : "";
            return string.Format(format,
                RewardName,
                _waypointReward.LastReward,
                _movementReward.LastReward,
                _obstaclePenalty.LastReward,
                _stateReward.LastReward,
                _heightReward.LastReward,
                LastReward,
                CumulativeReward,
                finalRewardLabel
            );
        }
        
        public override void DrawGizmos()
        {
           if (_settings.obstaclePenaltyEnabled)
               _obstaclePenalty.DrawGizmos();
           if (_settings.waypointRewardEnabled)
               _waypointReward.DrawGizmos();
           if (_settings.heightRewardEnabled)
               _heightReward.DrawGizmos();
        }
        
        /// <summary>
        /// Calculate drone's flight altitude.
        /// This method uses <see cref="HeightRewardProvider"/>.Height property to get accurate height.
        /// </summary>
        public float GetDroneAltitude()
        {
            return _heightReward.Height;
        }
        
        /// <summary>
        /// Count visited waypoints in this episode.
        /// This method uses <see cref="WaypointRewardProvider"/>.WaypointsReached property to get value.
        /// </summary>
        public float GetReachedWaypoints()
        {
            return _waypointReward.WaypointsReached;
        }

        public bool IsWaypointReachedAtThisStep()
        {
            return _waypointReward.IsWaypointReachedAtThisStep;
        }
    }
}