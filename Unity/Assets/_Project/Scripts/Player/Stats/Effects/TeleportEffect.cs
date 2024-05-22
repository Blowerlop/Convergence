using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Effects
{
    public class TeleportEffect : Effect
    {
        public override EffectType Type => EffectType.Neutral;

        [SerializeField] private float _distanceToTarget;
        
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier)
        {
            var targetTransform = effectable.AffectedEntity.transform;
            
            var dirToTarget = targetTransform.position - applier.PlayerTransform.position;
            dirToTarget.y = 0;
            
            dirToTarget -= dirToTarget.normalized * _distanceToTarget;

            var pos = applier.PlayerTransform.position + dirToTarget;
            int maxAttemps = 20;

            var index = 0;
            NavMeshHit hit;
            
            while (!NavMesh.SamplePosition(pos, out hit, 0.5f, NavMesh.AllAreas) && index < maxAttemps)
            {
                var dir = Random.insideUnitCircle.normalized * _distanceToTarget;
                var toV3 = new Vector3(dir.x, 0, dir.y);
                
                pos = targetTransform.position + toV3;
                index++;
            }
            
            if (index >= maxAttemps)
            {
                Debug.LogError("Couldn't find a valid position to teleport! Target might be outside of the navmesh.");
                return false;
            }
            
            applier.PlayerTransform.GetComponent<NetworkTransform>()
                .Teleport(hit.position, Quaternion.LookRotation(targetTransform.position - hit.position), Vector3.one);

            var navMeshAgent = applier.GetPC().NavMeshAgent;
            
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            
            navMeshAgent.Warp(hit.position);
            
            return true;
        }

        public override void KillEffect() { }

        public override Effect GetInstance()
        {
            return this;
        }
    }
}