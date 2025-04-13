namespace RL.Rewards
{
    /// <summary>
    /// Calculates and stores AI agent rewards.
    /// </summary>
    public abstract class RewardProvider
    {
        protected readonly string RewardTypeName;

        /// <summary>Last calculated reward.</summary>
        public float LastReward { get; protected set; }

        /// <summary>Sum of all rewards during current episode.</summary>
        public float CumulativeReward { get; protected set; }

        /// <summary>Final state reward was collected.</summary>
        public bool IsFinalReward { get; protected set; }

        /// <summary>Name of this reward.</summary>
        public virtual string RewardName => RewardTypeName;


        protected RewardProvider() { RewardTypeName = GetType().Name.Replace("Provider", ""); }

        protected RewardProvider(string rewardName) { RewardTypeName = rewardName; }


        /// <summary>Calculate current reward based on agent and environment state.</summary>
        /// <returns>Reward value.</returns>
        public abstract float CalculateReward();

        /// <summary>Reset all rewards values.</summary>
        public virtual void Reset() { LastReward = CumulativeReward = 0; }

        /// <summary>Shortcut for updating current and cumulative rewards.</summary>
        protected float UpdateRewards(float currentReward, bool isFinalReward = false)
        {
            LastReward = currentReward;
            CumulativeReward += currentReward;
            IsFinalReward = isFinalReward;
            return currentReward;
        }

        public override string ToString() =>
            $"{RewardName}: {LastReward:F2}, sum: {CumulativeReward:F2}{(IsFinalReward ? ", final" : "")}";
    }
}