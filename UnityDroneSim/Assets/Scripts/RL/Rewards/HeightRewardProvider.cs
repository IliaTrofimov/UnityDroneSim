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
        private readonly int _layerMask;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];
        private Ray _rayDown;
     
        public bool IsInHeightRange {get; private set;}
        
        public HeightRewardProvider(HeightRewardSettings settings, Agent agent)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _agent = agent;
            _layerMask = LayerMask.GetMask("Default");
            _rayDown = new Ray(agent.transform.position, Vector3.down);
        }
        
        public override float CalculateReward()
        {
            float height;
            if (_settings.useRaycastHeight)
            {
                _rayDown.origin = _agent.transform.position;
                var hits = Physics.RaycastNonAlloc(_rayDown, _raycastHits, _settings.maxHeight * 2, _layerMask);
                height = hits == 0 ? _settings.maxHeight + 1 : _raycastHits[0].distance;
            }
            else
            {
                height = _agent.transform.position.y;
            }

            if (height < _settings.minHeight || height > _settings.maxHeight)
            {
                IsInHeightRange = false;
                return UpdateRewards(_settings.outOfHeightRangePenalty);
            }

            IsInHeightRange = true;
            return UpdateRewards(_settings.heightReward);
        }

        public override void DrawGizmos()
        {
            var options = new GizmoOptions(IsInHeightRange ? Color.cyan : Color.red, capSize: 0);
            VectorDrawer.DrawDirection(_agent.transform.position, 
                Vector3.down * _settings.maxHeight, 
                "",
                options);
        }
    }
}