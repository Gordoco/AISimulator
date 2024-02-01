using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Struct for containing the node and depth information during recursive tree searches
 */
public struct NodeDepth
{
    public NodeDepth(QuadTreeNode inNode, int indepth) { node = inNode; depth = indepth; }
    public QuadTreeNode node;
    public int depth;
    public int CompareTo(NodeDepth other, Vector2 location)
    {
        if (other.node == null) return 1;
        if ((node.GetCenterPoint() - location).magnitude > (other.node.GetCenterPoint() - location).magnitude) return 1;
        if ((node.GetCenterPoint() - location).magnitude == (other.node.GetCenterPoint() - location).magnitude) return 0;
        else return -1;
    }
}