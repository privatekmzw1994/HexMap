using System.Collections.Generic;
using UnityEngine;


namespace TowerDefense.Nodes
{
    public abstract class NodeSelector : MonoBehaviour
    {
        /// <summary>
        /// 路径点列
        /// </summary>
        public List<Node> linkedNodes;

        /// <summary>
        /// 获取下一个路径点
        /// </summary>
        /// <returns>下一个路径点，如果是最后一个路径点则返回null</returns>
        public abstract Node GetNextNode();

#if UNITY_EDITOR

        /// <summary>
        /// 路径可视化(unity编辑模式下)
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (linkedNodes == null)
            {
                return;
            }
            int count = linkedNodes.Count;
            for (int i = 0; i < count; i++)
            {
                Node node = linkedNodes[i];
                if (node != null)
                {
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
        }
#endif
    }
}

