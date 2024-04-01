using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GroundingTree
{
    public GenericDigraph tree;
    public List<int> vertexNames;
    public Vector2 goalLoc;

    public GroundingTree(GenericDigraph tree, List<int> vertexNames, Vector2 goalLoc)
    {
        this.tree = tree;
        this.vertexNames = vertexNames;
        this.goalLoc = goalLoc;
    }
}

public class Waypoint : BroadcastGoal
{
    public override void Broadcast(List<GroundingInfo> groundings)
    {
        base.Broadcast(groundings);
        groundings.Sort((a, b) => a.CompareTo(b, new Vector2(transform.position.x, transform.position.z)));
        DynamicCoordinateGrid mapping = gameObject.GetComponent<Agent>().GetMapping();
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");

        //Create a tree from all known groundings pointing towards the closest to the root
        List<Vector2> V = new List<Vector2>();
        List<DirectedEdge> E = new List<DirectedEdge>();
        List<int> IDs = new List<int>();
        int Old = 0;
        int New = 1;
        if (groundings.Count > 0)
        {
            IDs.Add(groundings[0].ID);
            V.Add(mapping.toVector2(groundings[0].obj.transform.position));
        }
        if (groundings.Count > 1)
        {
            IDs.Add(groundings[1].ID);
            V.Add(mapping.toVector2(groundings[1].obj.transform.position));
            E.Add(new DirectedEdge(1, 0));
        }
        for (int j = 2; j < groundings.Count; j++)
        {
            V.Add(mapping.toVector2(groundings[j].obj.transform.position));
            IDs.Add(groundings[j].ID);
            int Next = j;
            float Dist1 = Vector2.Distance(V[Old], V[Next]);
            float Dist2 = Vector2.Distance(V[New], V[Next]);
            if (Dist1 < Dist2)
            {
                E.Add(new DirectedEdge(Next, Old));
                New = Next;
            }
            else
            {
                E.Add(new DirectedEdge(Next, New));
                Old = New;
                New = Next;
            }
        }

        GenericDigraph graph = new GenericDigraph(V, E);

        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] == gameObject) continue;
            /*StartCoroutine(*/agents[i].GetComponent<Agent>().RecieveGoalBroadcast(new GroundingTree(graph, IDs, mapping.toVector2(transform.position)))/*)*/;
            //graph.Print(1, 10);

        }
    }
}
