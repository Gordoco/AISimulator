using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelSpatialEntropy : GroundingMethod
{
    [SerializeField] private int entropyExtent = 2;

    public override bool ExecuteGrounding(Agent owner)
    {
        DynamicCoordinateGrid mapping = owner.GetMapping();
        int[] temp = { (int)mapping.lastMappedPos.x, (int)mapping.lastMappedPos.y };
        float b = mapping.GetBlockedRatio(entropyExtent, owner.ScanArea(temp));
        float f = mapping.GetFreeRatio(entropyExtent, owner.ScanArea(temp));

        //if (b + f != 1) return false;

        if (f <= 0 || b <= 0) return false;

        float entropy = (-f * Mathf.Log(f, 2)) - (b * Mathf.Log(b, 2));
        //if (owner.ShouldPrint) Debug.Log("f: " + f + " b: " + b + " entropy: " + entropy);

        f = 0.25f;
        b = 0.75f;
        float target = (-f * Mathf.Log(f, 2)) - (b * Mathf.Log(b, 2));
        //Debug.Log("MAX || entropy: " + entropy);

        if (entropy >= target)
        {
            for (int i = 0; i < owner.groundings.Count; i++)
            {
                if (Vector2.Distance(owner.GetMapping().toVector2(owner.transform.position), owner.GetMapping().toVector2(owner.groundings[i].obj.transform.position)) <= owner.GroundingUniqueDist) return false;
            }
            Grounding grounding = owner.master.CreateGrounding(transform.position);
            owner.AddGrounding(grounding);
            return true;
        }
        return false;
    }

    public override bool CanGround(Agent owner)
    {
        return true;
    }
}
