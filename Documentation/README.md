# **__Honor's Project Simulation__** **__Documentation__**

## **Source Code Documentation**

## Agents

## Camera

## Generation

## Broadcast

## DataStructures

## Groundings

## PathPlanning

### Agent Class
-------



 #### Agent

 -----

 Class representing the simulation of a physical agent within the digital environment


    public class Agent : MonoBehaviour



 #### void Start()

 Unity event which runs at initialization.

 Initializes a 3x3 re-allocatable grid


    void Start()



 #### int[][] ScanLocalArea()

 Checks the points in a 3x3 grid around the agent and sends that information to the local grid


    int[][] ScanLocalArea()



 #### float ScanDownAtXY(float, float)

 Utilizes Raycasts to check the height of terrain at a given point, expecting flat ground for traversal


    float ScanDownAtXZ(float x, float z)



 #### void Update()

 Unity event running every frame

 Implements wandering functionality (mainly for debugging)


    void Update()

### Goal Class
-------

### BroadcastGoal Class
-------

### GroundedHeading Class
-------

### NearestGrounding Class
-------

### Waypoint Class
-------

### DynamicCoordinateGrid Class
-------



 #### MappingIDs

 -----

 A collection of labels for mapping to be used in grid location identification

 Undefined: Location has yet to be mapped

 Free: Location is traversable

 Full: Location is un-traversable

 Grounding: Location is traversable and contains a grounding which may be demonstrated


    public enum MappingIDs { Undefined, Free, Full, Grounding }



 #### DynamicCoordinateGrid

 -----

 A data structure used to keep a locally valid coordinate grid for querying


    public class DynamicCoordinateGrid : MonoBehaviour



 #### void Move(Vector2, int[][])

 Updates coordinate grid based on a directional move and the result of an environmental scan


    public void Move(Vector2 dir, int[][] localMap)



 #### void SetLocalValues()

 Iterativly sets values through the grid at newly mapped locations


    private void SetLocalValues(int[][] localMap)



 #### Vector2 GetConversionFactor()

 Returns the x and y conversion values to go from array index to object space


    public Vector2 GetConversionFactor() 



 #### void Print()

 Debugging method which prints a grid representation to console


    public void Print()



 #### void Start()

 Unity event which runs at initialization.

 Initializes a new coordinate grid with dimensions 3x3


    private void Start()

### QuadTree Class
-------



 #### QuadTree

 -----

 A data structure class which interfaces with the DynamicCoordinateGrid to create a QuadTree for pathfinding


    public class QuadTree



 #### void Construct(DynamicCoordinateGrid, Vector3)

 Creates the physical QuadTree by initializing one base quad on the entire grid and recursivly partitioning that quad


    public void Construct(DynamicCoordinateGrid mapping, Vector3 offset)



 #### void Print()

 Mainly debugging method to visualize the entire QuadTree in 3D space overlayed on its position in the x and z dimensions


    public void Print()



 #### void Partition(QuadTreeNode, DynamicCoordinateGrid)

 Recursive method for splitting quads depending on the result of the MustBeSubdivided method


    void Partition(QuadTreeNode node, DynamicCoordinateGrid mapping)



 #### bool MustBeSubdivided(QuadTreeNode, DynamicCoordinateGrid)

 A boolean algorithm which compares the points within a quad with their mapping on the agent grid.

 Determines if a split is needed which is then propogated by Partition


    bool MustBeSubdivided(QuadTreeNode node, DynamicCoordinateGrid mapping)

### QuadTreeNode Class
-------



 #### NodeIDs

 -----

 A collection of labels for nodes to be used in pathfinding

 Undefined: Node was created on unmapped areas of the grid

 Free: Node was created on a fully traversable area of the grid

 Full: Node was created on a fully un-traversable area of the grid

 Mixed: Node was created on an area of the grid with mixed traversable and un-traversabele space


    public enum NodeIDs { Undefined, Free, Full, Mixed }



 #### QuadTreeNode

 -----

 A node representing one quad in the QuadTree. Used for pathfinding


    public class QuadTreeNode



 #### QuadTreeNode(float, float, float, float)

 Constructor which takes in x, y location of the bottom left of the quad as well as width, height of the quad


    public QuadTreeNode(float x, float y, float width, float height)



 #### void Print()

 A primarily debugging method which draws the quads overlayed in 3D space for visualization


    public void Print()

### GroundingMethod Class
-------

### LabelAtMeeting Class
-------

### LabelSpatialEntropy Class
-------

### PathPlanner Class
-------



 #### PathPlanner

 -----

 Class to compile data from local grids for pathfinding using QuadTrees


    public class PathPlanner



 #### bool CheckForValidPath(Vector2, DynamicCoordinateGrid)

 Uses the A algorithm to search for a valid path through the local grid's QuadTree

 Returns based on ability to find a valid path


    public List<Vector2> CheckForValidPath(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping)





 #### bool Move(Vector2, DynamicCoordinateGrid, Agent)

 Checks for a valid path and interfaces with agent locomotion to traverse to the destination

 Returns based on ability to conduct a move


    public bool Move(Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping)

