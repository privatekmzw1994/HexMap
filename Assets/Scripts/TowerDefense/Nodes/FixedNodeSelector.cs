using Core.Extensions;
using UnityEngine;

namespace TowerDefense.Nodes
{
    public class FixedNodeSelector : NodeSelector
    {
        protected int m_NodeIndex;

        public override Node GetNextNode()
        {
            if (linkedNodes.Next(ref m_NodeIndex, true))
            {
                return linkedNodes[m_NodeIndex];
            }
            return null;
        }
#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            base.OnDrawGizmos();
        }
#endif
    }
}

