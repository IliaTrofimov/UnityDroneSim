using Unity.MLAgents.Sensors;


namespace RL
{
    public static class MlAgentsExtensions
    {
        public static void EndEpisode(this DroneAgent agent, float finalReward)
        {
            agent.AddReward(finalReward);
            agent.EndEpisode();
        }
    }
}