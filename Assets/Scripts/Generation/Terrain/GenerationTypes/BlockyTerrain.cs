using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ### BlockyTerrain
 * -------
 * Class which adapts GenerateTerrain to provide descrete, perlin noise random terrain with sharp edges (blocky)
 */
public class BlockyTerrain : GenerateTerrain
{
    public float BlockFactor = 0.1F;

    /**
     * #### float Y_Operator
     * An overriden method which sets the operation done on the terrain Y value
     */
    protected override float Y_Operator(float inY)
    {
        float returnY = inY;
        returnY *= BlockFactor;
        returnY = (int)(returnY);
        returnY /= BlockFactor;
        return returnY;
    }
}