### LookAround Class
-------

### PlayerMove Class
-------

## Terrain

### ObjectPool Class
-------

## Foliage

## GenerationTypes

### GenerateChunks Class
-------





Terrain-specific valu
    private List<GameObject> terrain = new List<GameObject>();





void Awake() {

StartCoroutine(init());

}



IEnumerator init(bool inEditor = false)

{

if (inEditor) yield return null;

DestroyAllChildren(inEditor);

rand = Random.Range(0f, 1f);

initGenerateNewChunks();

}



private void DestroyAllChildren(bool inEditor)

{

for (int i = 0; i < gameObject.transform.childCount; i++)

{

DestroyImmediate(gameObject.transform.GetChild(i).gameObject);

}

}



int[] getChunkCoords() {

Error on negative numbers

int oneI = (int)(Player.transform.position.xchunkSize);

int twoI = (int)(Player.transform.position.zchunkSize);



Fix for 0.5==(-0.5) due to truncation

oneI = fixNegativeZero(Player.transform.position.x, oneI);

twoI = fixNegativeZero(Player.transform.position.z, twoI);



return new int[] {oneI, twoI};

}



Pre-truncation comparison for player position

int fixNegativeZero(float pos, int oneI) {

float oneF = poschunkSize;

if (oneF < oneI) return oneI-1;

else return oneI;

}



Initialization method for creating chunk grid

