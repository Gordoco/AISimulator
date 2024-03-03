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
     * #### PathInfo
     * Struct to hold path information, path variable should be exactly 2 larger than nodes array to account for start and end locations
     */
    public struct PathInfo
    {
        public List<Vector2> path;
        public List<QuadTreeNode> nodes;

        public PathInfo(List<Vector2> inPath, List<QuadTreeNode> inNodes)
        {
            path = inPath;
            nodes = inNodes;
        }
    }

    /**
     * #### bool CheckForValidPath(Vector2, DynamicCoordinateGrid)
     * Uses the A star algorithm to search for a valid path through the local grid's QuadTree
     * Returns based on ability to find a valid path
     */
    public PathInfo CheckForValidPath(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f)
    {
        List<Vector2> path = new List<Vector2>();
        List<QuadTreeNode> nodes = new List<QuadTreeNode>();
        Vector2 currentLocation = initialLocation;

        QuadTree tree = new QuadTree();
        tree.Construct(mapping, mapping.Origin, time);

        PathNode currentNode = new PathNode(0, null, tree.GetNode(initialLocation));
        PathNode startNode = currentNode;
        PathNode completePath = null;
        QuadTreeNode endNode = tree.GetNode(location);
        //Debug.Log(endNode);
        if (endNode == null) return new PathInfo(path, null);

        if (currentNode.node == endNode)
        {
            path.Add(initialLocation);
            path.Add(location);
            path.Add(startNode.node.GetCenterPoint());
            path.Add(endNode.GetCenterPoint());
            nodes.Add(startNode.node);
            return new PathInfo(path, nodes);
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
                if (openList[i].f < lowestF) lowestF = openList[i].f;
                currentNode = openList[i];
            }
            openList.Remove(currentNode);

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

                neighbor.g = currentNode.g + Mathf.Pow(currentNode.node.GetCenterPoint().x - neighbor.node.GetCenterPoint().x, 2) + Mathf.Pow(currentNode.node.GetCenterPoint().y - neighbor.node.GetCenterPoint().y, 2);
                float neighborh = Mathf.Pow(location.x - neighbor.node.GetCenterPoint().x, 2) + Mathf.Pow(location.y - neighbor.node.GetCenterPoint().y, 2);

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

        if (completePath == null) return new PathInfo(new List<Vector2>(0), null);
        else
        {
            path.Insert(0, location);
            Vector3 Location = mapping.toVector3(2, location);
            //completePath = completePath.parent; //Skips Center of node containing location

            while (completePath != null && completePath.parent != null)
            {
                path.Insert(0, completePath.node.GetCenterPoint());
                nodes.Insert(0, completePath.node);

                /*DEBUG*/
                //Debug.DrawLine(Location, mapping.toVector3(0.5f, completePath.node.GetCenterPoint()), Color.blue, 2f);
                /* - - - - - - */

                Location = mapping.toVector3(0.5f, completePath.node.GetCenterPoint());
                completePath = completePath.parent;
            }
            path.Insert(0, initialLocation);
            
        }
        return new PathInfo(path, nodes);
    }

    /**
     * #### List<int> GenericAStar(GenericDigraph)
     * Takes in a generic graph with the assumption that the first element of the vertex array is the start
     * and the last element is the end of the desired path.
     * Returns a list of vertex indecies describing the calculated path.
     */
    private List<int> GenericAStar(GenericDigraph graph, float time = 0f, float height = 1)
    {
        List<int> path = new List<int>();
        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closedList = new List<AStarNode>();

        AStarNode finalNode = null;

        Vector2 destination = graph.GetVertex(graph.GetNumVertices() - 1);

        openList.Add(new AStarNode(0, null, graph.GetVertex(0)));
        while (openList.Count != 0)
        {
            AStarNode q = null;
            float currLowest = Mathf.Infinity;
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].f < currLowest)
                {
                    currLowest = openList[i].f;
                    q = openList[i];
                }
            }
            openList.Remove(q);
            List<Vector2> neighbors = graph.GetNeighbors(q.value);
            for (int i = 0; i < neighbors.Count; i++)
            {
                AStarNode newNode = new AStarNode(q.g + Vector2.Distance(q.value, neighbors[i]), q, neighbors[i]);
                float newh = Vector2.Distance(neighbors[i], destination);
                newNode.f = newNode.g + newh;
                if (newNode.value == destination)
                {
                    //DONE
                    finalNode = newNode;
                    openList.Clear(); //Trigger end of while loop
                    break;
                }

                bool bContinue = false;
                for (int j = 0; j < openList.Count; j++) {
                    if (openList[j].value == newNode.value && openList[j].f < newNode.f) bContinue = true;
                }
                for (int j = 0; j < closedList.Count; j++)
                {
                    if (closedList[j].value == newNode.value && closedList[j].f < newNode.f) bContinue = true;
                }
                if (!bContinue) openList.Add(newNode);
            }
            closedList.Add(q);
        }

        while (finalNode != null)
        {
            path.Insert(0, graph.GetVertexIndex(finalNode.value));
            if (finalNode.parent != null) Debug.DrawLine(new Vector3(finalNode.value.x, height, finalNode.value.y), new Vector3(finalNode.parent.value.x, height, finalNode.parent.value.y), Color.magenta, time);
            finalNode = finalNode.parent;
        }
        return path;
    }

    /**
     * #### List<Vector2> OptimizePath(List<Vector2>)
     * Assumes path input is valid with non-zero length, applies rubber-banding to optimize path length
     */
    private List<Vector2> OptimizePath(PathInfo pathInfo, float time = 0.2f)
    {
        List<Vector2> arr = new List<Vector2>();

        GenericDigraph graph = GenerateGraphFromQuadTreePath(pathInfo);
        //graph.Print(1, time);

        //Run A* on directed graph created above
        List<int> vertexIndecies = GenericAStar(graph, time);
        for (int i = 0; i < vertexIndecies.Count; i++) arr.Add(graph.GetVertex(vertexIndecies[i]));

        //Do Visibility Checks to Simplify Path Geometry


        return arr;
    }

    /**
     * #### GenericDigraph GenerateGraphFromQuadTreePath(PathInfo)
     * Method for constructing a weighted digraph from the edges of the quads determined by A*
     */
    private GenericDigraph GenerateGraphFromQuadTreePath(PathInfo pathInfo)
    {
        List<Vector2> verts = new List<Vector2>();
        List<DirectedEdge> edges = new List<DirectedEdge>();
        verts.Add(pathInfo.path[0]); //Start Location
        for (int i = 0; i < pathInfo.nodes.Count - 1; i++)
        {
            //Construct Generic A* graph from edge verticies
            Vector2[] quadEdgePoints = GetEdgePoints(pathInfo.nodes[i], pathInfo.nodes[i + 1]);
            verts.AddRange(quadEdgePoints);

            int top = (i * 2) + 1;
            int bottom = (i * 2) + 2;
            if (i > 0)
            {
                //Connect to previous points (with direction moving "forward")
                int j = (i - 1);
                edges.Add(new DirectedEdge((j * 2) + 1, top));
                edges.Add(new DirectedEdge((j * 2) + 2, bottom));
                edges.Add(new DirectedEdge((j * 2) + 2, top));
                edges.Add(new DirectedEdge((j * 2) + 1, bottom));
            }
            else
            {
                edges.Add(new DirectedEdge(0, top));
                edges.Add(new DirectedEdge(0, bottom));
            }
            //Connect points (both directions)
            edges.Add(new DirectedEdge(top, bottom));
            edges.Add(new DirectedEdge(bottom, top));
        }
        verts.Add(pathInfo.path[pathInfo.path.Count - 1]);
        if (verts.Count > 2)
        {
            edges.Add(new DirectedEdge(verts.Count - 2, verts.Count - 1));
            edges.Add(new DirectedEdge(verts.Count - 3, verts.Count - 1));
        }
        else edges.Add(new DirectedEdge(0, 1));
        return new GenericDigraph(verts, edges);
    }

    /**
     * #### Vector2[] GetEdgePoints(QuadTreeNode, QuadTreeNode)
     * Determines the points of the smallest quad which lie on the boundry of the largest quad
     */
    private Vector2[] GetEdgePoints(QuadTreeNode curr, QuadTreeNode next)
    {
        Vector2[] arr = new Vector2[2];

        if (curr == next) return arr;

        if (curr.depth > next.depth)
        {
            QuadTreeNode temp = curr;
            curr = next;
            next = temp;
        }

        float currXMin = curr.x;
        float currXMax = curr.x + curr.w;
        float currYMin = curr.y;
        float currYMax = curr.y + curr.h;

        int count = 0;
        Vector2[] points = { 
            new Vector2(next.x, next.y),
            new Vector2(next.x + next.w, next.y),
            new Vector2(next.x + next.w, next.y + next.h),
            new Vector2(next.x, next.y + next.h) 
        };

        for (int i = 0; i < 4; i++)
        {
            if ((points[i].x >= currXMin && points[i].x <= currXMax) && (points[i].y >= currYMin && points[i].y <= currYMax))
            {
                arr[count] = points[i];
                count++;
            }
        }
        return arr;
    }

    /**
    * #### bool Move(Vector2, DynamicCoordinateGrid, Agent)
    * Checks for a valid path and interfaces with agent locomotion to traverse to the destination
    * Returns based on ability to conduct a move
    */
    public bool Move(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f)
    {
        //Debug.Log("[DCG_Move] Trying To Move");
        PathInfo pathInfo = CheckForValidPath(initialLocation, location, mapping, time);
        if (pathInfo.path.Count != 0)
        {
            //Debug.Log("[DCG_Move] Success");
            currentPath = OptimizePath(pathInfo, time);
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
