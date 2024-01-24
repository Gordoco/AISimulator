using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public enum Direction {
    LEFT, 
    RIGHT,
    DOWN,
    UP
}

[ExecuteInEditMode]
/**
 * ### GenerateChunks
 * -------
 * Class for managing terrain sections in a cohesive way procedurally
 */
public class GenerateChunks : MonoBehaviour
{
    /*Editor Exposed Values*/
    public GameObject Player;
    public GameObject terrainPrefab;
    public GameObject poolObject;

    public Material meshMat;

    public int viewDist = 10;
    public int chunkSize = 100;
    public int detail = 10;
    public int SEED = 12345678;

    public float terrainSeverity = 10.0f;
    public float terrainScale = 1f;

    public bool debugEdges = false;
    public bool debugVertices = false;

    public bool shouldExpand = true;

    [SerializeField] private bool RESET = false;
    /***********************/

    /*Terrain-specific values*/
    private List<GameObject> terrain = new List<GameObject>();
    private int[] currentChunkCoords;
    private float rand;
    private ObjectPool chunkPool;
    private List<ObjectPool[]> foliagePools;
    /*************************/

    /**
     * #### void Awake
     * Unity engine event which launches a seperate thread for in editor initialization
     */
    void Awake() {
        StartCoroutine(init());
    }

    /**
     * #### IEnumerator init
     * Multi-threaded method to initialize within editor
     */
    IEnumerator init(bool inEditor = false)
    {
        if (inEditor) yield return null;
        DestroyAllChildren(inEditor);
        rand = Random.Range(0f, 1f);
        initGenerateNewChunks();
    }

