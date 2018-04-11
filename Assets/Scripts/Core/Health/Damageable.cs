using System;
using UnityEngine;


namespace Core.Health
{
    /// <summary>
    /// 可损害类
    /// 用于各种单位(敌人、我方)，可破坏的物体
    /// </summary>
    [SerializeField]
    public class Damageable
    {
        public float maxHealth;

        public float startingHealth;

        public SerializableIAlignmentProvider alignment;

        /// <summary>
        /// 获取当前生命值
        /// </summary>
        public float currentHealth { protected set; get; }

        /// <summary>
        /// 获取正常化生命值(currentHealth / maxHealth)
        /// </summary>
        public float normalisedHealth
        {
            get
            {
                if (Math.Abs(maxHealth)<=Mathf.Epsilon)
                {
                    Debug.LogError("Max Health is 0.Set maxHealth = 1f");
                    maxHealth = 1f;
                }
                return currentHealth / maxHealth;
            }           
        }

        public IAlignmentProvider alignmentProvider
        {
            get
            {
                return alignment != null ? alignment.GetInterface() : null;
            }
        }

        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool isDead
        {
            get { return currentHealth <= 0f; }
        }

        /// <summary>
        /// 是否满血
        /// </summary>
        public bool isAtMaxHealth
        {
            get { return Mathf.Approximately(currentHealth, maxHealth); }
        }

        public event Action reachedMaxHealth;

        public event Action<HealthChangeInfo> damaged, healed, died, healthChanged;

        public virtual void Init()
        {
            currentHealth = startingHealth;
        }

        /// <summary>
        /// 统一赋值起始生命值和最大生命值
        /// </summary>
        /// <param name="health"></param>
        public void SetMaxHealth(float health)
        {
            if (health <= 0)
            {
                return;
            }
            maxHealth = startingHealth = health;
        }

        /// <summary>
        /// 分开赋值最大生命值和起始生命值
        /// </summary>
        /// <param name="health"></param>
        /// <param name="startingHealth"></param>
        public void SetMaxHealth(float health, float startingHealth)
        {
            if (health <= 0)
            {
                return;
            }
            maxHealth = health;
            this.startingHealth = startingHealth;
        }

        /// <summary>
        /// 赋值当前生命值
        /// </summary>
        /// <param name="health">
        /// The value to set <see cref="currentHealth"/> to
        /// </param>
        public void SetHealth(float health)
        {
            var info = new HealthChangeInfo
            {
                damageable = this,
                newHealth = health,
                oldHealth = currentHealth
            };

            currentHealth = health;

            if (healthChanged != null)
            {
                healthChanged(info);
            }
        }

        /// <summary>
        /// Use the alignment to see if taking damage is a valid action
        /// </summary>
        /// <param name="damage">
        /// The damage to take
        /// </param>
        /// <param name="damageAlignment">
        /// The alignment of the other combatant
        /// </param>
        /// <param name="output">
        /// The output data if there is damage taken
        /// </param>
        /// <returns>
        /// <value>true if this instance took damage</value>
        /// <value>false if this instance was already dead, or the alignment did not allow the damage</value>
        /// </returns>
        public bool TakeDamage(float damage, IAlignmentProvider damageAlignment, out HealthChangeInfo output)
        {
            output = new HealthChangeInfo
            {
                damageAlignment = damageAlignment,
                damageable = this,
                newHealth = currentHealth,
                oldHealth = currentHealth
            };

            bool canDamage = damageAlignment == null || alignmentProvider == null ||
                             damageAlignment.CanHarm(alignmentProvider);

            if (isDead || !canDamage)
            {
                return false;
            }

            ChangeHealth(-damage, output);
            SafelyDoAction(damaged, output);
            if (isDead)
            {
                SafelyDoAction(died, output);
            }
            return true;
        }

        /// <summary>
        /// Logic for increasing the health.
        /// </summary>
        /// <param name="health">Health.</param>
        public HealthChangeInfo IncreaseHealth(float health)
        {
            var info = new HealthChangeInfo { damageable = this };
            ChangeHealth(health, info);
            SafelyDoAction(healed, info);
            if (isAtMaxHealth)
            {
                SafelyDoAction(reachedMaxHealth);
            }

            return info;
        }

        /// <summary>
        /// Changes the health.
        /// </summary>
        /// <param name="healthIncrement">Health increment.</param>
        /// <param name="info">HealthChangeInfo for this change</param>
        protected void ChangeHealth(float healthIncrement, HealthChangeInfo info)
        {
            info.oldHealth = currentHealth;
            currentHealth += healthIncrement;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            info.newHealth = currentHealth;

            if (healthChanged != null)
            {
                healthChanged(info);
            }
        }

        /// <summary>
        /// A helper method for null checking actions
        /// </summary>
        /// <param name="action">Action to be done</param>
        protected void SafelyDoAction(Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        /// <summary>
        /// A helper method for null checking actions
        /// </summary>
        /// <param name="action">Action to be done</param>
        /// <param name="info">The HealthChangeInfo to be passed to the Action</param>
        protected void SafelyDoAction(Action<HealthChangeInfo> action, HealthChangeInfo info)
        {
            if (action != null)
            {
                action(info);
            }
        }
    }

}
