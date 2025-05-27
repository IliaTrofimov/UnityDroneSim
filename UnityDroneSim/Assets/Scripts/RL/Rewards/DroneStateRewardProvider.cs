using System;
using Drone;
using RL.RewardsSettings;


namespace RL.Rewards
{
    /// <summary>
    /// Rewards and penalties for changing drone state.
    /// </summary>
    public class DroneStateRewardProvider : RewardProvider
    {
        private readonly DroneStateRewardSettings _settings;
        private readonly DroneStateManager        _droneState;
        private bool _isLanded, _isDestroyed;
        
        public DroneStateRewardProvider(DroneStateRewardSettings settings, DroneStateManager droneState)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _droneState = droneState ?? throw new ArgumentNullException(nameof(droneState));
        }

        protected override (float, bool) CalculateRewardInternal()
        {
            if (_droneState.AnyMotorsDestroyed && !_isDestroyed)
            {
                _isDestroyed = true;
                AddAcademySumStats("Environment/Collisions", 1);
                return (_settings.destructionPenalty, _settings.finishOnDestruction);
            }
            if (_droneState.Landed && !_isLanded)
            {
                _isLanded = true;
                AddAcademySumStats("Environment/Landings", 1);
                return (_settings.landingReward, _settings.finishOnLanding);
            }
            
            _isDestroyed = _droneState.AnyMotorsDestroyed;
            _isLanded = _droneState.Landed;
            return (0, false);
        }

        protected override void ResetInternal(bool shouldSaveStats = true)
        {
            _isDestroyed = _isLanded = false;
        }
    }
}