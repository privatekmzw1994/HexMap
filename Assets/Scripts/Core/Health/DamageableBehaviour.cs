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


    }
}

