using UnityEngine;


namespace Utils
{
    public static class GameObjectUtils
    {
        public static bool TryGetDimensions(this GameObject gameObject, out Vector3 minPoint, out Vector3 maxPoint)
        {
            var meshes = gameObject.GetComponents<MeshRenderer>();
            var childMeshes = gameObject.GetComponentsInChildren<MeshRenderer>();

            minPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            maxPoint = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            if ((meshes == null || meshes.Length == 0) && (childMeshes == null || childMeshes.Length == 0))
                return false;
            
            if (meshes?.Length > 0)
            {
                minPoint = meshes[0].bounds.min;
                maxPoint = meshes[0].bounds.max;
                for (var i = 1; i < meshes.Length; i++)
                {
                    minPoint = Vector3.Min(minPoint, meshes[i].bounds.min);
                    maxPoint = Vector3.Max(maxPoint, meshes[i].bounds.max);
                }
            }

            if (childMeshes?.Length > 0)
            {
                for (var i = 0; i < childMeshes.Length; i++)
                {
                    minPoint = Vector3.Min(minPoint, childMeshes[i].bounds.min);
                    maxPoint = Vector3.Max(maxPoint, childMeshes[i].bounds.max);
                }
            }
            
            return true;           
        }

        public static bool TryGetDimensions(this GameObject gameObject, out Vector3 axis)
        {
            if (!TryGetDimensions(gameObject, out Vector3 minPoint, out Vector3 maxPoint))
            {
                axis = Vector3.zero;
                return false;
            }
            axis = (maxPoint - minPoint).Abs();
            return true;
        }
        
        public static bool TryGetDimensions(this GameObject gameObject, out float diagonalLength)
        {
            if (!TryGetDimensions(gameObject, out Vector3 minPoint, out Vector3 maxPoint))
            {
                diagonalLength = float.NaN;
                return false;
            }
            diagonalLength = (maxPoint - minPoint).magnitude;
            return true;
        }
    }
}