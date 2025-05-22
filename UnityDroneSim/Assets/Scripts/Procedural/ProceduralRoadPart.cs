using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;


namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ProceduralRoadPart : MeasurableObject
    {
        private const int DEFAULT_OBSTACLES = 10;
        private readonly List<GameObject> _obstacles = new(DEFAULT_OBSTACLES);
        

        [Header("Procedural Road Part parameters")]
        [Header("Connections")]
        [Tooltip("X coordinate where previous road part should be connected to this part.")]
        [SerializeField, Min(0)]
        private float connectThisAtX = 0;
        
        [Tooltip("X coordinate where next road part should be connected to this part.")]
        [SerializeField, Min(0)]
        private float connectNextAtX = 0;
        
        [Tooltip("Safe width (from connectThisAtX to connectThisAtX + startConnectionWidth) for connecting with previous road part.")]
        [SerializeField, Min(0)]
        private float startConnectionWidth = 20;
        
        [Tooltip("Safe width (from connectNextAtX to connectNextAtX + finishConnectionWidth) for connecting with next road part.")]
        [SerializeField, Min(0)]
        private float finishConnectionWidth = 20;
        
        
        [Header("Offsets")]
        [Tooltip("Distance along Z axis between two obstacles.")]
        [SerializeField]
        [Range(0.01f, 50)]
        private float obstacleOffset = 5f;
        
        [SerializeField]
        [Range(0, 50)]
        [Tooltip("Obstacles will should spawn after this distance from the beginning of the road.")]
        private float startOffset = 5f;
        
        [SerializeField]
        [Range(0, 50)]
        [Tooltip("Obstacles will should not spawn after this distance before finish of the road.")]
        private float finishOffset = 5f;

        [Header("Obstacles")]
        [Tooltip("Max amount of obstacles that can be spawned.")]
        [SerializeField]
        [Range(0, 50)]
        private int maxObstacles = DEFAULT_OBSTACLES;
        
        [SerializeField]
        [Tooltip("Obstacles will spawn inside this object.")]
        private Transform obstacleParent;
        
        [SerializeField]
        [Tooltip("List of all prefabs that can be spawned. All prefabs must contain MeasurableObject component attached to them.")]
        private List<GameObject> obstaclesPool = new();
        
        public float ConnectThisAtX => connectThisAtX + MinPoint.x;
        public float ConnectNextAtX => connectNextAtX + MinPoint.x;
        public float ConnectThisOffsetX => connectThisAtX;
        public float ConnectNextOffsetX=> connectNextAtX;
        public float StartConnectionWidth => startConnectionWidth;
        public float StartOffsetZ => startOffset;


        private void OnValidate()
        {
            for (var i = 0; i < obstaclesPool.Count; i++)
            {
                if (!obstaclesPool[i].GetComponent<MeasurableObstacle>())
                {
                    Debug.LogWarningFormat("Obstacle prefab '{0}' is missing MeasurableObject component and will be removed from the list",
                        obstaclesPool[i].name);
                    obstaclesPool.RemoveAt(i);
                }
            }

            var width = math.abs(MaxPoint.x - MinPoint.x);
            
            if (connectThisAtX < 0)
                connectThisAtX = 0;
            else if (connectThisAtX > width)
                connectThisAtX = width;
            
            if (startConnectionWidth > width - connectThisAtX)
                startConnectionWidth = width - connectThisAtX;
            
            if (connectNextAtX < 0)
                connectNextAtX = 0;
            else if (connectNextAtX > width)
                connectNextAtX = width;
            
            if (finishConnectionWidth > width - connectNextAtX)
                finishConnectionWidth = width - connectNextAtX;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!enabled || !showDebug)
                return;
            
            var min = MinPoint;
            var max = MaxPoint;
            
            // Start
            var vertices = new Vector3[]
            {
                new(min.x, min.y, min.z),
                new(min.x, max.y, min.z),
                new(max.x, max.y, min.z),
                new(max.x, min.y, min.z)
            };
            
            Handles.DrawSolidRectangleWithOutline(vertices, 
                new Color(0.2f, 0.2f, 0.2f, 0.2f),
                Color.white);
            
            // Start + offset
            vertices[0].z += startOffset;
            vertices[1].z += startOffset;
            vertices[2].z += startOffset;
            vertices[3].z += startOffset;
            
            Handles.DrawSolidRectangleWithOutline(vertices, 
                new Color(0, 1, 0.2f, 0.05f),
                Color.white);

            // Finish - offset
            vertices[0].z = max.z - finishOffset;
            vertices[1].z = max.z - finishOffset;
            vertices[2].z = max.z - finishOffset;
            vertices[3].z = max.z - finishOffset;

            Handles.DrawSolidRectangleWithOutline(vertices, 
                new Color(0, 0.2f, 1f, 0.05f),
                Color.white);
            
            // Connection start
            vertices[0] = new(min.x + connectThisAtX, min.y, min.z);
            vertices[1] = new(min.x + connectThisAtX, max.y, min.z);
            vertices[2] = new(min.x + connectThisAtX + startConnectionWidth, max.y, min.z);
            vertices[3] = new(min.x + connectThisAtX + startConnectionWidth, min.y, min.z);
   
            Handles.DrawSolidRectangleWithOutline(vertices, 
                new Color(1, 0.92f, 0.016f, 0.16f),
                Color.yellow);
            
            // Connection finish
            vertices[0] = new(min.x + connectNextAtX, min.y, max.z);
            vertices[1] = new(min.x + connectNextAtX, max.y, max.z);
            vertices[2] = new(min.x + connectNextAtX + finishConnectionWidth, max.y, max.z);
            vertices[3] = new(min.x + connectNextAtX + finishConnectionWidth, min.y, max.z);
   
            Handles.DrawSolidRectangleWithOutline(vertices, 
                Color.clear,
                Color.yellow);
        }

        [ContextMenu("Destroy all obstacles")]
        public void DestroyObstacles()
        {
            foreach (var obstacle in _obstacles)
                DestroyImmediate(obstacle);
            
            obstacleParent.DestroyChildren();
            
            _obstacles.Clear();
            ResetMeasurements();
        }

        [ContextMenu("Generate obstacles")]
        public void GenerateObstacles()
        {
            if (obstaclesPool.Count == 0 || !ResetMeasurements())
                return;
            
            DestroyObstacles();
            
            var min = MinPoint;
            var max = MaxPoint;
            max.y *= 0.9f;
            max.z -= finishOffset;
            min.z += startOffset;

            for (var i = 0; i < maxObstacles && min.z <= max.z; i++)
            {
                var (obstacle, length) = GenerateObstacle(min, max, i);
                _obstacles.Add(obstacle);
                min.z += obstacleOffset + length;
            }
            
            ResetMeasurements();
        }

        private (GameObject, float) GenerateObstacle(Vector3 min, Vector3 max, int index)
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
                z = min.z + measurableObst.Dimensions.z >= max.z
                    ? max.z - measurableObst.Dimensions.z
                    : min.z
            };
                
            var obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
            obstacle.transform.parent = obstacleParent ?? transform;
            obstacle.name = $"[Obst_{index+1}] {obstaclePrefab.name}";
            return (obstacle, measurableObst.Dimensions.z);
        }
    }
}