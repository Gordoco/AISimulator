using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class to compile data from local grids for pathfinding using QuadTrees
 */
public class PathPlanner
{
    public bool OnPath = false;
    public bool bCollisionReset = false;
    public Vector3 collisionDir;

    public List<Vector2> currentPath;
    private Agent owner = null;

    public PathPlanner() { }
    public PathPlanner(Agent owner) { this.owner = owner; }

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
    public PathInfo CheckForValidPath(QuadTree tree, Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f, bool bPrint = false)
    {
        List<Vector2> path = new List<Vector2>();
        List<QuadTreeNode> nodes = new List<QuadTreeNode>();

        /*QuadTree tree = new QuadTree();
        tree.Construct(mapping, mapping.Origin, time, bPrint);*/

        GenericDigraph graph = GenerateGraphFromQuadTree(initialLocation, location, tree);
        List<int> pathIndecies = GenericAStar(graph, time, 1, bPrint);
        if (pathIndecies == null)
        {
            return new PathInfo(path, nodes);
        }
        for (int i = 0; i < pathIndecies.Count; i++)
        {
            path.Add(graph.GetVertex(pathIndecies[i]));
            if (i > 0 && i < pathIndecies.Count - 1) nodes.Add(tree.GetNode(graph.GetVertex(pathIndecies[i])));
        }

        return new PathInfo(path, nodes);
    }

    /**
     * #### List<int> GenericAStar(GenericDigraph)
     * Takes in a generic graph with the assumption that the first element of the vertex array is the start
     * and the last element is the end of the desired path.
     * Returns a list of vertex indecies describing the calculated path.
     */
    private List<int> GenericAStar(GenericDigraph graph, float time = 0f, float height = 1, bool shouldPrint = false)
    {
        if (graph.GetNumVertices() <= 0) return null;
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
        if (finalNode == null) return null;
        while (finalNode != null)
        {
            path.Insert(0, graph.GetVertexIndex(finalNode.value));
            //if (finalNode.parent != null && shouldPrint) Debug.DrawLine(new Vector3(finalNode.value.x, height, finalNode.value.y), new Vector3(finalNode.parent.value.x, height, finalNode.parent.value.y), Color.magenta, time);
            finalNode = finalNode.parent;
        }
        return path;
    }

    /**
     * #### List<Vector2> OptimizePath(List<Vector2>)
     * Assumes path input is valid with non-zero length, applies rubber-banding to optimize path length
     */
    private List<Vector2> OptimizePath(PathInfo pathInfo, float time = 0.2f, bool bPrint = false)
    {
        List<Vector2> arr = new List<Vector2>();

        GenericDigraph graph = GenerateGraphFromQuadTreePath(pathInfo);
        if (bPrint)
        {
            //graph.Print(1, 5);
            //Debug.Log(graph.GetNumVertices());
        }

            //Run A* on directed graph created above
            List<int> vertexIndecies = GenericAStar(graph, time, 1, bPrint);
        for (int i = 0; i < vertexIndecies.Count; i++) arr.Add(graph.GetVertex(vertexIndecies[i]));

        //Do Visibility Checks to Simplify Path Geometry
        arr = VisibilitySimplification(pathInfo, arr, bPrint, time, 1);

        return arr;
    }

    /**
     * List<Vector2> VisibilitySimplification(PathInfo, List<Vector2>)
     * Method which performs "visibility" checks on path segments and removes unessesary points which don't affect path validity
     * Specifically, removes points which, when skipped, allow for the path to remain within the same quads as before.
     */
    private List<Vector2> VisibilitySimplification(PathInfo originalPath, List<Vector2> pathPoints, bool bShouldPrint = false, float time = 0.2f, float height = 1)
    {
        List<Vector2> simplifiedPath = new List<Vector2>();
        if (pathPoints.Count == 2) { return pathPoints; } //No simplification possible

        //Debugging error message
        if (pathPoints.Count > originalPath.nodes.Count + 1) Debug.Log("ERROR: Improper Path Length || Num Nodes: " + originalPath.nodes.Count + " Num Points: " + pathPoints.Count);
        if (originalPath.nodes.Count == 0) Debug.Log("NO NODES");
        int lastVisiblePoint = 0;
        int lastAddedPoint = 0;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            int i1 = -1;
            int i2 = -1;
            for (int k = 0; k < originalPath.nodes.Count; k++)
            {
                if (originalPath.nodes[k].CheckIfWithin(pathPoints[lastVisiblePoint])) { i1 = k; }
                if (originalPath.nodes[k].CheckIfWithin(pathPoints[i])) { i2 = k; }
            }

            if (i2 - i1 == 0)
            {
                simplifiedPath.Add(pathPoints[0]);
                lastAddedPoint = 0;
            }

            for (int k = i1; k < (i2-i1) - 2; k++)
            {
                //Debug.Log(originalPath.nodes.Count);
                if (k < 0 || k >= originalPath.nodes.Count || k + 1 < 0 || k + 1 >= originalPath.nodes.Count) continue;
                Vector2[] edge = GetEdgePoints(originalPath.nodes[k], originalPath.nodes[k+1]);
                Vector2 intersection;
                if (!Intersects(pathPoints[lastVisiblePoint], pathPoints[i], edge[0], edge[1], out intersection))
                {
                    simplifiedPath.Add(pathPoints[i - 1]);
                    lastVisiblePoint = lastAddedPoint;
                    lastAddedPoint = i - 1;
                    break;
                }
            }

            if (i == pathPoints.Count - 1) simplifiedPath.Add(pathPoints[i]);
        }

