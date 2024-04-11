using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Superclass for grounded broadcasting by agents
 */
public class BroadcastGoal : MonoBehaviour
{
    public virtual void Broadcast(List<GroundingInfo> groundings)
    {
        Debug.Log("BROADCASTING ARRIVAL AT GOAL");
    }
}
