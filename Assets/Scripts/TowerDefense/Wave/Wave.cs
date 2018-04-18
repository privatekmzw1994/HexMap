using Core.Extensions;
using Core.Utilities;
using System;
using System.Collections.Generic;
using TowerDefense.Nodes;
using Units.Enemy;
using Units.Enemy.Data;
using UnityEngine;

namespace TowerDefense.Wave
{
    public class Wave : TimedBehaviour
    {
        /// <summary>
        /// 出怪指令队列
        /// </summary>
        public List<SpawnInstruction> spawnInstructions;

        protected int m_CurrentIndex;

        /// <summary>
        /// 出怪间隔 (重复计时器)
        /// </summary>
        protected RepeatingTimer m_SpawnTimer;

        /// <summary>
        /// 波次完成
        /// </summary>
        public event Action waveCompleted;

        /// <summary>
        /// 波次进度
        /// </summary>
        public virtual float progress
        {
            get { return (float)(m_CurrentIndex) / spawnInstructions.Count; }
        }

        /// <summary>
        /// 初始化波次
        /// </summary>
        public virtual void Init()
        {
            if (spawnInstructions.Count == 0)
            {
                Debug.LogWarning("[LEVEL] Empty Wave");
                SafelyBroadcastWaveCompletedEvent();
                return;
            }

            m_SpawnTimer = new RepeatingTimer(spawnInstructions[0].delayToSpawn, SpawnCurrent);
            StartTimer(m_SpawnTimer);
        }

        protected void Spawn()
        {
            SpawnInstruction spawnInstruction = spawnInstructions[m_CurrentIndex];
            SpawnEnemy(spawnInstruction.agentConfiguration, spawnInstruction.startingNode);
        }

        /// <summary>
        /// Tries to setup the next spawn
        /// </summary>
        /// <returns>true if there is another spawn instruction, false if not</returns>
        protected bool TrySetupNextSpawn()
        {
            bool hasNext = spawnInstructions.Next(ref m_CurrentIndex);
            if (hasNext)
            {
                SpawnInstruction nextSpawnInstruction = spawnInstructions[m_CurrentIndex];
                if (nextSpawnInstruction.delayToSpawn <= 0f)
                {
                    SpawnCurrent();
                }
                else
                {
                    m_SpawnTimer.SetTime(nextSpawnInstruction.delayToSpawn);
                }
            }

            return hasNext;
        }

        /// <summary>
		/// Handles spawning the current enemy and sets up the next enemy for spawning
		/// </summary>
		protected virtual void SpawnCurrent()
        {
            Spawn();
            if (!TrySetupNextSpawn())
            {
                SafelyBroadcastWaveCompletedEvent();
                // this is required so wave progress is still accurate
                m_CurrentIndex = spawnInstructions.Count;
                StopTimer(m_SpawnTimer);
            }
        }

        /// <summary>
        /// spawn the enemy
        /// </summary>
        /// <param name="enemyconfig">怪</param>
        /// <param name="node">出怪点</param>
        protected virtual void SpawnEnemy(EnemyConfiguration enemyconfig,Node node)
        {
            Vector3 spawnPosition = node.transform.position;

            var poolable = Poolable.TryGetPoolable<Poolable>(enemyconfig.EnemyPrefab.gameObject);
            if (poolable==null)
            {
                return;
            }
            var enemyInstance = poolable.GetComponent<Enemy>();
            enemyInstance.transform.position = spawnPosition;
            enemyInstance.Initialize();
            enemyInstance.SetNode(node);
            enemyInstance.transform.rotation = node.transform.rotation;
        }

        /// <summary>
        /// Launch the waveCompleted event
        /// </summary>
        protected void SafelyBroadcastWaveCompletedEvent()
        {
            if (waveCompleted != null)
            {
                waveCompleted();
            }
        }
    }
}