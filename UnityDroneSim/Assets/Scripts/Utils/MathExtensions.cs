using System.Runtime.CompilerServices;
using Noise;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Utils
{
    /// <summary>Some math helper functions.</summary>
    public static class MathExtensions
    {
        /// <summary>Get next random value from Gaussian distribution with given params.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sample(float mean, float variance) => NextGaussianDouble() * math.sqrt(variance) + mean;

        /// <summary>Get next random value from Gaussian distribution with given params.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SamplePositive(NormalDistributionParam param) =>
            Mathf.Abs(NextGaussianDouble() * math.sqrt(param.variance) + param.mean);

        /// <summary>Get next random value from standard Gaussian distribution.</summary>
        public static float NextGaussianDouble()
        {
            float u, s;
            do
            {
                var v = 2.0f * Random.value - 1.0f;
                u = 2.0f * Random.value - 1.0f;
                s = u * u + v * v;
            } while (s >= 1.0f);

            var fac = Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);
            return u * fac;
        }

        /// <summary>Return given value if it is positive, otherwise return 0.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampPositive(float a) => a > 0f ? a : 0f;

        /// <summary>Set given value to zero if it is negative.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClampPositive(ref float a)
        {
            if (a < 0f) a = 0f;
        }

        /// <summary>Create new vector with all coordinates equal to module of given vector's coordinates.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(this Vector3 vector) =>
            new(math.abs(vector.x), math.abs(vector.y), math.abs(vector.z));

        /// <summary>Get rotation speed of the Rigidbody in rad/s along each axis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AxialAngularVelocity(this Rigidbody rigidBody)
        {
            var angularVelocity = rigidBody.angularVelocity;
            var rotation = rigidBody.rotation;
            return new Vector3(
                Vector3.Dot(rotation * Vector3.right, angularVelocity),
                Vector3.Dot(rotation * Vector3.up, angularVelocity),
                Vector3.Dot(rotation * Vector3.forward, angularVelocity)
            );
        }

        /// <summary>Get rotation speed of the Rigidbody in rad/s along X axis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PitchVelocity(this Rigidbody rigidBody) =>
            Vector3.Dot(rigidBody.rotation * Vector3.right, rigidBody.angularVelocity);

        /// <summary>Get rotation speed of the Rigidbody in rad/s along Y axis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float YawVelocity(this Rigidbody rigidBody) =>
            Vector3.Dot(rigidBody.rotation * Vector3.up, rigidBody.angularVelocity);

        /// <summary>Get rotation speed of the Rigidbody in rad/s along Z axis.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RollVelocity(this Rigidbody rigidBody) =>
            Vector3.Dot(rigidBody.rotation * Vector3.forward, rigidBody.angularVelocity);

        /// <summary>Returns new Euler angles of transform with values wrapped between [0, 179] degrees.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WrapEulerRotation180(this Transform transform)
        {
            var eulerRotation = transform.eulerAngles;
            var x = eulerRotation.x;
            var y = eulerRotation.y;
            var z = eulerRotation.z;

            if (x >= 180f) x -= 360f;
            if (y >= 180f) y -= 360f;
            if (z >= 180f) z -= 360f;

            return new Vector3(x, y, z);
        }

        /// <summary>Returns square root of absolute value and sets corresponding sign.</summary>
        /// <returns>sign(value) * √(abs(value))</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedSqrt(float value) => math.sign(value) * math.sqrt(math.abs(value));

        /// <summary>Returns square root of absolute value.</summary>
        /// <returns>√(abs(value))</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSqrt(float value) => math.sqrt(math.abs(value));

        /// <summary>Returns 1 if value greater than or equal to zero, otherwise -1.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NonZeroSign(float value) => value >= 0f ? 1f : -1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizedHeadingTo(this Transform current, Vector3 target)
        {
            var normalized = Vector3.Normalize(target - current.position);
            normalized.y = 0.0f;

            var currentHeading = Quaternion.Euler(new Vector3(0.0f, current.rotation.eulerAngles.y, 0.0f)) *
                                 Vector3.forward;

            currentHeading.y = 0.0f;

            var angle = Vector3.SignedAngle(currentHeading, normalized, Vector3.up);
            return angle;
        }
    }
}