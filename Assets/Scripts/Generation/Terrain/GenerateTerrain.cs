using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshCollider))]
/**
 * Class which handles the terrain generation for a specific TerrainSection within the array of terrain chunks
 */
public class GenerateTerrain : MonoBehaviour
{
    Mesh mesh;

    /* Configuration Variables */
    public int xSize = 100;
    public int zSize = 100;
    public int constOffset = 222;
    public int partitions = 10;
    public float severity = 10;
    public bool showVerts = false;
    public bool showSideVerts = false;
    public float scale = 1f;
    public float rand = 0.1f;

    public int id;
    public GenerateFoliage[] foliageGenerators;
    /* * * * * * * * * * * * * */

    /* Side meshing verticies */
    protected Vector3[][] sidedVertices; //Copied from verticies
    protected int[][] sidedIndexs; //Matched to indecies in verticies
    /* * * * * * * * * * * * */

    /* Main mesh vertecies */
    protected Vector3[] vertices;
    protected int[] triangles;
    protected Vector2[] uvs;
    /* * * * * * * * * * * */

    private MeshCollider MC;
    private bool bInit = false;
    private int SEED = 12345678;

    // Start is called before the first frame update
    void Start() {
        
    }

    /**
     * #### void initialize
     * Creates the section of terrain using the prescribed algoritms
     */
    public void initialize(ObjectPool[] foliagePools) {
        InitializeComp();
        foliageGenerators = GetComponents<GenerateFoliage>();
        for (int i = 0; i < foliageGenerators.Length; i++) {
            if (foliageGenerators[i] != null)
            {
                foliageGenerators[i].setFoliagePool(foliagePools[i]);
                foliageGenerators[i].initialize();
            }
        }
    }

    /**
     * #### void getYAtLocation
     * Helper method for utilizing Linecasts to determine the precise height at any x, y (x, z in world space) location
     */
    public float getYAtLocation(Vector2 Location)
    {
        Vector3 low = new Vector3(Location.x, -99999, Location.y);
        Vector3 high = new Vector3(Location.x, 99999, Location.y);

        RaycastHit info;
        Physics.Linecast(high, low, out info);
        return info.point.y;
    }

    /**
     * #### void setSeed
     * Mutator method for the SEED value
     */
    public void setSeed(int inSeed) 
    { SEED = inSeed; }

    /**
     * #### void InitializeComp
     * Sets up mesh component for terrain generation and vertex/triangle insertion
     */
    void InitializeComp() {
        //Initialization
        if (mesh == null) mesh = new Mesh();
        else mesh.Clear();

        GetComponent<MeshFilter>().mesh = mesh;

        //Mesh generation logic
        CreateShape();
        UpdateMesh();

        //Collider initialization
        if (MC == null) MC = GetComponent<MeshCollider>();
        MC.sharedMesh = mesh;

        //Flagged as finished initializing
        bInit = true;
    }

    /**
     * #### void AddSection
     * Interface method for use within GenerateChunks
     */
    public void AddSection() {
        if (!bInit) {
            InitializeComp();
        }
    }

    //Basic data structure for easy vert passing between chunks
    /**
     * #### struct Side
     * Data structure for passing all relevent data to represent one side of the TerrainSection
     */
    public struct Side {
        public Vector3[] verts;
        public int[] i;
        public Direction sideName;
    }

    //Main mesh creation logic
    /**
     * #### void CreateShape
     * Master method for creating the terrain
     */
    void CreateShape() {
        vertices = new Vector3[(partitions + 1) * (partitions + 1)]; //Create square vertex container
        CreateSideContainers();
        CreateVerts();
        CreateTris();
        CreateUVs();
    }

    /**
     * #### void CreateSideContainers
     * Initialized sub vertex arrays for holding the side vertices for easy alignment
     */
    void CreateSideContainers() {
        sidedVertices = new Vector3[4][];
        sidedIndexs = new int[4][];

        for (int i = 0; i < 4; i++) {
            sidedIndexs[i] = new int[partitions + 1];
            sidedVertices[i] = new Vector3[partitions + 1];
        }
    }

