using System;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralRoad
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ProceduralRoad : MonoBehaviour
    {
        private const int DEFAULT_PIECES_COUNT = 10;
        private List<ProceduralRoadPiece> _roadPieces = new(DEFAULT_PIECES_COUNT);
        private MeasurableObject _roadMeasurements;

        [SerializeField]
        [Range(0, 20)]
        [Tooltip("Amount of pieces to generate.")]
        private int piecesCount = DEFAULT_PIECES_COUNT;

        [SerializeField]
        [Tooltip("Prefab that used to generate each road piece. Must contain ProceduralRoadPiece component.")]
        private GameObject roadPiecePrefab;
        
        [SerializeField]
        [Tooltip("Prefab that used to generate start of the road.")]
        private GameObject roadStartPrefab;
        
        private void OnValidate()
        {
            if (!roadPiecePrefab)
                 return;
            
            if (!roadPiecePrefab.GetComponent<ProceduralRoadPiece>())
            {
                Debug.LogWarningFormat("Road piece prefab '{0}' is missing ProceduralRoadPiece component and cannot be used.",
                    roadPiecePrefab.name);
                roadPiecePrefab = null;
            }
            else
            {
                _roadMeasurements = roadPiecePrefab.GetComponent<MeasurableObject>();
            }
        }

        [ContextMenu("Destroy all road pieces")]
        public void DestroyRoad()
        {
            foreach (var roadPiece in _roadPieces)
                DestroyImmediate(roadPiece.gameObject);
            _roadPieces.Clear();
        }

        [ContextMenu("Generate road pieces")]
        public void GenerateRoad()
        {
            if (!roadPiecePrefab)
                return;
            
            DestroyRoad();

            var x = transform.position.x;
            var y = transform.position.y;
            var z = transform.position.z;
            var roadLength = _roadMeasurements.Dimensions.z;
         
            for (var i = 0; i < piecesCount; i++)
            {
                var position = new Vector3(x, y, z);
                var roadPiece = Instantiate(roadPiecePrefab, position, Quaternion.identity, transform);

                var proceduralRoadPiece = roadPiece.GetComponent<ProceduralRoadPiece>();
                proceduralRoadPiece.gameObject.name = $"[Road_{i+1}] " + roadPiecePrefab.name;
                proceduralRoadPiece.GenerateObstacles();
                _roadPieces.Add(proceduralRoadPiece);
                
                z += roadLength;
            }
        }
    }
}