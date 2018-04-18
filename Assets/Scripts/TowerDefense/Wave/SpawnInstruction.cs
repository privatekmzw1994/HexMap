using System;
using TowerDefense.Nodes;
using Units.Enemy.Data;
using UnityEngine;

namespace TowerDefense.Wave
{
	/// <summary>
	/// Serializable class for specifying properties of spawning an enemy
	/// </summary>
	[Serializable]
	public class SpawnInstruction
	{
		/// <summary>
		/// 产生的敌人 - i.e. the monster for the wave
		/// </summary>
		public EnemyConfiguration agentConfiguration;

        /// <summary>
        /// 从上次出怪到这次出怪的延迟
        /// </summary>
        [Tooltip("从上次出怪到这次出怪的延迟")]
		public float delayToSpawn;

		/// <summary>
		/// 出怪点
		/// </summary>
		public Node startingNode;
	}
}