using System;
using UnityEngine;

namespace Core.Health
{
    public class DamageableBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The Damageable object
        /// </summary>
        public Damageable configuration;

        /// <summary>
        /// Gets whether this <see cref="DamageableBehaviour" /> is dead.
        /// </summary>
        /// <value>True if dead</value>
        public bool isDead
        {
            get { return configuration.isDead; }
        }

        public virtual Vector3 position
        {
            get { return transform.position; }
        }

        public event Action<HitInfo> hit;

        /// <summary>
		/// Event that is fired when this instance is removed, such as when pooled or destroyed
		/// </summary>
		public event Action<DamageableBehaviour> removed;

        /// <summary>
        /// Event that is fired when this instance is killed
        /// </summary>
        public event Action<DamageableBehaviour> died;

        /// <summary>
        /// Takes the damage and also provides a position for the damage being dealt
        /// </summary>
        /// <param name="damageValue">Damage value.</param>
        /// <param name="damagePoint">Damage point.</param>
        /// <param name="alignment">Alignment value</param>
        public virtual void TakeDamage(float damageValue, Vector3 damagePoint, IAlignmentProvider alignment)
        {
            HealthChangeInfo info;
            configuration.TakeDamage(damageValue, alignment, out info);
            var damageInfo = new HitInfo(info, damagePoint);
            if (hit != null)
            {
                hit(damageInfo);
            }
        }

        protected virtual void Awake()
        {
            configuration.Init();
            configuration.died += OnConfigurationDied;
        }

        /// <summary>
		/// Kills this damageable
		/// </summary>
		protected virtual void Kill()
        {
            HealthChangeInfo healthChangeInfo;
            configuration.TakeDamage(configuration.currentHealth, null, out healthChangeInfo);
        }

        /// <summary>
        /// Removes this damageable without killing it
        /// </summary>
        public virtual void Remove()
        {
            // Set health to zero so that this behaviour appears to be dead. This will not fire death events
            configuration.SetHealth(0);
            OnRemoved();
        }

        /// <summary>
		/// 击杀
		/// </summary>
		void OnDeath()
        {
            if (died != null)
            {
                died(this);
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        void OnRemoved()
        {
            if (removed != null)
            {
                removed(this);
            }
        }

        /// <summary>
        /// Event fired when Damageable takes critical damage
        /// </summary>
        void OnConfigurationDied(HealthChangeInfo changeInfo)
        {
            OnDeath();
            Remove();
        }
    }
}

