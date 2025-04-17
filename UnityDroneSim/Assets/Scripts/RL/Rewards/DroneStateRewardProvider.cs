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

        public override float CalculateReward()
        {
            if (_droneState.AnyMotorsDestroyed && !_isDestroyed)
                UpdateRewards(_settings.destructionPenalty, _settings.finishOnDestruction);
            else if (_droneState.Landed && !_isLanded)
                UpdateRewards(_settings.landingReward, _settings.finishOnLanding);
            else
                UpdateRewards(0, false);
            
            _isDestroyed = _droneState.AnyMotorsDestroyed;
            _isLanded = _droneState.Landed;
            return LastReward;
        }

        public override void Reset()
        {
            base.Reset();
            _isDestroyed = _isLanded = false;
        }
    }
}