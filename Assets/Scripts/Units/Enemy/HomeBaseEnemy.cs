using ActionGameFramework.Health;
using Core.Health;
using Core.Utilities;
using TowerDefense.Nodes;
using UnityEngine;

namespace Units.Enemy
{
    /// <summary>
    /// A component that attacks a home base when an enemy reaches it 
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class HomeBaseAttacker : MonoBehaviour
    {
        /// <summary>
        /// How long the enemy charges for before it attacks
        /// the home base
        /// </summary>
        public float homeBaseAttackChargeTime = 0.5f;

        /// <summary>
        /// Timer used to stall attack to the home base
        /// </summary>
        protected Timer m_HomeBaseAttackTimer;

        /// <summary>
        /// If the enemy has reached the Player Home Base and is charging an attack
        /// </summary>
        protected bool m_IsChargingHomeBaseAttack;

        /// <summary>
        /// The DamageableBehaviour on the home base 
        /// </summary>
        protected DamageableBehaviour m_FinalDestinationDamageableBehaviour;

        /// <summary>
        /// The enemy component attached to this gameObject
        /// </summary>
        public Enemy enemy { get; protected set; }

        /// <summary>
        /// Fired on completion of <see cref="m_HomeBaseAttackTimer"/>
        /// Applies damage to the homebase
        /// </summary>
        protected void AttackHomeBase()
        {
            m_IsChargingHomeBaseAttack = false;
            var damager = GetComponent<Damager>();
            if (damager != null)
            {
                m_FinalDestinationDamageableBehaviour.TakeDamage(damager.damage, transform.position, enemy.configuration.alignmentProvider);
            }
            enemy.Remove();
        }

        /// <summary>
        /// Ticks the attack timer
        /// </summary>
        protected virtual void Update()
        {
            // Update HomeBaseAttack Timer
            if (m_IsChargingHomeBaseAttack)
            {
                m_HomeBaseAttackTimer.Tick(Time.deltaTime);
            }
        }

        /// <summary>
        /// Caches the attached Agent and subscribes to the destinationReached event
        /// </summary>
        protected virtual void Awake()
        {
            enemy = GetComponent<Enemy>();
            enemy.destinationReached += OnDestinationReached;
            enemy.died += OnDied;
        }

        /// <summary>
        /// Unsubscribes from the destinationReached event
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (enemy != null)
            {
                enemy.destinationReached -= OnDestinationReached;
                enemy.died -= OnDied;
            }
        }

        /// <summary>
        /// Stops the attack on the home base
        /// </summary>
        void OnDied(DamageableBehaviour damageableBehaviour)
        {
            m_IsChargingHomeBaseAttack = false;
        }

        /// <summary>
        /// Fired then the enemy reached its final node,
        /// Starts the attack timer
        /// </summary>
        /// <param name="homeBase"></param>
        void OnDestinationReached(Node homeBase)
        {
            m_FinalDestinationDamageableBehaviour = homeBase.GetComponent<DamageableBehaviour>();
            // start timer 
            if (m_HomeBaseAttackTimer == null)
            {
                m_HomeBaseAttackTimer = new Timer(homeBaseAttackChargeTime, AttackHomeBase);
            }
            else
            {
                m_HomeBaseAttackTimer.Reset();
            }
            m_IsChargingHomeBaseAttack = true;
        }
    }
}
