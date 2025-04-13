using System.Collections.Generic;


namespace RL.Rewards
{
    /// <summary>
    /// Reward provider that contains several other rewards.
    /// </summary>
    public interface ICompositeRewardProvider
    {
        /// <summary>Current amount of different rewards.</summary>
        public int RewardsCount { get; }

        /// <summary>Enumerate all internal reward providers.</summary>
        public IEnumerable<RewardProvider> GetRewards();
    }
}