    /**
     * #### float[] GetParams
     * Gets information from array indexes
     */
    float[] GetParams(int i, int j)
    {
        float[] arr = new float[2];
        arr[0] = (((xSize / partitions) * i) + (gameObject.transform.position.x + constOffset)) / scale;
        arr[1] = (((zSize / partitions) * j) + (gameObject.transform.position.z + constOffset)) / scale;
        return arr;
    }

    /**
     * #### void Y_Operator
     * Virtual method to allow customization of the specifics of the terrain height choices
     */
    protected virtual float Y_Operator(float inY)
    {
        return inY;
    }

    /**
     * #### void CreateUVs
     * Maps the texture coordinates to the newly generated terrain
     */
    protected virtual void CreateUVs()
    {
        uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x/xSize, vertices[i].z/zSize);
        }
    }

    /**
     * #### void CreateVerts
     * Creates the verticies of the terrain in a grid in x, z space and using perlin noise for the y height
     */
    protected virtual void CreateVerts() {
        int count0 = 0;
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;

        int x = 0;
        for (int i = 0; i <= partitions; i++) {
            for (int j = 0; j <= partitions; j++) {

                float[] Params = GetParams(i, j);

                float y = Mathf.PerlinNoise(Params[0], Params[1]) * severity;
                y = Y_Operator(y);

                /*
                Needs Save/Load functionality to allow for static pre-runtime generated maps (SEED)
                */

                vertices[x] = new Vector3((xSize/partitions) * i, y, (zSize/partitions) * j);

                if (i == 0) { //Save LEFT edge
                    sidedVertices[0][count0] = vertices[x];
                    sidedIndexs[1][count0] = x;
                    count0++;
                }
                else if (i == partitions) { //Save RIGHT edge
                    sidedVertices[1][count1] = vertices[x];
                    sidedIndexs[0][count1] = x;
                    count1++;
                }

                if (j == 0) { //Save DOWN edge
                    sidedVertices[2][count2] = vertices[x];
                    sidedIndexs[3][count2] = x;
                    count2++;
                }
                else if (j == partitions) { //Save UP edge
                    sidedVertices[3][count3] = vertices[x];
                    sidedIndexs[2][count3] = x;
                    count3++;
                }
                x++;
            }
        }
    }

    /**
     * #### void CreateTris
     * Creates triangles for rendering from the vertex array
     */
    protected virtual void CreateTris() {
        triangles = new int[partitions*partitions*6];

        int vert = 0;
        int tris = 0;
        for (int j = 0; j < partitions; j++) {
            for (int i = 0; i < partitions; i++) {
                
                //Generates the 6 triangles needed for each partition pair
                triangles[tris + 1] = vert + 0;
                triangles[tris + 0] = vert + partitions + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 4] = vert + 1;
                triangles[tris + 3] = vert + partitions + 1;
                triangles[tris + 5] = vert + partitions + 2;

                vert++;
                tris+=6;
            }
            vert++;
        }
    }

    //Re-init mesh
    /**
     * #### void UpdateMesh
     * Safely regenerates terrain at runtime
     */
    void UpdateMesh() {
        //Reset mesh for future use
        mesh.Clear();

        //Updates the mesh values
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        //Forces a recomputation of the mesh structure
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    //Editor Preview/Debug Method
    /**
     * #### void OnDrawGizmos
     * Unity engine event which is called for debugging purposes
     * Draws debugging icons on vertices 
     */
    private void OnDrawGizmos() {
        if (vertices == null) {
            return;
        }

        //Visual representation of each vertex in the chunk
        if (showVerts) {
            for (int i = 0; i < vertices.Length; i++) {
                Gizmos.color = new Color(uvs[i].x * 10, 0, uvs[i].y * 10);
                Gizmos.DrawSphere(gameObject.transform.position + vertices[i], 1.0f);
            }
        }

        //Visual color-coded representation of each side edge of each chunk (in vertices)
        if (showSideVerts) {
            if (sidedVertices != null) {
                Color[] colors = { Color.blue, Color.yellow, Color.red, Color.green };
                for (int i = 0; i < sidedVertices.Length; i++) {
                    Gizmos.color = colors[i];
                    for (int j = 0; j < sidedVertices[0].Length; j++) {
                        Gizmos.DrawSphere(gameObject.transform.position + sidedVertices[i][j], 1.0f);
                    }
                }
            }
        }

        //Runs the generation code in editor
        Start();
    }
}