void initGenerateNewChunks() {

currentChunkCoords = getChunkCoords();



Create Pool for terrain chunks

GameObject newPool = Instantiate(poolObject, Vector3.zero, Quaternion.identity) as GameObject;

newPool.transform.parent = gameObject.transform;

chunkPool = newPool.GetComponent<ObjectPool>();

chunkPool.initializePool(((2viewDist) + 1)  ((2  viewDist) + 1), terrainPrefab);



Create Pools for foliage

GenerateFoliage[] foliageGenerators = terrainPrefab.GetComponents<GenerateFoliage>();

if (foliageGenerators != null)

{

foliagePools = new List<ObjectPool[]>();

for (int i = 0; i < ((2  viewDist) + 1)  ((2  viewDist) + 1); i++)

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



Logic handler for new chunk creation

GameObject createChunk(int i, int j, int x) {

GameObject newSection = chunkPool.getObject(); Uses Pooled Object Implementation



Population of individual modules based on collective settings

GenerateTerrain terrainLogic = newSection.GetComponent<GenerateTerrain>();

resizeChunks(terrainLogic);

newSection.transform.position = new Vector3(i  chunkSize, 0, j  chunkSize);



terrainLogic.partitions = detail;

terrainLogic.severity = terrainSeverity;

terrainLogic.scale = terrainScale;

terrainLogic.rand = rand;

terrainLogic.setSeed(SEED);

if (meshMat)

{

newSection.GetComponent<MeshRenderer>().material = meshMat;

}



DEBUG SIDE VERTS

terrainLogic.showSideVerts = debugEdges;

terrainLogic.showVerts = debugVertices;





for (int w = 0; w < foliagePools[x].Length; w++)

{

foliagePools[x][w].transform.parent = terrainLogic.gameObject.transform;

}



terrainLogic.initialize(foliagePools[x]);



VERTEX COPYING FOR ADJACENT CELLS

terrainLogic.AddSection();

terrain.Insert(x, newSection);



return newSection;

}



protected virtual void resizeChunks(GenerateTerrain terrainLogic)

{

terrainLogic.xSize = chunkSize;

terrainLogic.zSize = chunkSize;

}



 Update is called once per frame

void Update()

{

Updates chunks when player crosses chunk boundry

verifyChunkState();

}



void OnValidate()

{

if (RESET)

{

RESET = false;

CODE TO REGENERATE THE MESH HERE

StartCoroutine(init(true));

}

}



void verifyChunkState() {



Checks each direction, loops through each chunk movement (BAD for teleportation implementations:

O(N) where N is # of chunks teleported across, O(N^2) for 2 axis teleport)


    if (!shouldExpand) return;

### GenerateMazeChunks Class
-------

### GenerateTerrain Class
-------

             



 Side meshing verticie
    protected Vector3[][] sidedVertices; Copied from verticies

            



 Main mesh vertecie
    protected Vector3[] vertices;

           



private MeshCollider MC;

private bool bInit = false;

private int SEED = 12345678;



 Start is called before the first frame update

void Start() {



}



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



public float getYAtLocation(Vector2 Location)

{

Vector3 low = new Vector3(Location.x, -99999, Location.y);

Vector3 high = new Vector3(Location.x, 99999, Location.y);



RaycastHit info;

Physics.Linecast(high, low, out info);

return info.point.y;

}



public void setSeed(int inSeed) { SEED = inSeed; }



void InitializeComp() {

Initialization

if (mesh == null) mesh = new Mesh();

else mesh.Clear();



GetComponent<MeshFilter>().mesh = mesh;



Mesh generation logic

CreateShape();

UpdateMesh();



Collider initialization

if (MC == null) MC = GetComponent<MeshCollider>();

MC.sharedMesh = mesh;



Flagged as finished initializing

bInit = true;

}



public void AddSection() {

if (!bInit) {

InitializeComp();

}

}



Basic data structure for easy vert passing between chunks

public struct Side {

public Vector3[] verts;

public int[] i;

public Direction sideName;

}



Main mesh creation logic

void CreateShape() {

vertices = new Vector3[(partitions + 1)  (partitions + 1)]; Create square vertex container

CreateSideContainers();

CreateVerts();

CreateTris();

CreateUVs();

}



void CreateSideContainers() {

sidedVertices = new Vector3[4][];

sidedIndexs = new int[4][];



for (int i = 0; i < 4; i++) {

sidedIndexs[i] = new int[partitions + 1];

sidedVertices[i] = new Vector3[partitions + 1];

}

}



float[] GetParams(int i, int j)

{

float[] arr = new float[2];

arr[0] = (((xSize  partitions)  i) + (gameObject.transform.position.x + constOffset))  scale;

arr[1] = (((zSize  partitions)  j) + (gameObject.transform.position.z + constOffset))  scale;

return arr;

}



protected virtual float YOperator(float inY)

{

return inY;

}



protected virtual void CreateUVs()

{

uvs = new Vector2[vertices.Length];

for (int i = 0; i < vertices.Length; i++)

{

uvs[i] = new Vector2(vertices[i].xxSize, vertices[i].zzSize);

}

}



protected virtual void CreateVerts() {

int count0 = 0;

int count1 = 0;

int count2 = 0;

int count3 = 0;



int x = 0;

for (int i = 0; i <= partitions; i++) {

for (int j = 0; j <= partitions; j++) {



float[] Params = GetParams(i, j);



float y = Mathf.PerlinNoise(Params[0], Params[1])  severity;

y = YOperator(y);





Needs SaveLoad functionality to allow for static pre-runtime generated maps (SEED)


    

### Foliage Class
-------

### GenerateFoliage Class
-------

                

}

else

{

foliageRenderers[i].enabled = false; Hide

}

}

}

}



public void initialize()

{

if (foliageClass.GetComponent<MeshFilter>()) Check if using mesh

{

meshComp = foliageClass.GetComponent<MeshFilter>().sharedMesh; Get Mesh



 Calculate 3D model safe spawning radius (with pre-set offset
    radius += Mathf.Max(meshComp.bounds.size.x  foliageClass.transform.localScale.x, meshComp.bounds.size.z  foliageClass.transform.localScale.z);

                         



instantiateFoliageInstance(xVal, zVal, count);

count++;

}

}



void ClearFoliage()

{

if (foliageObjs == null) return;

for (int i = 0; i < fCount; i++)

{

foliagePool.disableObject(foliageObjs[i]);

}

fCount = 0;

}



void instantiateFoliageInstance(float xVal, float zVal, int count)

{

GameObject foliage = foliagePool.getObject();

foliage.transform.position = new Vector3(xVal, generator.getYAtLocation(new Vector2(xVal, zVal)) + yOffset, zVal);

foliage.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);

foliageObjs[count] = foliage;

foliageRenderers[count] = foliage.GetComponent<Renderer>();

fCount++;

}

}

### GenerateTree Class
-------

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class Tree : GenerateFoliage

{



}

### BlockyTerrain Class
-------

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class BlockyTerrain : GenerateTerrain

{

public float BlockFactor = 0.1F;



protected override float YOperator(float inY)

{

float returnY = inY;

returnY = BlockFactor;

returnY = (int)(returnY);

returnY = BlockFactor;

return returnY;

}

}

### MazeGenerator Class
-------

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class MazeGenerator : GenerateTerrain

{

 EDITOR EXPOSED VALUE
    [SerializeField] private float mazeHeight = 10;

            



[HideInInspector] public bool hasFloor = true;



private int oldVertLength = 0;

private int TrueOldVertLength = 0;



public void setSourceTex(Texture2D inTex) { sourceTex = inTex; }

public int getSourceTextWidth() { return sourceTex.width  unitSize; }

public int getSourceTextHeight() { return sourceTex.height  unitSize; }



protected override void CreateVerts()

{

if (sourceTex == null) return;

xSize = getSourceTextWidth();

zSize = getSourceTextHeight();



if (hasFloor)

{

int x = 0;

Generate a Flat Baseplate

for (int i = 0; i <= partitions; i++)

{

for (int j = 0; j <= partitions; j++)

{

vertices[x] = new Vector3((xSize  partitions)  i, 0, (zSize  partitions)  j);

x++;

}

}

}

TrueOldVertLength = vertices.Length;

GenerateMaze();

}



protected override void CreateUVs()

{

createBaseUVs(); Setup Baseplate Rendering



createWallUVs(); Setup UVs for each wall face



Debug.Log(vertices.Length);

}



private void createBaseUVs()

{

uvs = new Vector2[TrueOldVertLength];

for (int i = 0; i < TrueOldVertLength; i++)

{

uvs[i] = new Vector2(vertices[i].x  xSize, vertices[i].z  zSize);

Debug.Log(vertices[i].x);

Debug.Log(uvs[i].x
    }

