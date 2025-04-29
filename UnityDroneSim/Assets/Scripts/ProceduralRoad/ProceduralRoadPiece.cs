using System.Collections.Generic;
using UnityEngine;


namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeasurableObject))]
    public class ProceduralRoadPiece : MonoBehaviour
    {
        private const int DEFAULT_OBSTACLES = 10;
        
        private MeasurableObject _roadMeasurement;
        private readonly List<GameObject> _obstacles = new(DEFAULT_OBSTACLES);
        
        [SerializeField]
        [Range(0, 50)]
        [Tooltip("Max amount of obstacles that can be spawned.")]
        private int maxObstacles = DEFAULT_OBSTACLES;
        
        [SerializeField]
        [Range(1, 20)]
        [Tooltip("Distance along Z axis between two obstacles.")]
        private float obstacleOffset = 5f;
        
        [SerializeField]
        [Tooltip("Obstacles will spawn inside this object.")]
        private Transform obstacleParent;
        
        [SerializeField]
        [Tooltip("List of all prefabs that can be spawned. All prefabs must contain MeasurableObject component attached to them.")]
        private List<GameObject> obstaclesPool = new();
        
        
        private void OnEnable()
        {
            _roadMeasurement = GetComponent<MeasurableObject>();
        }

        private void OnValidate()
        {
            if (!obstacleParent)
                obstacleParent = transform;

            for (var i = 0; i < obstaclesPool.Count; i++)
            {
                if (!obstaclesPool[i].GetComponent<MeasurableObstacle>())
                {
                    Debug.LogWarningFormat("Obstacle prefab '{0}' is missing MeasurableObject component and will be removed from the list",
                        obstaclesPool[i].name);
                    obstaclesPool.RemoveAt(i);
                }
            }
        }
        
        [ContextMenu("Destroy all obstacles")]
        public void DestroyObstacles()
        {
            foreach (var obstacle in _obstacles)
                DestroyImmediate(obstacle, true);
            
            _obstacles.Clear();
            _roadMeasurement.ResetMeasurements();
        }

        [ContextMenu("Generate obstacles")]
        public void GenerateObstacles()
        {
            if (obstaclesPool.Count == 0 || !_roadMeasurement.ResetMeasurements())
                return;
            
            if (!obstacleParent)
                obstacleParent = transform;
            
            DestroyObstacles();
            
            var min = _roadMeasurement.MinPoint;
            var max = _roadMeasurement.MaxPoint;
            var z = min.z;
            
            for (var i = 0; i < maxObstacles && z <= max.z; i++)
            {
                var (obstacle, length) = GenerateObstacle(min, max, z, i);
                _obstacles.Add(obstacle);
                z += obstacleOffset + length;
            }

            _roadMeasurement.ResetMeasurements();
        }

        private (GameObject, float) GenerateObstacle(Vector3 min, Vector3 max, float z, int index)
        {
            var obstaclePrefab = obstaclesPool[Random.Range(0, obstaclesPool.Count)];
            var measurableObst = obstaclePrefab.GetComponent<MeasurableObstacle>();
                
            var position = new Vector3
            {
                x = measurableObst.AllowRandomX
                    ? Random.Range(min.x, max.x - measurableObst.Dimensions.x)
                    : min.x,
                y = measurableObst.AllowRandomY
                    ? Random.Range(min.y, max.y - measurableObst.Dimensions.y)
                    : min.y,
                z = z + measurableObst.Dimensions.z >= max.z
                    ? max.z - measurableObst.Dimensions.z
                    : z
            };
                
            var obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
            obstacle.transform.parent = obstacleParent;
            obstacle.name = $"[Obst_{index+1}] {obstaclePrefab.name}";
            return (obstacle, measurableObst.Dimensions.z);
        }
    }
}