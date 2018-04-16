using UnityEngine;

namespace Units.Enemy.Data
{
	[CreateAssetMenu(fileName = "EnemyConfiguration.asset", menuName = "TowerDefense/Enemy Configuration", order = 1)]
	public class EnemyConfiguration : ScriptableObject
	{
		/// <summary>
		/// 敌人名称
		/// </summary>
		public string enemyName;

		/// <summary>
		/// 敌人简介
		/// </summary>
		[Multiline]
		public string agentDescription;

		/// <summary>
		/// The Agent prefab that will be used on instantiation
		/// </summary>
		public Enemy EnemyPrefab;
	}
}