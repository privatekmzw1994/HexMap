using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedPath : MonoBehaviour
{
    public bool createPathLine = true;
    public float dynamicOffset = 1;
    public bool loop = false;
    public int loopPoint = 0;

    [HideInInspector] private bool isLinearPath = true;
    public bool IsLinearPath() { return isLinearPath; }

    public List<Transform> nodeList = new List<Transform>();
    public List<PathNode> pathNodeList = new List<PathNode>();

    public void Init()
    {
        pathNodeList = new List<PathNode>();
        for (int i = 0; i < nodeList.Count; i++)
        {
            Transform pathnode = nodeList[i];
            if (nodeList != null)
            {
                PathNode node = new PathNode();
                node.pathNodeT = pathnode;

                if (true)
                {
                    node.isOnHexCell = true;
                    node.hexCell = pathnode.gameObject.GetComponent<HexCell>();
                    //node.
                }
                pathNodeList.Add(node);
            }
            else
            {
                nodeList.RemoveAt(i);
                i -= 1;
            }
        }
    }
}

public class PathNode
{
    public List<HexCell> pathNodeList = new List<HexCell>();
    public Transform pathNodeT;
    public bool isOnHexCell = true;
    public HexCell hexCell;
    public int nodeIDOnHexCell = 0;
}
