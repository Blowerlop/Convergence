using System;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts
{
    public class Projectile : NetworkBehaviour
    {
        private AttackController _attackController;
        private Transform _target;
        private float _speed;


        public override void OnNetworkSpawn()
        {
            enabled = IsServer;
        }

        private void FixedUpdate()
        {
            if (IsSpawned == false) return;
            
            transform.LookAt(_target);
            Vector3 direction = _target.position - transform.position;
            Vector3 directionNormalized = direction.normalized;
            
            transform.position += directionNormalized * (Time.fixedDeltaTime * _speed); 
            if (direction.sqrMagnitude < SOProjectile.HIT_RANGE * SOProjectile.HIT_RANGE || (directionNormalized * (Time.fixedDeltaTime * _speed)).magnitude > direction.magnitude)
            {
                // Projectile follow an IDamageable target. Don't need to check if it's null.
                _attackController.Hit(_target.GetComponent<IDamageable>());
                GetComponent<NetworkObject>().Despawn();
            }
        }
        
        
        public void Init(AttackController attackController, SOProjectile projectileData)
        {
            _attackController = attackController;
            _target = attackController.targetNetworkObject.transform;
            _speed = projectileData._speed;
        }
    }
}