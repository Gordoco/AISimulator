using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A data structure class which interfaces with the DynamicCoordinateGrid to create a QuadTree for pathfinding
 */
public class QuadTree
{
    public float MinSize = 1f;

    private QuadTreeNode root;
    private Vector2 Origin;

    /**
     * #### void Construct(DynamicCoordinateGrid, Vector3)
     * Creates the physical QuadTree by initializing one base quad on the entire grid and recursivly partitioning that quad
     */
    public void Construct(DynamicCoordinateGrid mapping, Vector3 offset, float time = 0.1f, bool bPrint = false)
    {
        /*Vector3 line1S = new Vector3(offset.x + mapping.gridCorner[0], 5, offset.z + mapping.gridCorner[1]);
        Vector3 line1E = new Vector3(offset.x + mapping.gridCorner[0], -5, offset.z + mapping.gridCorner[1]);
        Debug.DrawLine(line1S, line1E, Color.cyan, 20);*/


        root = new QuadTreeNode(offset.x + mapping.gridCorner[0], offset.z + mapping.gridCorner[1], 
            mapping.width - 1, mapping.height - 1);
        root.tree = this;
        Origin = new Vector2(offset.x + mapping.gridCorner[0], offset.z + mapping.gridCorner[1]);
        Partition(root, mapping, bPrint);
        if (bPrint) Print(time);
    }

    /**
     * QuadTreeNode GetNode(Vector2)
     * Recursively finds the correct leaf node at the specified (X, Z) world location
     */
    public QuadTreeNode GetNode(Vector2 location)
    {
        return root.GetNode(location);
    }

    public QuadTreeNode GetNearestFreeNode(Vector2 location)
    {
        QuadTreeNode currNode = GetNode(location);
        if (currNode.nodeType == NodeIDs.Free) return currNode;
        List<QuadTreeNode> neighbors = currNode.GetDirections();
        float val = Mathf.Infinity;
        QuadTreeNode currNearest = currNode;
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i].nodeType != NodeIDs.Free) continue;
            float dist = Vector2.Distance(neighbors[i].GetCenterPoint(), location);
            if (dist < val)
            {
                val = dist;
                currNearest = neighbors[i];
            }
        }
        return currNearest;
    }
    public List<NodeDepth> GetFurthestFreeNodeDepths(Vector2 location)
    {
        List<NodeDepth> children = root.GetFreeChildren();
        children.Sort((a, b) => a.CompareTo(b, location));
        return children;
    }


    /**
    * #### QuadTreeNode GetFurthestFreeNodes(Vector2)
    * Takes in an (X, Z) world location and returns a list of all nodes which are free in sorted order on distance
    */
    public List<QuadTreeNode> GetFurthestFreeNodes(Vector2 location)
    {
        /*List<NodeDepth> children = root.GetFreeChildren();
        children.Sort((a, b) => a.CompareTo(b, location));
        List<QuadTreeNode> nodes = new List<QuadTreeNode>();
        foreach (NodeDepth depth in children) nodes.Add(depth.node);
        return nodes;*/

        QuadTreeNode myNode = GetNode(location);
        if (myNode == null || myNode.nodeType != NodeIDs.Free) return null;
        List<QuadTreeNode> finalArr = new List<QuadTreeNode>();
        List<QuadTreeNode> neighbors = myNode.GetDirections();
        while (neighbors.Count > 0)
        {
            float furthest = 0;
            QuadTreeNode furthestNode = null;
            foreach (QuadTreeNode node in neighbors)
            {
                if (!finalArr.Contains(node) && node.nodeType == NodeIDs.Free && Vector2.Distance(location, node.GetCenterPoint()) > furthest)
                {
                    furthest = Vector2.Distance(location, node.GetCenterPoint());
                    furthestNode = node;
                }
            }
            if (furthestNode == null) break;
            finalArr.Add(furthestNode);
            neighbors = furthestNode.GetDirections();
        }
        finalArr.Remove(myNode);
        return finalArr;
    }

    public List<QuadTreeNode> GetFurthestFreeNodesInDir(Vector2 location, Vector2 dir) 
    {
        List<NodeDepth> arr = GetFurthestFreeNodeDepths(location);
        arr.Sort((a, b) => a.CompareTo(b, location + dir));
        arr.Reverse();
        List<QuadTreeNode> nodes = new List<QuadTreeNode>();
        foreach (NodeDepth depth in arr) nodes.Add(depth.node);
        return nodes;
        /*if (arr == null || arr.Count <= 0) return null;
        int x = -1;
        for (int i = arr.Count - 1; i >= 0; i--)
        {
            if (Vector2.Dot((arr[i].GetCenterPoint() - location).normalized, -dir) > 0.5f)
            {
                x = i;
                break;
            }
        }

        if (x > -1 && x < arr.Count)
        {
            QuadTreeNode temp = arr[0];
            arr[0] = arr[x];
            arr[x] = temp;
        }

        return arr;*/
    }

    /**
     * List<QuadTreeNode> GetLeaves()
     * Method to return all the leaves of the quad tree for path finding graph construction
     */
    public List<QuadTreeNode> GetLeaves()
    {
        List<NodeDepth> nodeDepths = root.GetChildren();
        List<QuadTreeNode> leaves = new List<QuadTreeNode>();
        for (int i = 0; i < nodeDepths.Count; i++) leaves.Add(nodeDepths[i].node);
        return leaves;
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
    void Partition(QuadTreeNode node, DynamicCoordinateGrid mapping, bool bPrint = false)
    {
        if (MustBeSubdivided(node, mapping, bPrint))
        {
            node.SW = new QuadTreeNode(node.x, node.y, node.w / 2, node.h / 2);
            node.SW.nodeLoc = 3;
            node.SW.depth = node.depth + 1;
            node.SW.parent = node;
            node.SW.tree = this;

            node.SE = new QuadTreeNode(node.x + node.w / 2, node.y, node.w / 2, node.h / 2);
            node.SE.nodeLoc = 2;
            node.SE.depth = node.depth + 1;
            node.SE.parent = node;
            node.SE.tree = this;

            node.NW = new QuadTreeNode(node.x, node.y + node.h / 2, node.w / 2, node.h / 2);
            node.NW.nodeLoc = 1;
            node.NW.depth = node.depth + 1;
            node.NW.parent = node;
            node.NW.tree = this;

            node.NE = new QuadTreeNode(node.x + node.w / 2, node.y + node.h / 2, node.w / 2, node.h / 2);
            node.NE.nodeLoc = 0;
            node.NE.depth = node.depth + 1;
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
    bool MustBeSubdivided(QuadTreeNode node, DynamicCoordinateGrid mapping, bool bPrint = false)
    {
        bool foundValid = false;
        bool foundInvalid = false;

        for (int i = (int)node.x; i <= node.x + node.w; i++)
        {
            for (int j = (int)node.y; j <= node.y + node.h; j++)
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
                //if (bPrint) Debug.DrawLine(init, end, lineCol, 0.2f);
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
            if (node.w >= MinSize*2 && node.h >= MinSize*2)
            {
                return true;
            }
        }
        return false;
    }
}
