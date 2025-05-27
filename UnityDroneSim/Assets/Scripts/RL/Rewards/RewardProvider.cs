using Unity.MLAgents;


namespace RL.Rewards
{
    /// <summary>
    /// Calculates and stores AI agent rewards.
    /// </summary>
    public abstract class RewardProvider 
    {
        protected readonly string RewardTypeName;
        protected readonly string RewardStatsName;

        /// <summary>Last calculated reward.</summary>
        public float LastReward { get; protected set; }

        /// <summary>Sum of all rewards during current episode.</summary>
        public float CumulativeReward { get; protected set; }

        /// <summary>Final state reward was collected.</summary>
        public bool IsFinalReward { get; protected set; }

        /// <summary>Name of this reward.</summary>
        public virtual string RewardName => RewardTypeName;


        protected RewardProvider()
        {
            RewardTypeName = GetType().Name.Replace("Provider", "").Replace("Reward", "");
            RewardStatsName = "Environment/" + RewardTypeName;
        }

        protected RewardProvider(string rewardName)
        {
            RewardTypeName = rewardName;
        }

        /// <summary>Calculate current reward based on agent and environment state.</summary>
        /// <returns>Reward value.</returns>
        public virtual float CalculateReward()
        {
            (LastReward, IsFinalReward) = CalculateRewardInternal();
            CumulativeReward += LastReward;
            return LastReward;
        }

        public virtual void Reset(bool shouldSaveStats = true)
        {
            if (shouldSaveStats)
                AddAcademyAvgStats(RewardStatsName, CumulativeReward);
 
            LastReward = CumulativeReward = 0;
            ResetInternal(shouldSaveStats);
        }
        
        
        /// <summary>Call this method inside OnDrawGizmos to display additional information about reward.</summary>
        public virtual void DrawGizmos() { }

        /// <summary>Reset all rewards values.</summary>
        /// <param name="shouldSaveStats"></param>
        protected virtual void ResetInternal(bool shouldSaveStats = true) { }

        /// <summary>Calculate current reward based on agent and environment state.</summary>
        /// <returns>Reward value.</returns>
        protected abstract (float, bool) CalculateRewardInternal();

        protected void AddAcademyAvgStats(string name, float value)
        {
            if (!Academy.IsInitialized) return;
            Academy.Instance.StatsRecorder.Add(name, value);
        }
        
        protected void AddAcademyRecentStats(string name, float value)
        {
            if (!Academy.IsInitialized) return;
            Academy.Instance.StatsRecorder.Add(name, value, StatAggregationMethod.MostRecent);
        }
        
        protected void AddAcademyHistStats(string name, float value)
        {
            if (!Academy.IsInitialized) return;
            Academy.Instance.StatsRecorder.Add(name, value, StatAggregationMethod.Histogram);
        }
        
        protected void AddAcademySumStats(string name, float value)
        {
            if (!Academy.IsInitialized) return;
            Academy.Instance.StatsRecorder.Add(name, value, StatAggregationMethod.Sum);
        }

        public override string ToString() =>
            $"{RewardName}: {LastReward:F2}, sum: {CumulativeReward:F2}{(IsFinalReward ? ", final" : "")}";
    }
}