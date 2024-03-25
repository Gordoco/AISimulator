using System.Collections.Generic;
using UnityEngine;

public struct DirectedEdge
{
    public int startIndex;
    public int endIndex;

    public DirectedEdge(int inStart, int inEnd)
    {
        startIndex = inStart;
        endIndex = inEnd;
    }
}

/**
 * Class representing a generic, directed graph with weights corresponding to euclidian distance between vectors
 */
public class GenericDigraph
{
    private List<Vector2> V; //2D Locations of verticies on abstract plane
    private List<DirectedEdge> E; //Ordered Pairs of two vertex indecies


    public Vector2 GetVertex(int index)
    {
        return V[index];
    }

    public DirectedEdge GetEdge(int index)
    {
        return E[index];
    }
    public int GetNumVertices() { return V.Count; }
    public int GetNumEdges() { return E.Count; }
    public int GetVertexIndex(Vector2 index) { return V.IndexOf(index); }

    /**
     * #### GenericDigraph(List<Vector2> List<DirectedEdge>)
     * Constructor expecting pre-formatted verticies and edges
     */
    public GenericDigraph(List<Vector2> verts, List<DirectedEdge> edges)
    {
        V = verts;
        E = edges;
    }

    public GenericDigraph()
    {
        V = new List<Vector2>();
        E = new List<DirectedEdge>();
    }

    /**
    * #### List<int> GetNeighborIndecies(int)
    * Retrieve all neighbors from index input
    */
    public List<int> GetNeighborIndecies(int vertIndex)
    {
        List<int> verts = new List<int>();

        for (int i = 0; i < E.Count; i++)
        {
            if (vertIndex == E[i].startIndex)
            {
                if (E[i].endIndex < 0 || E[i].endIndex >= V.Count) Debug.Log("RETURNING INVALID NEIGHBORS");
                verts.Add(E[i].endIndex);
            }
        }

        return verts;
    }

    /**
    * #### List<int> GetNeighborIndecies(Vector2)
    * Retrieve all neighbors from location input
    */
    public List<int> GetNeighborIndecies(Vector2 vert)
    {
        int vertIndex = -1;
        for (int i = 0; i < V.Count; i++) { if (vert == V[i]) vertIndex = i; }
        return GetNeighborIndecies(vertIndex);
    }

    /**
    * #### List<Vector2> GetNeighborIndecies(int)
    * Retrieve all neighbors as vectors from index input
    */
    public List<Vector2> GetNeighbors(int vertIndex)
    {
        List<Vector2> verts = new List<Vector2>();
        List<int> neighborIndecies = GetNeighborIndecies(vertIndex);
        for (int i = 0; i < neighborIndecies.Count; i++)
        {
            verts.Add(V[neighborIndecies[i]]);
        }
        return verts;
    }

    /**
    * #### List<Vector2> GetNeighborIndecies(Vector2)
    * Retrieve all neighbors as vectors from vector input
    */
    public List<Vector2> GetNeighbors(Vector2 vert)
    {
        int vertIndex = -1;
        for (int i = 0; i < V.Count; i++) { if (vert == V[i]) vertIndex = i; }
        return GetNeighbors(vertIndex);
    }

    /**
     * #### void Print()
     * Debugging method for creating a visual representation of the graph in world space
     */
    public void Print(float height, float time = 0)
    {
        for (int i = 0; i < V.Count; i++)
        {
            List<Vector2> neighbors = GetNeighbors(i);
            Color drawColor;
            if (i == 0) drawColor = Color.cyan;
            else if (i == V.Count - 1) drawColor = Color.red;
            else drawColor = Color.green;
            Debug.DrawLine(new Vector3(V[i].x, height - 1, V[i].y), new Vector3(V[i].x, height + 1, V[i].y), drawColor, time); //Draw Vertex
            for (int j = 0; j < neighbors.Count; j++)
            {
                Color edgeColor;
                if (i == 0) edgeColor = Color.yellow;
                else edgeColor = Color.blue;
                Debug.DrawLine(new Vector3(V[i].x, height, V[i].y), new Vector3(neighbors[j].x, height, neighbors[j].y), edgeColor, time); //Draw Edges
            }
        }
    }
}

