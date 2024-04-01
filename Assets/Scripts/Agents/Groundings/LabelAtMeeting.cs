using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelAtMeeting : GroundingMethod
{
    public override bool ExecuteGrounding(Agent owner)
    {
        GameObject[] others = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject other in others)
        {
            if (other == owner.gameObject) continue;
            if (Vector2.Distance(owner.GetMapping().toVector2(other.transform.position), owner.GetMapping().toVector2(owner.transform.position)) <= owner.collisionDist && other.GetComponent<Agent>().CanGround())
            {
                for (int i = 0; i < owner.groundings.Count; i++)
                {
                    if (Vector2.Distance(owner.GetMapping().toVector2(owner.groundings[i].obj.transform.position), owner.GetMapping().toVector2(((other.transform.position - owner.transform.position) / 2) + owner.transform.position)) <= owner.GroundingUniqueDist) return false;
                }
                Grounding grounding = owner.master.CreateGrounding(((other.transform.position - owner.transform.position) / 2) + owner.transform.position);
                GroundingInfo ownerGrounding = owner.AddGrounding(grounding, 2);
                if (ownerGrounding.obj != null) other.GetComponent<Agent>().AddGrounding(ownerGrounding);
                else
                {
                    owner.master.RemoveGrounding(ownerGrounding);
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public override bool CanGround(Agent owner)
    {
        GameObject[] others = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject other in others)
        {
            if (other == owner.gameObject) continue;
            if (Vector2.Distance(owner.GetMapping().toVector2(other.transform.position), owner.GetMapping().toVector2(owner.transform.position)) <= owner.collisionDist && other.GetComponent<Agent>().CanGround())
            {
                return true;
            }
        }
        return false;
    }
}
