using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundingMethod : MonoBehaviour
{
    public virtual bool ExecuteGrounding(Agent owner)
    {
        return true;
    }

    public virtual bool CanGround(Agent owner)
    {
        return true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
