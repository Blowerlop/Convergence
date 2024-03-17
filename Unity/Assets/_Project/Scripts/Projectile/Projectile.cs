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
            
            Vector3 direction = (_target.position - transform.position).normalized;
            
            transform.position += direction * (Time.fixedDeltaTime * _speed);
            transform.LookAt(_target);
        }
        
        
        public void Init(AttackController attackController, SOProjectile projectileData)
        {
            Instantiate(projectileData.modelPrefab, transform);
            _attackController = attackController;
            _target = attackController.targetNetworkObject.transform;
            _speed = projectileData._speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_attackController.IsDamageable(other.transform, out IDamageable damageable))
            {
                _attackController.Hit(damageable);
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }
}