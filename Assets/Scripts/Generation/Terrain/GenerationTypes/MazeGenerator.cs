using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class which adapts the GenerateTerrain archetype to utilize a png image in order to construct maze-like structures
 */
public class MazeGenerator : GenerateTerrain
{
    /* EDITOR EXPOSED VALUES */
    [SerializeField] private float mazeHeight = 10;
    [SerializeField] private int unitSize = 10;

    [SerializeField] private Texture2D sourceTex;
    /* * * * * * * * * * * * */

    [HideInInspector] public bool hasFloor = true;

    private int oldVertLength = 0;
    private int TrueOldVertLength = 0;

    /**
     * #### void setSourceTex
     * Mutator for the Texture2D value
     */
    public void setSourceTex(Texture2D inTex) 
    { sourceTex = inTex; }

    /**
     * #### int getSourceTextWidth
     * Accessor for the Texture2D width
     */
    public int getSourceTextWidth() 
    { return sourceTex.width * unitSize; }

    /**
     * #### int getSourceTextHeight
     * Accessor for the Texture2D height
     */
    public int getSourceTextHeight() 
    { return sourceTex.height * unitSize; }

    /**
     * #### void CreateVerts
     * Overriden method from GenerateTerrain which creates the baseplate and initializes maze generation
     */
    protected override void CreateVerts()
    {
        if (sourceTex == null) return;
        xSize = getSourceTextWidth();
        zSize = getSourceTextHeight();

        if (hasFloor)
        {
            int x = 0;
            //Generate a Flat Baseplate
            for (int i = 0; i <= partitions; i++)
            {
                for (int j = 0; j <= partitions; j++)
                {
                    vertices[x] = new Vector3((xSize / partitions) * i, 0, (zSize / partitions) * j);
                    x++;
                }
            }
        }
        TrueOldVertLength = vertices.Length;
        GenerateMaze();
    }

    /**
     * #### void CreateUVs
     * Overriden method from GenerateTerrain which maps the texture coordinates to the generated terrain
     */
    protected override void CreateUVs()
    {
        createBaseUVs(); //Setup Baseplate Rendering

        createWallUVs(); //Setup UVs for each wall face

        //Debug.Log(vertices.Length);
    }

