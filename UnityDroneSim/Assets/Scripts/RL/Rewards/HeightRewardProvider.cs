using System;
using RL.RewardsSettings;
using Unity.MLAgents;
using UnityEngine;
using UtilsDebug;


namespace RL.Rewards
{
    public class HeightRewardProvider : RewardProvider
    {
        private readonly HeightRewardSettings _settings;
        private readonly Agent _agent;

        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];
        private readonly int _layerMask;
        private Ray _rayDown;
        
        private GizmoOptions _debugGizmo = new (Color.cyan, capSize: 0)
        {
            LabelPlacement = GizmoLabelPlacement.Center,
            LabelSize = 0.75f,
            VectCapSize = 0.05f
        };
     
        /// <summary>Agent had correct height during last environment step.</summary>
        public bool IsInHeightRange { get; private set; }
        
        /// <summary>Agent's height.</summary>
        public float Height {get; private set;}
        

        public HeightRewardProvider(HeightRewardSettings settings, Agent agent)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            _layerMask = LayerMask.GetMask("Default");
            _rayDown = new Ray(agent.transform.position, Vector3.down);
        }
        
        public override float CalculateReward()
        {
            if (_settings.useRaycastHeight)
            {
                _rayDown.origin = _agent.transform.position;
                var hits = Physics.RaycastNonAlloc(_rayDown, _raycastHits, _settings.maxHeight * 2, _layerMask);
                Height = hits == 0 ? _settings.maxHeight + 1 : _raycastHits[0].distance;
            }
            else
            {
                Height = _agent.transform.position.y;
            }

            if (Height < _settings.minHeight || Height > _settings.maxHeight)
            {
                IsInHeightRange = false;
                return UpdateRewards(_settings.outOfHeightRangePenalty);
            }

            IsInHeightRange = true;
            return UpdateRewards(_settings.heightReward);
        }

        public override void DrawGizmos()
        {
            _debugGizmo.Color = IsInHeightRange ? Color.cyan : Color.red;
            _debugGizmo.LabelColor = IsInHeightRange ? Color.cyan : Color.red;

            VectorDrawer.DrawDirection(_agent.transform.position, 
                Vector3.down * Height, 
                $"Height: {Height:F1} m\nR: {LastReward:F3}",
                _debugGizmo);
        }
    }
}