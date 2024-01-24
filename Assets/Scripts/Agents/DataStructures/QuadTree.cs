using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * #### QuadTree
 * -----
 * A data structure class which interfaces with the DynamicCoordinateGrid to create a QuadTree for pathfinding
 */
public class QuadTree
{
    public int MinSize = 1;

    private QuadTreeNode root;
    private Vector2 Origin;

    /**
     * #### void Construct(DynamicCoordinateGrid, Vector3)
     * Creates the physical QuadTree by initializing one base quad on the entire grid and recursivly partitioning that quad
     */
    public void Construct(DynamicCoordinateGrid mapping, Vector3 offset, float time = 1)
    {
        root = new QuadTreeNode(offset.x + mapping.gridCorner[0], offset.z + mapping.gridCorner[1], 
            mapping.width, mapping.height);
        root.tree = this;
        Origin = new Vector2(offset.x + mapping.gridCorner[0], offset.z + mapping.gridCorner[1]);
        Partition(root, mapping);
        Print(time);
    }

    public QuadTreeNode GetNode(Vector2 location)
    {
        return root.GetNode(location);
    }

    public bool IsChild(QuadTreeNode potChild, QuadTreeNode parent)
    {
        if (potChild == parent) return true;
        while (potChild.parent != null)
        {
            potChild = potChild.parent;
            if (potChild == parent) return true;
        }
        return false;
    }

    /**
     * #### void Print()
     * Mainly debugging method to visualize the entire QuadTree in 3D space overlayed on its position in the x and z dimensions
     */
    public void Print(float time)
    {
        root.Print(time);
    }

    /**
     * #### void Partition(QuadTreeNode, DynamicCoordinateGrid)
     * Recursive method for splitting quads depending on the result of the MustBeSubdivided method
     */
    void Partition(QuadTreeNode node, DynamicCoordinateGrid mapping)
    {
        if (MustBeSubdivided(node, mapping))
        {
            node.SW = new QuadTreeNode(node.x, node.y, node.w / 2, node.h / 2);
            node.SW.nodeLoc = 3;
            node.SW.parent = node;
            node.SW.tree = this;

            node.SE = new QuadTreeNode(node.x + node.w / 2, node.y, node.w / 2, node.h / 2);
            node.SE.nodeLoc = 2;
            node.SE.parent = node;
            node.SE.tree = this;

            node.NW = new QuadTreeNode(node.x, node.y + node.h / 2, node.w / 2, node.h / 2);
            node.NW.nodeLoc = 1;
            node.NW.parent = node;
            node.NW.tree = this;

            node.NE = new QuadTreeNode(node.x + node.w / 2, node.y + node.h / 2, node.w / 2, node.h / 2);
            node.NE.nodeLoc = 0;
            node.NE.parent = node;
            node.NE.tree = this;

            Partition(node.NW, mapping);
            Partition(node.NE, mapping);
            Partition(node.SW, mapping);
            Partition(node.SE, mapping);
        }
    }

    /**
     * #### bool MustBeSubdivided(QuadTreeNode, DynamicCoordinateGrid)
     * A boolean algorithm which compares the points within a quad with their mapping on the agent grid.
     * Determines if a split is needed which is then propogated by Partition
     */
    bool MustBeSubdivided(QuadTreeNode node, DynamicCoordinateGrid mapping)
    {
        bool foundValid = false;
        bool foundInvalid = false;

        for (int i = (int)node.x; i < node.x + node.w; i++)
        {
            for (int j = (int)node.y; j < node.y + node.h; j++)
            {
                MappingIDs map = mapping.GetMapping((int)(i - Origin.x), (int)(j - Origin.y));
                if (map == MappingIDs.Full || map == MappingIDs.Undefined)
                {
                    foundInvalid = true;
                    if (foundValid) break;
                }
                else
                {
                    foundValid = true;
                }
            }
        }

        if (!foundValid && foundInvalid)
        {
            node.nodeType = NodeIDs.Full;
            return false;
        }
        else if (!foundInvalid && foundValid)
        {
            node.nodeType = NodeIDs.Free;
            return false;
        }

        else if (foundValid && foundInvalid)
        {
            node.nodeType = NodeIDs.Mixed;
            if (node.w >= MinSize && node.h >= MinSize)
            {
                return true;
            }
        }
        return false;
    }
}
