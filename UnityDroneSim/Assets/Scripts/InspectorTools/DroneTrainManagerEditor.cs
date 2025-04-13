using System.Linq;
using RL;
using UnityEditor;
using UnityEngine;


namespace InspectorTools
{
    /// <summary>
    /// Custom inspector editor for <see cref="DroneTrainManager"/>.
    /// </summary>
    [CustomEditor(typeof(DroneTrainManager))]
    public class DroneTrainManagerEditor : Editor
    {
        public enum TableMode { LastReward, CumulativeReward, Summary }

        private const float AGENT_COLUMN_WIDTH  = 80;
        private const float REWARD_COLUMN_WIDTH = 60;

        private static readonly GUILayoutOption[] AgentColumnOptions =
        {
            GUILayout.MinWidth(AGENT_COLUMN_WIDTH), GUILayout.MaxWidth(AGENT_COLUMN_WIDTH * 2),
            GUILayout.ExpandWidth(true)
        };

        private static readonly GUILayoutOption[] RewardColumnOptions =
        {
            GUILayout.MinWidth(REWARD_COLUMN_WIDTH), GUILayout.MaxWidth(REWARD_COLUMN_WIDTH * 2),
            GUILayout.ExpandWidth(true)
        };

        private DroneTrainManager _droneTrainManager;
        private TableMode         _tableMode;
        private float[,]          _rewardsTable;
        private string[]          _rewardsNames;
        private bool              _statsExpanded;

        private void OnEnable() { _droneTrainManager = (DroneTrainManager)target; }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Update drone agents"))
                    _droneTrainManager.UpdateDrones();

                if (GUILayout.Button("Clear drone agents"))
                    _droneTrainManager.ClearDrones();
            }

            EditorGUILayout.Space(10);
            RewardsStats();
        }

        private void RewardsStats()
        {
            _statsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_statsExpanded, "Rewards Statistics");
            if (!_statsExpanded)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            if (_droneTrainManager.DroneAgents.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Found no active drone agents in children nodes.\n" +
                    "Click 'Update drone agents' to fix this.",
                    MessageType.Warning
                );

                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            EditorGUILayout.LabelField($"Active drone agents count: {_droneTrainManager.DroneAgents.Count}");

            if ((_droneTrainManager.DroneAgents.First().RewardProvider?.RewardsCount ?? 0) == 0)
            {
                EditorGUILayout.HelpBox(
                    "Drone agents doesn't have configured RewardsProviders.\n" +
                    "Click 'Update drone agents' to fix this.",
                    MessageType.Warning
                );

                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            _tableMode = (TableMode)EditorGUILayout.EnumPopup("Table mode", _tableMode);
            switch (_tableMode)
            {
            case TableMode.LastReward:
                InitLastRewardsTable();
                break;
            case TableMode.CumulativeReward:
                InitCumulativeRewardsTable();
                break;
            case TableMode.Summary:
                InitSummaryTable();
                break;
            }

            EditorGUILayout.Space(5);
            DrawRewardsTable();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRewardsTable()
        {
            using var h = new EditorGUILayout.HorizontalScope();

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("", AgentColumnOptions);
                foreach (var agent in _droneTrainManager.DroneAgents)
                {
                    if (agent.IsHeuristicsOnly)
                        EditorGUILayout.LabelField(agent.drone.name, EditorStyles.boldLabel, AgentColumnOptions);
                    else
                        EditorGUILayout.LabelField(agent.drone.name, AgentColumnOptions);
                }
            }

            for (var col = 0; col < _rewardsTable.GetLength(1); col++)
            {
                using var v = new EditorGUILayout.VerticalScope();

                EditorGUILayout.LabelField(_rewardsNames[col], EditorStyles.miniLabel, RewardColumnOptions);
                var enabled = GUI.enabled;
                GUI.enabled = false;

                for (var row = 0; row < _rewardsTable.GetLength(0); row++)
                    EditorGUILayout.FloatField(_rewardsTable[row, col], RewardColumnOptions);

                GUI.enabled = enabled;
            }
        }

        private void InitLastRewardsTable()
        {
            if (_rewardsTable == null ||
                _rewardsTable.GetLength(0) != _droneTrainManager.DroneAgents.Count ||
                _rewardsTable.GetLength(1) != _droneTrainManager.DroneAgents.First().RewardProvider.RewardsCount)
            {
                _rewardsTable = new float[
                        _droneTrainManager.DroneAgents.Count,
                        _droneTrainManager.DroneAgents.First().RewardProvider.RewardsCount
                    ];

                _rewardsNames = new string[_droneTrainManager.DroneAgents.First().RewardProvider.RewardsCount];
            }

            var agentIdx = 0;
            foreach (var agent in _droneTrainManager.DroneAgents)
            {
                var rewardIdx = 0;
                foreach (var reward in agent.RewardProvider.GetRewards())
                {
                    if (agentIdx == 0)
                        _rewardsNames[rewardIdx] = reward.RewardName;

                    _rewardsTable[agentIdx, rewardIdx] = reward.LastReward;
                    rewardIdx++;
                }

                agentIdx++;
            }
        }

        private void InitCumulativeRewardsTable()
        {
            if (_rewardsTable == null ||
                _rewardsTable.GetLength(0) != _droneTrainManager.DroneAgents.Count ||
                _rewardsTable.GetLength(1) != _droneTrainManager.DroneAgents.First().RewardProvider.RewardsCount)
            {
                _rewardsTable = new float[
                        _droneTrainManager.DroneAgents.Count,
                        _droneTrainManager.DroneAgents.First().RewardProvider.RewardsCount
                    ];

                _rewardsNames = new string[_droneTrainManager.DroneAgents.First().RewardProvider.RewardsCount];
            }

            var agentIdx = 0;
            foreach (var agent in _droneTrainManager.DroneAgents)
            {
                var rewardIdx = 0;
                foreach (var reward in agent.RewardProvider.GetRewards())
                {
                    if (agentIdx == 0)
                        _rewardsNames[rewardIdx] = reward.RewardName;

                    _rewardsTable[agentIdx, rewardIdx] = reward.CumulativeReward;
                    rewardIdx++;
                }

                agentIdx++;
            }
        }

        private void InitSummaryTable()
        {
            if (_rewardsTable == null ||
                _rewardsTable.GetLength(0) != _droneTrainManager.DroneAgents.Count ||
                _rewardsTable.GetLength(1) != 2)
            {
                _rewardsTable = new float[_droneTrainManager.DroneAgents.Count, 2];
                _rewardsNames = new string[2];

                _rewardsNames[0] = "Last";
                _rewardsNames[1] = "Cumulative";
            }

            var agentIdx = 0;
            foreach (var agent in _droneTrainManager.DroneAgents)
            {
                _rewardsTable[agentIdx, 0] = agent.RewardProvider.LastReward;
                _rewardsTable[agentIdx, 1] = agent.RewardProvider.CumulativeReward;
                agentIdx++;
            }
        }
    }
}