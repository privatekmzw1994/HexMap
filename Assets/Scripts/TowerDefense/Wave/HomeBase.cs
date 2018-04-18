using Core.Health;
using System.Collections;
using System.Collections.Generic;
using Units.Enemy;
using UnityEngine;
using System;

namespace TowerDefense.Wave
{
    public class HomeBase : DamageableBehaviour
    {
        /// <summary>
        /// The current enemies within the home base attack zone
        /// </summary>
        protected List<Enemy> m_CurrentEnemiesInside = new List<Enemy>();

        /// <summary>
        /// Subscribes to damaged event
        /// </summary>
        protected virtual void Start()
        {
            configuration.damaged += OnDamaged;
        }

        /// <summary>
        /// Unsubscribes to damaged event
        /// </summary>
        protected virtual void OnDestroy()
        {
            configuration.damaged -= OnDamaged;
        }

        protected virtual void OnDamaged(HealthChangeInfo obj)
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            var homeBaseAttacker = other.GetComponent<HomeBaseAttacker>();
            if (homeBaseAttacker == null)
            {
                return;
            }
            m_CurrentEnemiesInside.Add(homeBaseAttacker.enemy);
            homeBaseAttacker.enemy.removed += OnAgentRemoved;
        }

        void OnTriggerExit(Collider other)
        {
            var homeBaseAttacker = other.GetComponent<HomeBaseAttacker>();
            if (homeBaseAttacker == null)
            {
                return;
            }
            RemoveTarget(homeBaseAttacker.enemy);
        }

        void RemoveTarget(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }
            m_CurrentEnemiesInside.Remove(enemy);
        }

        void OnAgentRemoved(DamageableBehaviour targetable)
        {
            targetable.removed -= OnAgentRemoved;
            Enemy attackingAgent = targetable as Enemy;
            RemoveTarget(attackingAgent);
        }

        
    }
}