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

        public DroneStateRewardProvider(DroneStateRewardSettings settings, DroneStateManager droneState)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _droneState = droneState;
        }

        public override float CalculateReward()
        {
            if (_droneState.AnyMotorsDestroyed && _settings.destructionPenalty != 0)
                return UpdateRewards(_settings.destructionPenalty, true);

            if (_droneState.Landed && _settings.landingReward != 0)
                return UpdateRewards(_settings.destructionPenalty, true);

            return UpdateRewards(0);
        }
    }
}