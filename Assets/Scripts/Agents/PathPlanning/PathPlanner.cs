using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * #### PathPlanner
 * -----
 * Class to compile data from local grids for pathfinding using QuadTrees
 */

class PathNode
{
    public float g;
    public float f;
    public PathNode parent;
    public QuadTreeNode node;

    public PathNode(float inG, PathNode inParent, QuadTreeNode inNode) { g = inG; parent = inParent; node = inNode; }
}
public class PathPlanner
{
    public bool OnPath = false;
    public List<Vector2> currentPath;

    /**
     * #### bool CheckForValidPath(Vector2, DynamicCoordinateGrid)
     * Uses the A* algorithm to search for a valid path through the local grid's QuadTree
     * Returns based on ability to find a valid path
     */
    public List<Vector2> CheckForValidPath(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 currentLocation = initialLocation;

        QuadTree tree = new QuadTree();
        tree.Construct(mapping, mapping.Origin);
        PathNode currentNode = new PathNode(0, null, tree.GetNode(initialLocation));
        PathNode startNode = currentNode;
        PathNode completePath = null;

        //DEBUG
        currentNode.node.colorOverride = Color.cyan;
        QuadTreeNode endNode = tree.GetNode(location);
        endNode.colorOverride = Color.black;
        endNode.Print(1);
        currentNode.node.Print(1);
        //*****

        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        openList.Add(currentNode);

        bool bDone = false;
        while (openList.Count > 0 && !bDone)
        {
            //Consider Node with lowest F value
            float lowestF = Mathf.Infinity;
            for (int i = 0; i < openList.Count; i++)
            {
                float f = openList[i].g + Mathf.Abs(location.x - openList[i].node.x) + Mathf.Abs(location.y - openList[i].node.y);
                if (f < lowestF) lowestF = f;
                currentNode = openList[i];
            }
            openList.Remove(currentNode);

            List<QuadTreeNode> neighborNodes = currentNode.node.GetDirections();
            for (int i = 0; i < neighborNodes.Count; i++)
            {
                float dist = Mathf.Abs(neighborNodes[i].x - currentNode.node.x) + Mathf.Abs(neighborNodes[i].y - currentNode.node.y);
                PathNode node = new PathNode(currentNode.g + dist, currentNode, neighborNodes[i]);
                if (node.node == endNode)
                {
                    completePath = node;
                    bDone = true;
                    break;
                }
                else
                {
                    float newh = Mathf.Abs(location.x - node.node.x) + Mathf.Abs(location.y - node.node.y);
                    node.f = node.g + newh;
                }

                bool bSkip = false;
                for (int j = 0; j < openList.Count; j++)
                {
                    if (openList[j].node == node.node && openList[j].f < node.f)
                    {
                        bSkip = true;
                        break;
                    }
                }
                if (bSkip) break;
                for (int j = 0; j < closedList.Count; j++)
                {
                    if (closedList[j].node == node.node && closedList[j].f < node.f)
                    {
                        bSkip = true;
                        break;
                    }
                }
                if (bSkip) break;
                openList.Add(node);
            }
            closedList.Add(currentNode);
        }

        if (completePath == null) return new List<Vector2>(0);
        else
        {
            while(completePath.parent != null)
            {
                path.Insert(0, completePath.node.GetCenterPoint());
            }
            path.Insert(0, completePath.node.GetCenterPoint());
        }
        return path;
    }

    /**
    * #### bool Move(Vector2, DynamicCoordinateGrid, Agent)
    * Checks for a valid path and interfaces with agent locomotion to traverse to the destination
    * Returns based on ability to conduct a move
    */
    public bool Move(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping)
    {
        List<Vector2> path = CheckForValidPath(initialLocation, location, mapping);
        if (path.Count != 0)
        {
            currentPath = path;
            OnPath = true;
            return true;
        }
        return false;
    }
}
