using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class to compile data from local grids for pathfinding using QuadTrees
 */
public class PathPlanner
{
    public bool OnPath = false;
    public List<Vector2> currentPath;

    /**
     * #### bool CheckForValidPath(Vector2, DynamicCoordinateGrid)
     * Uses the A star algorithm to search for a valid path through the local grid's QuadTree
     * Returns based on ability to find a valid path
     */
    public List<Vector2> CheckForValidPath(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 currentLocation = initialLocation;

        QuadTree tree = new QuadTree();
        tree.Construct(mapping, mapping.Origin, time);

        PathNode currentNode = new PathNode(0, null, tree.GetNode(initialLocation));
        PathNode startNode = currentNode;
        PathNode completePath = null;
        QuadTreeNode endNode = tree.GetNode(location);
        //Debug.Log(endNode);
        if (endNode == null) return path;

        //DEBUG
        //if (currentNode.node != null)
        //{
            //Debug.Log("[DCG_Move] SHOULD PRINT START");
            /*currentNode.node.colorOverride = Color.cyan;
            endNode.colorOverride = Color.black;
            endNode.Print(2);
            currentNode.node.Print(2);*/
        //}
        
        if (currentNode.node == endNode)
        {
            path.Add(location);
            return path;
        }

        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        openList.Add(currentNode);

        int MAX_ATTEMPTS = 10000;
        while (openList.Count > 0 && MAX_ATTEMPTS > 0)
        {
            MAX_ATTEMPTS--;
            //Consider Node with lowest F value
            float lowestF = Mathf.Infinity;
            for (int i = 0; i < openList.Count; i++)
            {
                //float f = openList[i].g + Mathf.Abs(location.x - openList[i].node.x) + Mathf.Abs(location.y - openList[i].node.y);
                if (openList[i].f < lowestF) lowestF = openList[i].f;
                currentNode = openList[i];
            }
            openList.Remove(currentNode);

            //If this node is destination we are done
            /*if (currentNode.node == endNode)
            {
                closedList.Add(currentNode);
            }*/

            //closedList.Add(currentNode);

            List<QuadTreeNode> neighborNodes = currentNode.node.GetDirections();
            for (int i = 0; i < neighborNodes.Count; i++)
            {
                if (neighborNodes[i].nodeType != NodeIDs.Free) continue;
                PathNode neighbor = new PathNode(0, currentNode, neighborNodes[i]);

                if (neighbor.node == endNode)
                {
                    completePath = neighbor;
                    MAX_ATTEMPTS = 0;
                    break;
                }

                neighbor.g = currentNode.g + Mathf.Sqrt(Mathf.Pow(currentNode.node.GetCenterPoint().x - neighbor.node.GetCenterPoint().x, 2) + Mathf.Pow(currentNode.node.GetCenterPoint().y - neighbor.node.GetCenterPoint().y, 2));
                float neighborh = Mathf.Sqrt(Mathf.Pow(location.x - neighbor.node.GetCenterPoint().x, 2) + Mathf.Pow(location.y - neighbor.node.GetCenterPoint().y, 2));

                neighbor.f = neighbor.g + neighborh;

                bool shouldContinue = false;
                for (int j = 0; j < openList.Count; j++)
                {
                    if (openList[j].node == neighbor.node && openList[j].f < neighbor.f)
                    {
                        shouldContinue = true;
                        break;
                    }
                }
                if (shouldContinue) continue;

                for (int j = 0; j < closedList.Count; j++)
                {
                    if (closedList[j].node == neighbor.node && closedList[j].f < neighbor.f)
                    {
                        shouldContinue = true;
                        break;
                    }
                }
                if (shouldContinue) continue;

                openList.Add(neighbor);
            }
            closedList.Add(currentNode);
        }

        if (completePath == null) return new List<Vector2>(0);
        else
        {
            path.Insert(0, location);
            Vector3 Location = mapping.toVector3(2, location);
            completePath = completePath.parent; //Skips Center of node containing location

            while (completePath != null && completePath.parent != null)
            {
                path.Insert(0, completePath.node.GetCenterPoint());

                /*DEBUG*/
                Debug.DrawLine(Location, mapping.toVector3(0.5f, completePath.node.GetCenterPoint()), Color.blue, 2f);

                /*completePath.node.colorOverride = Color.magenta;
                completePath.node.Print(2);*/
                /* - - - - - - */

                Location = mapping.toVector3(0.5f, completePath.node.GetCenterPoint());
                completePath = completePath.parent;
            }
            
        }
        return path;
    }

    // HELPER METHODS TO BE IMPLEMENTATED FOR ABOVE
    private List<PathNode> AStarAlgorithm()
    {
        return null;
    }

    private bool SanitizePathInput()
    {
        return false;
    }
    //----------------------------------

    /*private Vector3 toVector3(float y, Vector2 input)
    {
        return new Vector3(input.x, y, input.y);
    }*/

    /**
    * #### bool Move(Vector2, DynamicCoordinateGrid, Agent)
    * Checks for a valid path and interfaces with agent locomotion to traverse to the destination
    * Returns based on ability to conduct a move
    */
    public bool Move(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f)
    {
        //Debug.Log("[DCG_Move] Trying To Move");
        List<Vector2> path = CheckForValidPath(initialLocation, location, mapping, time);
        if (path.Count != 0)
        {
            //Debug.Log("[DCG_Move] Success");
            currentPath = path;
            OnPath = true;
            return true;
        }
        return false;
    }

    public void CancelPath()
    {
        OnPath = false;
        currentPath = null;
    }
}
