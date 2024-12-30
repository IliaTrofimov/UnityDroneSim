using Noise;
using UnityEngine;


namespace Utils
{
    public static class MathExtensions
    {
        public static float Sample(NormalDistributionParam param)
        {
            return NextGaussianDouble() * Mathf.Sqrt(param.variance) + param.mean;
        }
        
        public static float Sample(float mean, float variance)
        {
            return NextGaussianDouble() * Mathf.Sqrt(variance) + mean;
        }

        public static float SamplePositive(NormalDistributionParam param)
        {
            return Mathf.Abs( NextGaussianDouble() * Mathf.Sqrt(param.variance) + param.mean);
        }

        public static float NextGaussianDouble()
        {
            float u, s;
            do
            {
                var v = 2.0f * Random.value - 1.0f;
                u = 2.0f * Random.value - 1.0f;
                s = u*u + v*v;
            }
            while (s >= 1.0f);
            var fac = Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);
            return u * fac;
        }
        
        
    }
}