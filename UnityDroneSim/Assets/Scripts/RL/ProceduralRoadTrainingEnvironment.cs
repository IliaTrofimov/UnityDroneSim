using System.Collections;
using System.Linq;
using UnityEngine;


namespace RL
{
    public class ProceduralRoadTrainingEnvironment : DroneTrainingManager
    {
        [Header("Procedural generation")]
        [Tooltip("Procedural road generator.")]
        public ProceduralRoad.ProceduralRoad proceduralRoad;

        protected override void Start()
        {
            UpdateSpawnPoint();
            base.Start();
        }

        protected override IEnumerator ResetEnvironment()
        {
            Debug.AssertFormat(proceduralRoad, "ProceduralRoadEnvironment '{0}': missing ProceduralRoad.", name);
            yield return proceduralRoad.GenerateRoadParts();
            UpdateSpawnPoint();
        }
        
        private void UpdateSpawnPoint()
        {
            proceduralRoad.UpdateRoadParts();

            if (!proceduralRoad.RoadParts.Any())
            {
                Debug.LogErrorFormat("ProceduralRoadEnvironment '{0}': procedural road '{1}' is missing any road parts", name, proceduralRoad.name);
                return;
            }
            
            foreach (var roadPart in proceduralRoad.RoadParts)
            {
                spawn = roadPart.GetComponent<SpawnPoint>();
                if (spawn)
                {
                    Debug.LogFormat("ProceduralRoadEnvironment '{0}': found new spawn point '{1}'", name, spawn.name);
                    foreach (var agent in  DroneAgents)
                        agent.spawnPoint = spawn;
                    break;
                }
            }
        }
    }
}