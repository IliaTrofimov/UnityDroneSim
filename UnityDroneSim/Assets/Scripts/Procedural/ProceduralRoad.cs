using System;
using System.Collections;
using System.Collections.Generic;
using Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;


namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ProceduralRoad : MonoBehaviour
    {
        private const int DEFAULT_PIECES_COUNT = 10;
        private List<ProceduralRoadPart> _roadParts = new(DEFAULT_PIECES_COUNT);

        [SerializeField]
        [Header("Navigation")]
        [Tooltip("Waypoint path.")]
        private WaypointPath waypointPath;

        [SerializeField]
        [Range(0.1f, 5)]
        [Tooltip("All waypoints will have this activation radius.")]
        private float defaultWaypointRadius = 2;
        
        [SerializeField]
        [Range(0f, 50f)]
        [Tooltip("All waypoints will be generated at this Y coordinate (in local space) if allowRandomWaypointY=false.")]
        private float defaultWaypointY = 0;
        
        [SerializeField]
        [Tooltip("Waypoints can be generated at random Y coordinate. Height will be in not less than defaultWaypointRadius.")]
        private bool allowRandomWaypointY;
        
        [SerializeField]
        [Range(0, 20)]
        [Header("Procedural generation")]
        [Tooltip("Amount of pieces to generate.")]
        private int piecesCount = DEFAULT_PIECES_COUNT;

        [SerializeField] 
        [Tooltip("Parent object for all generated road parts.")]
        private Transform roadParent;
        
        [SerializeField]
        [Tooltip("Prefab that used to generate start of the road.")]
        private GameObject roadStartPrefab;
        private ProceduralRoadPart _roadStart;

        [SerializeField]
        [Tooltip("Prefab that used to generate finish of the road.")]
        private GameObject roadFinishPrefab;
        private ProceduralRoadPart _roadFinish;

        [SerializeField]
        [Tooltip("Prefab that used to generate each road piece. Must contain ProceduralRoadPiece component.")]
        private List<GameObject> roadPartsPool;
        
        public IEnumerable<ProceduralRoadPart> RoadParts => _roadParts;
        
        private void OnValidate()
        {
            for (var i = 0; i < roadPartsPool.Count; i++)
            {
                if (!roadPartsPool[i].GetComponent<ProceduralRoadPart>())
                {
                    Debug.LogWarningFormat("Road part prefab '{0}' is missing ProceduralRoadPart component and will be removed from the list",
                        roadPartsPool[i].name);
                    roadPartsPool.RemoveAt(i);
                }
            }
            
            if (roadStartPrefab && !roadStartPrefab.GetComponent<ProceduralRoadPart>())
            {
                Debug.LogWarningFormat("Road start prefab '{0}' is missing ProceduralRoadPart component and will be removed from the list",
                    roadStartPrefab.name);

                roadStartPrefab = null;
            }
        }

        [ContextMenu("Destroy all road parts")]
        public void DestroyRoad()
        {
            foreach (var roadPiece in _roadParts)
               roadPiece.TryDestroy();
            
            _roadParts.Clear();
            waypointPath?.ClearWaypoints();

            roadParent.DestroyChildren();
        }

        [ContextMenu("Generate road parts")]
        private void GenerateRoad()
        {
            StartCoroutine(GenerateRoadParts());
        }
        
        public IEnumerator GenerateRoadParts()
        {
            if (roadPartsPool == null || roadPartsPool.Count == 0)
                yield break;
            
            DestroyRoad();
            
            var x = transform.position.x;
            var y = transform.position.y;
            var z = transform.position.z;
            
            var parent = roadParent ?? transform;

            if (roadStartPrefab)
            {
                _roadStart.TryDestroy();
                
                var roadStart = Instantiate(roadStartPrefab, parent);
                _roadStart = roadStart.GetComponent<ProceduralRoadPart>();
                roadStart.name = "[Road_start] " + roadStartPrefab.name;
                z += _roadStart.Dimensions.z;
                x += _roadStart.ConnectNextOffsetX;
            }
            
            yield return null;
            
            for (var i = 0; i < piecesCount; i++)
            {
                var roadPartPrefab = roadPartsPool[Random.Range(0, roadPartsPool.Count)];
                
                var roadObj = Instantiate(roadPartPrefab, parent);
                var roadPart = roadObj.GetComponent<ProceduralRoadPart>();
                
                x -= roadPart.ConnectThisOffsetX;
                
                roadObj.transform.position = new Vector3(x, y, z);
                roadObj.name = $"[Road_{i+1}] " + roadPartPrefab.name;
                roadPart.ResetMeasurements();
                roadPart.GenerateObstacles();
                _roadParts.Add(roadPart);
                
                z += roadPart.Dimensions.z;
                x += roadPart.ConnectNextOffsetX;
                
                yield return null;
            }
            
            if (roadFinishPrefab)
            {
                _roadFinish.TryDestroy();

                var roadFinish = Instantiate(roadFinishPrefab, parent);
                roadFinish.name = "[Road_finish] " + roadFinishPrefab.name;
                _roadFinish = roadFinish.GetComponent<ProceduralRoadPart>();
                
                roadFinish.transform.position = new Vector3(x + _roadFinish.ConnectThisOffsetX, y, z);
                _roadFinish.ResetMeasurements();
            }
            
            GenerateWaypoints();
        }

        [ContextMenu("Generate waypoints")]
        public void GenerateWaypoints()
        {
            waypointPath ??= gameObject.AddComponent<WaypointPath>();
            waypointPath.ClearWaypoints();
            
            for (var i = 0; i < piecesCount; i++)
            {
                var roadPart = _roadParts[i];
                waypointPath.AddWaypoint(
                    x: roadPart.ConnectThisAtX + Random.Range(roadPart.StartConnectionWidth*0.25f, roadPart.StartConnectionWidth * 0.75f), 
                    y: GetWaypointHeight(roadPart.MinPoint.y, roadPart.MaxPoint.y),
                    z: roadPart.transform.position.z + roadPart.StartOffsetZ / 2,
                    radius: defaultWaypointRadius);
            }
            
            if (roadFinishPrefab)
            {
                waypointPath.AddWaypoint(
                    x: _roadFinish.MinPoint.x + _roadFinish.Dimensions.x / 2, 
                    y: defaultWaypointRadius,
                    z: _roadFinish.MinPoint.z + _roadFinish.Dimensions.z / 2,
                    radius: defaultWaypointRadius,
                    waypointName: "Road_finish");
            }
        }

        private float GetWaypointHeight(float minY, float maxY)
        {
            if (allowRandomWaypointY)
            {
                return Random.Range(
                    minY + defaultWaypointRadius,
                    maxY - 3 * defaultWaypointRadius
                );
            }
            else
            {
                minY = math.max(minY + defaultWaypointRadius, minY + defaultWaypointY);
                return math.min(minY, maxY - 2 * defaultWaypointRadius);
            }
        }
        
        [ContextMenu("Generate road obstacles")]
        public void GenerateRoadObstacles()
        {
            if (_roadParts == null || _roadParts.Count == 0)
                return;

            StartCoroutine(GenerateObstaclesCoroutine());
        }

        [ContextMenu("Update road parts")]
        public void UpdateRoadParts()
        {
            if (_roadParts == null)
                _roadParts = new List<ProceduralRoadPart>();
            else
                _roadParts.Clear();
            
            var parent = roadParent ?? transform;
            for (var i = 0; i < parent.childCount; i++)
                _roadParts.Add(parent.GetChild(i).GetComponent<ProceduralRoadPart>());
            
            if (_roadParts.Count == 0) 
                Debug.LogWarningFormat("ProceduralRoad '{0}': missing any ProceduralRoadPart inside '{1}'", name, parent.name);
            else
                Debug.LogFormat("ProceduralRoad '{0}': found {1} ProceduralRoadPart inside '{2}'", name, _roadParts.Count, parent.name);
        }

        private IEnumerator GenerateObstaclesCoroutine()
        {
            for (var i = 0; i < _roadParts.Count; i++)
            {
                _roadParts[i].GenerateObstacles();
                yield return null;
            }
        }
    }
}