    /**
     * #### void DestroyAllChildren
     * Destroys the generated terrain sections (chunks)
     */
    private void DestroyAllChildren(bool inEditor)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
        }
    }

    /**
     * #### int[] getChunkCoords
     * Retrieves the transformed coordinates of the player in terms of the chunk array
     */
    int[] getChunkCoords() {
        //Error on negative numbers
        int oneI = (int)(Player.transform.position.x/chunkSize);
        int twoI = (int)(Player.transform.position.z/chunkSize);

        //Fix for 0.5==(-0.5) due to truncation
        oneI = fixNegativeZero(Player.transform.position.x, oneI);
        twoI = fixNegativeZero(Player.transform.position.z, twoI);

        return new int[] {oneI, twoI};
    }

    //Pre-truncation comparison for player position
    /**
     * #### int fixNegativeZero
     * Pre-truncation comparison for player position to avoid rounding error
     */
    int fixNegativeZero(float pos, int oneI) {
        float oneF = pos/chunkSize;
        if (oneF < oneI) return oneI-1;
        else return oneI;
    }

    //Initialization method for creating chunk grid
    /**
     * #### void initGenerateNewChunks
     * Initialization method for the TerrainSection array, creates the landscape
     */
    void initGenerateNewChunks() {
        currentChunkCoords = getChunkCoords();

        //Create Pool for terrain chunks
        GameObject newPool = Instantiate(poolObject, Vector3.zero, Quaternion.identity) as GameObject;
        newPool.transform.parent = gameObject.transform;
        chunkPool = newPool.GetComponent<ObjectPool>();
        chunkPool.initializePool(((2*viewDist) + 1) * ((2 * viewDist) + 1), terrainPrefab);

        //Create Pools for foliage
        GenerateFoliage[] foliageGenerators = terrainPrefab.GetComponents<GenerateFoliage>();
        if (foliageGenerators != null)
        {
            foliagePools = new List<ObjectPool[]>();
            for (int i = 0; i < ((2 * viewDist) + 1) * ((2 * viewDist) + 1); i++)
            {
                foliagePools.Add(new ObjectPool[foliageGenerators.Length]);
                for (int j = 0; j < foliageGenerators.Length; j++)
                {
                    foliagePools[i][j] = Instantiate(poolObject, Vector3.zero, Quaternion.identity).GetComponent<ObjectPool>();
                    foliagePools[i][j].initializePool(foliageGenerators[j].foliageDensity, foliageGenerators[j].foliageClass);
                }
            }
        }

        int x = 0;
        for (int i = currentChunkCoords[0] - viewDist; i <= currentChunkCoords[0] + viewDist; i++) {
            for (int j = currentChunkCoords[1] - viewDist; j <= currentChunkCoords[1] + viewDist; j++) {
                createChunk(i, j, x);
                x++;
            }
        }
    }

    //Logic handler for new chunk creation
    /**
     * #### GameObject createChunk
     * Utilises the ObjectPool to retrieve and initialize a new chunk in its appropriate position
     */
    GameObject createChunk(int i, int j, int x) {
        GameObject newSection = chunkPool.getObject(); //Uses Pooled Object Implementation

        //Population of individual modules based on collective settings
        GenerateTerrain terrainLogic = newSection.GetComponent<GenerateTerrain>();
        resizeChunks(terrainLogic);
        newSection.transform.position = new Vector3(i * chunkSize, 0, j * chunkSize);

        terrainLogic.partitions = detail;
        terrainLogic.severity = terrainSeverity;
        terrainLogic.scale = terrainScale;
        terrainLogic.rand = rand;
        terrainLogic.setSeed(SEED);
        if (meshMat)
        {
            newSection.GetComponent<MeshRenderer>().material = meshMat;
        }

        //DEBUG SIDE VERTS
        terrainLogic.showSideVerts = debugEdges;
        terrainLogic.showVerts = debugVertices;
        //****************

        for (int w = 0; w < foliagePools[x].Length; w++)
        {
            foliagePools[x][w].transform.parent = terrainLogic.gameObject.transform;
        }

        terrainLogic.initialize(foliagePools[x]);
        
        //VERTEX COPYING FOR ADJACENT CELLS
        terrainLogic.AddSection();
        terrain.Insert(x, newSection);

        return newSection;
    }

    /**
     * #### void resizeChunks
     * Ensures square terrain
     */
    protected virtual void resizeChunks(GenerateTerrain terrainLogic)
    {
        terrainLogic.xSize = chunkSize;
        terrainLogic.zSize = chunkSize;
    }

    // Update is called once per frame
    /**
     * #### void Update
     * Unity editor event called every frame
     * Calls verification method to check for Player movement
     */
    void Update()
    {
        //Updates chunks when player crosses chunk boundry
        verifyChunkState();
    }

    /**
     * #### void OnValidate
     * Checks for editor value RESET in order to allow editor regeneration of terrain
     */
    void OnValidate()
    {
        if (RESET)
        {
            RESET = false;
            //CODE TO REGENERATE THE MESH HERE
            StartCoroutine(init(true));
        }
    }

    /**
     * #### void verifyChunkState
     * Checks if Player's transformed chunk coordinates are within the allowed range, and if not generates chunks to follow the Player
     */
    void verifyChunkState() {
        /*
        Checks each direction, loops through each chunk movement (BAD for teleportation implementations:
        O(N) where N is # of chunks teleported across, O(N^2) for 2 axis teleport)
        */
        if (!shouldExpand) return;
        int staticSpace = Mathf.FloorToInt(viewDist/2f);
        if (getChunkCoords()[0] > currentChunkCoords[0] + staticSpace) for (int i = 0; i < getChunkCoords()[0] - currentChunkCoords[0]; i++) updateChunks(Direction.RIGHT);
        if (getChunkCoords()[0] < currentChunkCoords[0] - staticSpace) for (int i = 0; i < currentChunkCoords[0] - getChunkCoords()[0]; i++) updateChunks(Direction.LEFT);
        if (getChunkCoords()[1] > currentChunkCoords[1] + staticSpace) for (int i = 0; i < getChunkCoords()[1] - currentChunkCoords[1]; i++) updateChunks(Direction.UP);
        if (getChunkCoords()[1] < currentChunkCoords[1] - staticSpace) for (int i = 0; i < currentChunkCoords[1] - getChunkCoords()[1]; i++) updateChunks(Direction.DOWN);
    }

    //Iterates through furthest side destroying it, then iterates through opposite side creating new chunks
    /**
     * #### void updateChunks
     * Iterates through furthest side of the chunk array destroying it, then iterates through opposite side creating new chunks
     */
    void updateChunks(Direction dir) {
        int dist = (2*viewDist)+1;
        int j;
        switch(dir) {
            case Direction.RIGHT:
                {
                    currentChunkCoords[0]++;
                    ObjectPool[][] oldIs = new ObjectPool[dist][];
                    int count = 0;
                    for (int i = dist - 1; i >= 0; i--)
                    { //DESTROY LEFT
                        DestroyTerrainSection(terrain[i]);
                        oldIs[count] = foliagePools[i];
                        count++;
                        terrain.RemoveAt(i);
                        foliagePools.RemoveAt(i);
                    }
                    count = 1;
                    for (int i = dist * (dist - 1); i < dist * dist; i++) //CREATE ON RIGHT
                    {
                        foliagePools.Insert(i, oldIs[dist - count]);
                        GameObject newSection = createChunk(currentChunkCoords[0] + viewDist, currentChunkCoords[1] + (i - (dist * (dist - 1))) - viewDist, i);
                        count++;
                    }
                }
                break;
            case Direction.LEFT:
                {
                    currentChunkCoords[0]--;
                    ObjectPool[][] oldIs = new ObjectPool[dist][];
                    int count = 0;
                    int x = (dist * dist) - 1;
                    int y = dist * (dist - 1);
                    for (int i = x; i >= y; i--)
                    {
                        DestroyTerrainSection(terrain[i]);
                        oldIs[count] = foliagePools[i];
                        count++;
                        terrain.RemoveAt(i);
                        foliagePools.RemoveAt(i);
                    }
                    count = 1;
                    for (int i = 0; i < dist; i++)
                    {
                        foliagePools.Insert(i, oldIs[dist - count]);
                        GameObject newSection = createChunk(currentChunkCoords[0] - viewDist, currentChunkCoords[1] + i - viewDist, i);
                        count++;
                    }
                }
                break;
            case Direction.UP:
                {
                    currentChunkCoords[1]++;
                    ObjectPool[][] oldIs = new ObjectPool[dist][];
                    int count = 0;
                    for (int i = (dist * dist) - dist; i >= 0; i -= dist)
                    { //DESTROY DOWN
                        DestroyTerrainSection(terrain[i]);
                        oldIs[count] = foliagePools[i];
                        count++;
                        terrain.RemoveAt(i);
                        foliagePools.RemoveAt(i);
                    }
                    j = 0;
                    count = dist-1;
                    for (int i = dist - 1; i < dist * dist; i += dist) //CREATE ON TOP
                    {
                        foliagePools.Insert(i, oldIs[count]);
                        GameObject newSection = createChunk(currentChunkCoords[0] + j - viewDist, currentChunkCoords[1] + viewDist, i);
                        count--;
                        j++;
                    }
                }
                break;
            case Direction.DOWN:
                {
                    currentChunkCoords[1]--;
                    ObjectPool[][] oldIs = new ObjectPool[dist][];
                    int count = 0;
                    for (int i = (dist * dist) - 1; i >= dist - 1; i -= dist)
                    {
                        DestroyTerrainSection(terrain[i]);
                        oldIs[count] = foliagePools[i];
                        count++;
                        terrain.RemoveAt(i);
                        foliagePools.RemoveAt(i);
                    }
                    j = 0;
                    count = dist-1;
                    for (int i = 0; i <= (dist * dist) - dist; i += dist)
                    {
                        foliagePools.Insert(i, oldIs[count]);
                        GameObject newSection = createChunk(currentChunkCoords[0] + j - viewDist, currentChunkCoords[1] - viewDist, i);
                        count--;
                        j++;
                    }
                }
                break;
        }
    }

    /**
     * #### void DestroyTerrainSection
     * "Destroys" a chunk by returning it to the ObjectPool
     */
    void DestroyTerrainSection(GameObject terrain)
    {
        chunkPool.disableObject(terrain);
    }
}
