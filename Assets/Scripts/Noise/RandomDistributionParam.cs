using System;


namespace Noise
{
    [Serializable]
    public class RandomDistributionParam
    {
        public float mean;
        public float variance;

        public RandomDistributionParam(float mean, float variance)
        {
            this.variance = variance;
            this.mean = mean;
        }
        
        public RandomDistributionParam(float variance)
        {
            this.variance = variance;
        }
        
        public RandomDistributionParam() { }
    }
}