    /**
     * #### void createBaseUVs
     * Overriden method from GenerateTerrain which maps the base texture coordinates
     */
    private void createBaseUVs()
    {
        uvs = new Vector2[TrueOldVertLength];
        for (int i = 0; i < TrueOldVertLength; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / xSize, vertices[i].z / zSize);
            /*Debug.Log(vertices[i].x);
            Debug.Log(uvs[i].x);*/
        }
    }

    /**
     * #### void createWallUVs
     * Overriden method from GenerateTerrain which maps the wall texture coordinates
     */
    private void createWallUVs()
    {
        //INEFFICIENT BUT USEFUL FOR UNDERSTANDING
        Vector2[] finalUVs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++) finalUVs[i] = uvs[i];
        uvs = finalUVs;

        int numFaceVerts = vertices.Length - TrueOldVertLength;

        float x = (float)unitSize / (float)xSize;
        float z = (float)unitSize / (float)zSize;

        //Each Cube
        for (int i = TrueOldVertLength; i < numFaceVerts + 20; i+=20)
        {
            for (int j = 0; j < 5; j++)
            {
                uvs[i + (j * 4) + 3] = new Vector2(0, 0);
                uvs[i + (j * 4) + 2] = new Vector2(x, 0);
                uvs[i + (j * 4) + 1] = new Vector2(x, z);
                uvs[i + (j * 4) + 0] = new Vector2(0, z);
            }
        }
    }

    /**
     * #### void CreateTris
     * Overriden method from GenerateTerrain which maps the vertex indecies together into graphical triangles for rendering
     */
    protected override void CreateTris()
    {
        int j;
        if (hasFloor)
        {
            base.CreateTris();
            j = triangles.Length;
        }
        else j = 0;

        //(((vertices.Length - TrueOldVertLength)/20)*30):
        //  20 = num vertices per unitSize cube
        //  30 = num triangles per unitSize cube
        int[] newTris = new int[j + (((vertices.Length - TrueOldVertLength)/20)*30)];

        for (int i = 0; i < j; i++) newTris[i] = triangles[i]; //Primitive re-allocation of array
        triangles = newTris;
        for (int k = TrueOldVertLength; k < vertices.Length; k+=20)
        {
            for (int i = 0; i < 5; i++)
            {
                createTrianglesFromVertexIndex(triangles, j, k + (i * 4));
                j += 6;
            }
        }
    }

    //Initial attempt to abstract triangle generation
    //Need to standardize vertex ordering
    /**
     * #### void createTrianglesFromVertexIndex
     * Abstraction of triangle generation for range of vertex indecies
     */
    private void createTrianglesFromVertexIndex(int[] tris, int j /* start tri index */, int k /* start vert index */)
    {
        tris[j + 0] = k + 0;
        tris[j + 1] = k + 1;
        tris[j + 2] = k + 3;

        tris[j + 3] = k + 1;
        tris[j + 4] = k + 2;
        tris[j + 5] = k + 3;
    }

    /**
     * #### void GenerateMaze
     * Utilizes helper method to generate a box mesh on the terrain
     * in a corresponding place to each black pixel in the input image
     */
    void GenerateMaze()
    { 
        if (sourceTex == null) return;
        Color[] pixels = sourceTex.GetPixels();
        for (int i = 0; i < sourceTex.height; i++)
        {
            for (int j = 0; j < sourceTex.width; j++)
            {
                Color pixel = pixels[j + (i * sourceTex.width)];

                if (pixel.Equals(Color.black))
                {
                    GenerateBoxOnMesh(new Vector3((unitSize * j),
                        0, (unitSize * i)), unitSize, unitSize);
                }
            }
        }
    }

    /**
     * #### void GenerateBoxOnMesh
     * Helper method for GenerateMaze to create a box of specified size on the mesh
     */
    void GenerateBoxOnMesh(Vector3 topLeft, float x, float z)
    {
        oldVertLength = vertices.Length;
        Vector3[] newVerts = new Vector3[vertices.Length + 20];
        for (int i = 0; i < vertices.Length; i++) newVerts[i] = vertices[i];
        vertices = newVerts;

        int j = oldVertLength;

        //TOP
        vertices[j + 3] = new Vector3(topLeft.x, mazeHeight, topLeft.z);
        vertices[j + 0] = new Vector3(topLeft.x, mazeHeight, topLeft.z + z);
        vertices[j + 1] = new Vector3(topLeft.x + x, mazeHeight, topLeft.z + z);
        vertices[j + 2] = new Vector3(topLeft.x + x, mazeHeight, topLeft.z);

        //Positive X dir
        vertices[j + 4] = new Vector3(topLeft.x, mazeHeight, topLeft.z);
        vertices[j + 5] = new Vector3(topLeft.x + x, mazeHeight, topLeft.z);
        vertices[j + 6] = new Vector3(topLeft.x + x, 0, topLeft.z);
        vertices[j + 7] = new Vector3(topLeft.x, 0, topLeft.z);

        //Positive Z dir
        vertices[j + 8] = new Vector3(topLeft.x, mazeHeight, topLeft.z + z);
        vertices[j + 9] = new Vector3(topLeft.x, mazeHeight, topLeft.z);
        vertices[j + 10] = new Vector3(topLeft.x, 0, topLeft.z);
        vertices[j + 11] = new Vector3(topLeft.x, 0, topLeft.z + z);

        //Positive Z -> X
        vertices[j + 12] = new Vector3(topLeft.x + x, mazeHeight, topLeft.z + z);
        vertices[j + 13] = new Vector3(topLeft.x, mazeHeight, topLeft.z + z);
        vertices[j + 14] = new Vector3(topLeft.x, 0, topLeft.z + z);
        vertices[j + 15] = new Vector3(topLeft.x + x, 0, topLeft.z + z);

        //Positive X -> Z
        vertices[j + 16] = new Vector3(topLeft.x + x, mazeHeight, topLeft.z);
        vertices[j + 17] = new Vector3(topLeft.x + x, mazeHeight, topLeft.z + z);
        vertices[j + 18] = new Vector3(topLeft.x + x, 0, topLeft.z + z);
        vertices[j + 19] = new Vector3(topLeft.x + x, 0, topLeft.z);
    }
}
