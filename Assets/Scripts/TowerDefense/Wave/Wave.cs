using Core.Utilities;
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
    }
}