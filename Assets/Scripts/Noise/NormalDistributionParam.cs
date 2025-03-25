using System;


namespace Noise
{
    /// <summary>Parameters of normal distribution.</summary>
    [Serializable]
    public class NormalDistributionParam
    {
        public float mean;
        public float variance;

        public NormalDistributionParam() : this(0, 0)
        {
        }

        public NormalDistributionParam(float variance) : this(0, variance)
        {
        }
        
        public NormalDistributionParam(float mean, float variance)
        {
            this.variance = variance;
            this.mean = mean;
        }
    }
}