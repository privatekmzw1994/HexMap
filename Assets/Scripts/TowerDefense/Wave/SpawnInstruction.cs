using System;
using TowerDefense.Nodes;
using Units.Enemy.Data;
using UnityEngine;

namespace TowerDefense.Wave
{
	/// <summary>
	/// Serializable class for specifying properties of spawning an agent
	/// </summary>
	[Serializable]
	public class SpawnInstruction
	{
		/// <summary>
		/// �����ĵ��� - i.e. the monster for the wave
		/// </summary>
		public EnemyConfiguration agentConfiguration;

        /// <summary>
        /// ���ϴγ��ֵ���γ��ֵ��ӳ�
        /// </summary>
        [Tooltip("���ϴγ��ֵ���γ��ֵ��ӳ�")]
		public float delayToSpawn;

		/// <summary>
		/// ���ֵ�
		/// </summary>
		public Node startingNode;
	}
}