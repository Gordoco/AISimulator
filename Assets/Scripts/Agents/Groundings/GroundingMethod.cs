using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Superclass for grounded construction by agents
 */
public class GroundingMethod : MonoBehaviour
{
    /**
     * #### bool ExecuteGrounding(Agent)
     * Superclass hook for grounding construction
     */
    public virtual bool ExecuteGrounding(Agent owner)
    {
        return true;
    }

    /**
     * #### bool CanGround(Agent)
     * Superclass hook for grounding verification
     */
    public virtual bool CanGround(Agent owner)
    {
        return true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
