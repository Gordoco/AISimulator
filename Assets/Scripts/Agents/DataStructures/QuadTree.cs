using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
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
    public void Construct(DynamicCoordinateGrid mapping, Vector3 offset, float time = 0.1f)
    {
        root = new QuadTreeNode(offset.x + mapping.gridCorner[0], offset.z + mapping.gridCorner[1], 
            mapping.width, mapping.height);
        root.tree = this;
        Origin = new Vector2(offset.x + mapping.gridCorner[0], offset.z + mapping.gridCorner[1]);
        Partition(root, mapping);
        Print(time);
    }

    /**
     * QuadTreeNode GetNode(Vector2)
     * Recursively finds the correct leaf node at the specified (X, Z) world location
     */
    public QuadTreeNode GetNode(Vector2 location)
    {
        return root.GetNode(location);
    }

    /**
     * #### QuadTreeNode GetFurthestFreeNodes(Vector2)
     * Takes in an (X, Z) world location and returns a list of all nodes which are free in sorted order on distance
     */
    public List<NodeDepth> GetFurthestFreeNodes(Vector2 location)
    {
        /*float furthest = 0;
        QuadTreeNode furNode = null;*/
        List<NodeDepth> children = root.GetFreeChildren();
        /*for (int i = 0; i < children.Count; i++)
        {
            if (children[i].node.nodeType != NodeIDs.Free) continue;
            if ((children[i].node.GetCenterPoint() - location).magnitude > furthest)
            {
                furthest = (children[i].node.GetCenterPoint() - location).magnitude;
                furNode = children[i].node;
            }
        }*/
        children.Sort((a, b) => a.CompareTo(b, location));
        //if (furNode == GetNode(location)) return null;
        return children;
    }

    /**
     * #### bool IsChild(QuadTreeNode, QuadTreeNode)
     * Checks if the first parameter is a child of the second in the QuadTree
     */
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
                //Enum representing location status, transformed back to world origin from agent origin
                MappingIDs map = mapping.GetMapping((int)(i - Origin.x), (int)(j - Origin.y));

                //Small hardcoded ray locations
                Vector3 init = new Vector3(i, 5, j);
                Vector3 end = new Vector3(i, -5, j);

                //Differentiate between free and full raycasts
                Color lineCol;
                if (map == MappingIDs.Free) lineCol = Color.green;
                else lineCol = Color.red;

                //DEBUG: Visualize the mapping used by the current tree
                if (map == MappingIDs.Full) Debug.DrawLine(init, end, lineCol, 2);
                //-----------------------------------------------------

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
            if (node.w > MinSize && node.h > MinSize)
            {
                return true;
            }
        }
        return false;
    }
}
