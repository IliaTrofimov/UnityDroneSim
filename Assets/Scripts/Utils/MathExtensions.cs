using System.Runtime.CompilerServices;
using Noise;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetNaN(this Vector3 vector) => vector.Set(float.NaN, float.NaN, float.NaN);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetNaN(this Vector2 vector) => vector.Set(float.NaN, float.NaN);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NaNVector3() => new (float.NaN, float.NaN, float.NaN);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngle(float a) => math.ceil(math.floor(a / 180f) / 2f) * 360f;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(this Vector3 vector) => new (math.abs(vector.x), math.abs(vector.y), math.abs(vector.z));
    }
}