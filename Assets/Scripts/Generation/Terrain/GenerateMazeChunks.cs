using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * ### GenerateMazeChunks
 * -------
 * Adaptation of the generic GenerateChunks for creating mazes/rooms
 */
public class GenerateMazeChunks : GenerateChunks
{

    [SerializeField] private bool hasFloor = true;
    [SerializeField] private Texture2D sourceTex;

    /**
     * #### void resizeChunks
     * Overriden method from GenerateChunks which interfaces with the MazeGenerator subclass
     */
    protected override void resizeChunks(GenerateTerrain terrainLogic)
    {
        if ((MazeGenerator)terrainLogic != null)
        {
            if (sourceTex) ((MazeGenerator)terrainLogic).setSourceTex(sourceTex);
            chunkSize = ((MazeGenerator)terrainLogic).getSourceTextWidth();
            ((MazeGenerator)terrainLogic).hasFloor = hasFloor;
            if (meshMat)
            {
                int scale = 100 * (((MazeGenerator)terrainLogic).getSourceTextWidth() / 20);
                meshMat.mainTextureScale = new Vector2(scale, scale);
            }
        }
        base.resizeChunks(terrainLogic);
    }
}
