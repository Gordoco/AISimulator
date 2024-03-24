using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroadcastGoal : MonoBehaviour
{
    public virtual void Broadcast()
    {
        Debug.Log("BROADCASTING ARRIVAL AT GOAL");
    }
}
