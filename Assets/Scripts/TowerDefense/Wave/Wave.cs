using Core.Utilities;
using System;
using System.Collections.Generic;
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
        protected RepeatingTimer m_spawnTimer;

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

        }
    }
}