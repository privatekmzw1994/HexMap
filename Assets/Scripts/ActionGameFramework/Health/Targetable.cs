using System;
using Core.Health;
using UnityEngine;

namespace ActionGameFramework.Health
{
    /// <summary>
    /// 可被瞄准类  一般是敌人
    /// </summary>
    public class Targetable : DamageableBehaviour
    {
        public Transform targetTransform;

        protected Vector3 m_CurrentPosition, m_PreviousPosition;

        public virtual Vector3 velocity { get; protected set; }

        public Transform targetableTransform
        {
            get
            {
                return targetTransform == null ? transform : targetTransform;
            }
        }

        public override Vector3 position
        {
            get
            {
                return targetableTransform.position;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            ResetPositionData();
        }

        protected void ResetPositionData()
        {
            m_CurrentPosition = position;
            m_PreviousPosition = position;
        }

        /// <summary>
        /// Calculates the velocity and updates the position
        /// </summary>
        void FixedUpdate()
        {
            m_CurrentPosition = position;
            velocity = (m_CurrentPosition - m_PreviousPosition) / Time.fixedDeltaTime;
            m_PreviousPosition = m_CurrentPosition;
        }
    }
}

