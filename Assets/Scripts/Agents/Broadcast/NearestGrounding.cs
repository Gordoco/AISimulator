using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearestGrounding : BroadcastGoal
{
    public override void Broadcast(List<GroundingInfo> groundings)
    {
        base.Broadcast(groundings);
        float closeNum = Mathf.Infinity;
        GroundingInfo currClosest = new GroundingInfo(null, 0);
        for (int i = 0; i < groundings.Count; i++)
        {
            float dist = Vector2.Distance(new Vector2(groundings[i].obj.transform.position.x, groundings[i].obj.transform.position.z), new Vector2(transform.position.x, transform.position.z));
            if (dist < closeNum)
            {
                currClosest = groundings[i];
                closeNum = dist;
            }
        }
        if (currClosest.obj == null) return;
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Agent");
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] == gameObject) continue;
            agents[i].GetComponent<Agent>().RecieveGoalBroadcast(currClosest);
        }
    }
}