        if (bShouldPrint)
        {
            for (int i = 1; i < simplifiedPath.Count; i++)
            {
                Debug.DrawLine(new Vector3(simplifiedPath[i-1].x, height, simplifiedPath[i-1].y), new Vector3(simplifiedPath[i].x, height, simplifiedPath[i].y), Color.magenta, time);
            }
        }
        return simplifiedPath;
    }

    /**
     * bool Intersects(Vector2, Vector2, Vector2, Vector2, out Vector2)
     * Line intersection helper method, takes in two lines and outputs the boolean intersaction and intersection location
     */
    private bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        Vector2 b = a2 - a1;
        Vector2 d = b2 - b1;
        float bDotDPerp = b.x * d.y - b.y * d.x;

        // if b dot d == 0, it means the lines are parallel so have infinite intersection points
        if (bDotDPerp == 0)
            return false;

        Vector2 c = b1 - a1;
        float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
        if (t < 0 || t > 1)
            return false;

        float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
        if (u < 0 || u > 1)
            return false;

        intersection = a1 + t * b;

        return true;
    }

    /**
     * GenericDigraph GenerateGraphFromQuadTree(Vector2, Vector2, QuadTree)
     * Generates a graphical representation of a QuadTree along with a start and end location for pathfinding
     * Creates the graph as two uni-directional edges connecting the centers of each neighboring quad in the tree.
     */
    private GenericDigraph GenerateGraphFromQuadTree(Vector2 startLoc, Vector2 endLoc, QuadTree tree)
    {
        List<Vector2> verts = new List<Vector2>();
        List<DirectedEdge> edges = new List<DirectedEdge>();
        List<QuadTreeNode> temp = tree.GetLeaves();
        List<QuadTreeNode> children = new List<QuadTreeNode>();
        for (int i = 0; i < temp.Count; i++) if (temp[i].nodeType == NodeIDs.Free) children.Add(temp[i]);
        verts.Add(startLoc);
        if (tree.GetNode(startLoc) == null) return new GenericDigraph();
        List<QuadTreeNode> startAdjs = tree.GetNode(startLoc).GetDirections();
        for (int i = 0; i < startAdjs.Count; i++)
        {
            if (startAdjs[i].nodeType == NodeIDs.Free)
            {
                edges.Add(new DirectedEdge(0, children.IndexOf(startAdjs[i]) + 1));
            }
        }
        for (int i = 0; i < children.Count; i++)
        {
            verts.Add(children[i].GetCenterPoint());
            List<QuadTreeNode> adjs = children[i].GetDirections();
            foreach (QuadTreeNode adj in adjs)
            {
                if (children.Contains(adj))
                {
                    edges.Add(new DirectedEdge(i + 1, children.IndexOf(adj) + 1));
                }
            }
        }
        verts.Add(endLoc);

        QuadTreeNode endNode = tree.GetNode(endLoc);
        if (endNode != null && endNode.nodeType == NodeIDs.Free) edges.Add(new DirectedEdge(children.IndexOf(endNode) + 1, verts.Count - 1)); //Guarunteed to have 2 verts so this is safe

        GenericDigraph graph = new GenericDigraph(verts, edges);
        //if (owner != null && owner.ShouldPrint) graph.Print(2, 2);
        return graph;
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

        GenericDigraph graph = new GenericDigraph(verts, edges);
        //graph.Print(2, 5);
        return graph;
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
    public bool Move(QuadTree tree, Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f, bool bPrint = false)
    {
        PathInfo pathInfo = CheckForValidPath(tree, initialLocation, location, mapping, time, bPrint);
        if (pathInfo.path.Count != 0)
        {
            //Debug.Log("[DCG_Move] Success");
            currentPath = OptimizePath(pathInfo, time, bPrint);
            OnPath = true;
            return true;
        }
        return false;
    }

    public void CancelPath(bool bCollision = false, Vector3 collisionDir = new Vector3())
    {
        OnPath = false;
        currentPath = null;
        bCollisionReset = bCollision;
        this.collisionDir = collisionDir;
    }
}
