using Units.Enemy;
using UnityEngine;

namespace TowerDefense.Nodes
{
    [RequireComponent(typeof(Collider))]
    public class Node : MonoBehaviour
    {
        /// <summary>
        /// Gets the next node from the selector
        /// </summary>
        /// <returns>Next node, or null if this is the terminating node</returns>
        public Node GetNextNode()
        {
            var selector = GetComponent<NodeSelector>();
            if (selector != null)
            {
                return selector.GetNextNode();
            }
            return null;
        }

        /// <summary>
        /// When enemy enters the node area, get the next node
        /// </summary>
        public virtual void OnTriggerEnter(Collider other)
        {
            var agent = other.gameObject.GetComponent<Enemy>();
            if (agent != null)
            {
                agent.GetNextNode(this);
            }
        }
    }

}

