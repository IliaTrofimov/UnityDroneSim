using UnityEngine;


namespace Utils
{
    public static class AnimationCurveExtensions
    {
        /// <summary>Add new key to the curve so internal buffer keeps constant size removing old keys.</summary>
        /// <remarks>
        /// Works only in Editor mode, otherwise does nothing.<br/>
        /// Does nothing when <c>maxBufferSize</c> is less than or equal to 0.
        /// </remarks>
        public static void AddKeyWrapped(this AnimationCurve curve, float time, float value, int maxBufferSize = 50)
        {
            curve.AddKeyWrapped(new Keyframe(time, value), maxBufferSize);   
        }
        
        /// <summary>Add new key to the curve so internal buffer keeps constant size removing old keys.</summary>
        /// <remarks>
        /// Works only in Editor mode, otherwise does nothing.<br/>
        /// Does nothing when <c>maxBufferSize</c> is less than or equal to 0.
        /// </remarks>
        public static void AddKeyWrapped(this AnimationCurve curve, Keyframe keyframe, int maxBufferSize = 50)
        {
            #if UNITY_EDITOR
            if (maxBufferSize <= 0) return; 
            
            if (curve.length <= maxBufferSize)
            {
                curve.AddKey(keyframe);
            }
            else
            {
                for (var i = 1; i < curve.length; i++)
                    curve.MoveKey(i - 1, curve.keys[i]);
                curve.MoveKey(curve.length - 1, keyframe);
            }
            #else
            Debug.LogError($"Extension method {nameof(AnimationCurve)}.{nameof(AddKeyWrapped)} can be called only in editor mode.");
            #endif
        }
    }
}