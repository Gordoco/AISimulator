# **__Honor's Project Simulation__** **__Documentation__**

## **Source Code Documentation**

## Agents

### Agent
-------



 Class representing the simulation of a physical agent within the digital environment


    [RequireComponent(typeof(BroadcastGoal))]



 #### void Init()

 Initializes a 3x3 re-allocatable grid


    public void Init()



 #### GroundingInfo AddGrounding(Grounding, int = 1)

 Adds a new grounding to agent, for use when constructed through GroundingMethod subclass


    public GroundingInfo AddGrounding(Grounding newGrounding, int localConsistency = 1)



 #### GroundingInfo AddGrounding(GroundingInfo)

 Adds an existing grounding to agent, for use when sharing groundings through demonstration


    public GroundingInfo AddGrounding(GroundingInfo grounding)



 #### bool CanGround()

 Checks if the cooldown for grounding has elapsed


    public bool CanGround()



 #### bool RequestRecieveDemonstration()

 Checks if the cooldown for grounding has elapsed


    public bool RequestRecieveDemonstration()



 #### void RecieveGoalBroadcast(GroundingInfo)

 Simple broadcast handle for a single grounding


    public void RecieveGoalBroadcast(GroundingInfo info)



 #### void RecieveGoalBroadcast(GroundingTree)

 Complex broadcast handle for the waypoint system


    public void RecieveGoalBroadcast(GroundingTree info)



 #### GroundingInfo RecieveDemonstration(GroundingInfo)

 Handle a foreign agent demonstration and return the GroundingInfo that is kept after analysis


    public GroundingInfo RecieveDemonstration(GroundingInfo input)



 #### void OnCollisionEnter(Collision)

 Unity method for evaluating collisions.

 Used to error check collision prevention systems.


    public void OnCollisionEnter(Collision collision)



 #### int[][] ScanLocalArea()

 Checks the points in a 3x3 grid around the agent and sends that information to the local grid


    public int[][] ScanArea(int[] center)



 #### float ScanDownAtXY(float, float)

 Utilizes Raycasts to check the height of terrain at a given point, expecting flat ground for traversal


    float ScanDownAtXZ(float x, float z)



 #### void Update()

 Unity event running every frame

 Implements wandering functionality (mainly for debugging)


    void FixedUpdate()



 #### Wander Algorithm

 Currently in a test implementation format, should implement an efficent domain-independant exploration algorithm


    private void WanderAlgorithm()



 #### PathExecution()

 Utilizes calculated path to plan motion direction for the next timestep


    private void PathExecution()

### SpawnAgents
-------



 #### Grounding CreateGrounding(Vector3)

 Creates a new object to represent a grounding


    public Grounding CreateGrounding(Vector3 position)



 #### void RemoveGrounding(GroundingInfo)

 Destroys and removes all reference to the specified Grounding, should be done after agent local references are removed


    public void RemoveGrounding(GroundingInfo grounding)



 #### int GetUniqueID()

 Creates a new integer ID for use in naming groundings


    public int GetUniqueID()



 #### void InitAgents(bool bStart = false)

 Initializes an experiment when bStart == true, or an iteration when false


    public void InitAgents(bool bStart = false)



 #### bool CheckValidLoc(Vector3)

 Ensures a location in space is clear to spawn an agentthe goal in


    private bool CheckValidLoc(Vector3 location)



 #### void CompleteIteration()

 Adds Logging for an iteration to temp array


    void CompleteIteration()



 #### void CompleteSimulation()

 Adds end of experiment logging to temp array;


    void CompleteSimulation()



 #### void OutputResult()

 Writes complete experiment results to a logging file based on temp array


    void OutputResult()

### TEST_TriggerSpecifiedAgents
-------



 Test Script for allowing hand placed agents to be included in simulations


    public class TESTTriggerSpecifiedAgents : MonoBehaviour

### BroadcastGoal
-------



 Superclass for grounded broadcasting by agents


    public class BroadcastGoal : MonoBehaviour

### Waypoint
-------



 Class implementing BroadcastGoal responsible for waypoint tree construction and broadcasting


    public class Waypoint : BroadcastGoal

### DynamicCoordinateGrid
-------



 A data structure used to keep a locally valid coordinate grid for querying


    public class DynamicCoordinateGrid : MonoBehaviour



 #### void Move(Vector2, int[][])

 Updates coordinate grid based on a directional move and the result of an environmental scan


    public void Move(Vector2 dir, int[][] localMap, float inverseMoveSpeed)



 #### void SetLocalValues()

 Iterativly sets values through the grid at newly mapped locations


    private void SetLocalValues(int[][] localMap)



 #### Vector2 GetConversionFactor()

 Returns the x and y conversion values to go from array index to object space


    public Vector2 GetConversionFactor()



 #### void Print()

 Debugging method which prints a grid representation to console


    public void Print(float inverseMoveSpeed = 0.2f)

### GenericDigraph
-------



 Class representing a generic, directed graph with weights corresponding to euclidian distance between vectors


    public class GenericDigraph



 #### GenericDigraph(List<Vector2> List<DirectedEdge>)

 Constructor expecting pre-formatted verticies and edges


    public GenericDigraph(List<Vector2> verts, List<DirectedEdge> edges)



 #### List<int> GetNeighborIndecies(int)

 Retrieve all neighbors from index input


    public List<int> GetNeighborIndecies(int vertIndex)



 #### List<int> GetNeighborIndecies(Vector2)

 Retrieve all neighbors from location input


    public List<int> GetNeighborIndecies(Vector2 vert)



 #### List<Vector2> GetNeighborIndecies(int)

 Retrieve all neighbors as vectors from index input


    public List<Vector2> GetNeighbors(int vertIndex)



 #### List<Vector2> GetNeighborIndecies(Vector2)

 Retrieve all neighbors as vectors from vector input


    public List<Vector2> GetNeighbors(Vector2 vert)



 #### void Print()

 Debugging method for creating a visual representation of the graph in world space


    public void Print(float height, float time = 0)

### MappingIDs
-------



 A collection of labels for mapping to be used in grid location identification

 Undefined: Location has yet to be mapped

 Free: Location is traversable

 Full: Location is un-traversable

 Grounding: Location is traversable and contains a grounding which may be demonstrated


    public enum MappingIDs { Undefined, Free, Full }
### NodeDepth
-------



 Struct for containing the node and depth information during recursive tree searches


    public struct NodeDepth

### NodeIDs
-------



 A collection of labels for nodes to be used in pathfinding

 Undefined: Node was created on unmapped areas of the grid

 Free: Node was created on a fully traversable area of the grid

 Full: Node was created on a fully un-traversable area of the grid

 Mixed: Node was created on an area of the grid with mixed traversable and un-traversabele space


    public enum NodeIDs { Undefined, Free, Full, Mixed }
### QuadTree
-------



 A data structure class which interfaces with the DynamicCoordinateGrid to create a QuadTree for pathfinding


    public class QuadTree



 #### void Construct(DynamicCoordinateGrid, Vector3)

 Creates the physical QuadTree by initializing one base quad on the entire grid and recursivly partitioning that quad


    public void Construct(DynamicCoordinateGrid mapping, Vector3 offset, float time = 0.1f, bool bPrint = false)



 QuadTreeNode GetNode(Vector2)

 Recursively finds the correct leaf node at the specified (X, Z) world location


    public QuadTreeNode GetNode(Vector2 location)



 #### QuadTreeNode GetNearestFreeNode(Vector2)

 Retrieves the closest node in the QuadTree with NodeID.Free


    public QuadTreeNode GetNearestFreeNode(Vector2 location)



 #### List<NodeDepth> GetFurthestFreeNodeDepths(Vector2)

 Retrieves a sorted list of all nodes by distance to specified location, along with Node Depth in the QuadTree


    public List<NodeDepth> GetFurthestFreeNodeDepths(Vector2 location)



 #### QuadTreeNode GetFurthestFreeNodes(Vector2)

 Takes in an (X, Z) world location and returns a list of all nodes which are free in sorted order on distance


    public List<QuadTreeNode> GetFurthestFreeNodes(Vector2 location)



 #### void Shuffle<T>(List<T>)

 Simple array shuffler


    public void Shuffle<T>(List<T> ts)



 #### List<QuadTreeNode> GetFurthestFreeNodesInDir(Vector2, Vector2) 

 Based on a given direction, gets furthest free node that is necessarily within the same connected component as the location specified


    public List<QuadTreeNode> GetFurthestFreeNodesInDir(Vector2 location, Vector2 dir) 



 List<QuadTreeNode> GetLeaves()

 Method to return all the leaves of the quad tree for path finding graph construction


    public List<QuadTreeNode> GetLeaves()



 #### bool IsChild(QuadTreeNode, QuadTreeNode)

 Checks if the first parameter is a child of the second in the QuadTree


    public bool IsChild(QuadTreeNode potChild, QuadTreeNode parent)



 #### void Print()

 Mainly debugging method to visualize the entire QuadTree in 3D space overlayed on its position in the x and z dimensions


    public void Print(float time)



 #### void Partition(QuadTreeNode, DynamicCoordinateGrid)

 Recursive method for splitting quads depending on the result of the MustBeSubdivided method


    void Partition(QuadTreeNode node, DynamicCoordinateGrid mapping, bool bPrint = false)



 #### bool MustBeSubdivided(QuadTreeNode, DynamicCoordinateGrid)

 A boolean algorithm which compares the points within a quad with their mapping on the agent grid.

 Determines if a split is needed which is then propogated by Partition


    bool MustBeSubdivided(QuadTreeNode node, DynamicCoordinateGrid mapping, bool bPrint = false)

### QuadTreeNode
-------



 A node representing one quad in the QuadTree. Used for pathfinding


    public class QuadTreeNode



 #### QuadTreeNode(float, float, float, float)

 Constructor which takes in x, y location of the bottom left of the quad as well as width, height of the quad


    public QuadTreeNode(float x, float y, float width, float height)



 #### QuadTreeNode GetNode(Vector2)

 Gets the leaf node which contains the specified location


    public QuadTreeNode GetNode(Vector2 location)



 #### bool CheckIfWithin(Vector2)

 Checks if a specified point is within the bounds for the current node


    public bool CheckIfWithin(Vector2 point)



 #### bool NodeIsLeaf()

 Checks if node has been subdivided


    public bool NodeIsLeaf()



 #### List<NodeDepth> GetChildren(int depth = 0)

 Returns all children of a QuadTree from this node on, utilizes recursion


    public List<NodeDepth> GetChildren(int depth = 0)



 #### List<NodeDepth> GetFreeChildren()

 Gets all children within the QuadTree which are free for pathing


    public List<NodeDepth> GetFreeChildren(int depth = 0)



 #### List<QuadTreeNode> GetDirections()

 Returns the neighbors of the node within the QuadTree


    public List<QuadTreeNode> GetDirections()



 #### void Print()

 A primarily debugging method which draws the quads overlayed in 3D space for visualization


    public void Print(float time)

### Side
-------



 An Enum for easy readability of NSEW


    public enum Side { N, S, E, W }
### GroundingMethod
-------



 Superclass for grounded construction by agents


    public class GroundingMethod : MonoBehaviour



 #### bool ExecuteGrounding(Agent)

 Superclass hook for grounding construction


    public virtual bool ExecuteGrounding(Agent owner)



 #### bool CanGround(Agent)

 Superclass hook for grounding verification


    public virtual bool CanGround(Agent owner)

### LabelAtMeeting
-------



 Class to implement Label-At-Meeting communication strategy


    public class LabelAtMeeting : GroundingMethod



 #### override bool ExecuteGrounding(Agent)

 Superclass override which should be called on a clock based on groundingCreationCooldown


    public override bool ExecuteGrounding(Agent owner)



 #### override bool CanGround(Agent)

 Superclass override which checks if grounding is valid in a non-destructive way


    public override bool CanGround(Agent owner)

### LabelSpatialEntropy
-------



 Class to implement Label-Spatial-Entropy communication strategy


    public class LabelSpatialEntropy : GroundingMethod



 #### override bool ExecuteGrounding(Agent)

 Superclass override which should be called on a clock based on groundingCreationCooldown


    public override bool ExecuteGrounding(Agent owner)



 #### override bool CanGround(Agent)

 Superclass override which checks if grounding is valid in a non-destructive way


    public override bool CanGround(Agent owner)

### NoGrounding
-------



 Placeholder component for running baselines


    public class NoGrounding : GroundingMethod

### PathNode
-------



 Class for storing the A tree structure during pathfinding


    public class PathNode

### PathPlanner
-------



 Class to compile data from local grids for pathfinding using QuadTrees


    public class PathPlanner



 #### PathInfo

 Struct to hold path information, path variable should be exactly 2 larger than nodes array to account for start and end locations


    public struct PathInfo



 #### bool CheckForValidPath(Vector2, DynamicCoordinateGrid)

 Uses the A star algorithm to search for a valid path through the local grid's QuadTree

 Returns based on ability to find a valid path


    public PathInfo CheckForValidPath(QuadTree tree, Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f, bool bPrint = false)



 #### List<int> GenericAStar(GenericDigraph)

 Takes in a generic graph with the assumption that the first element of the vertex array is the start

 and the last element is the end of the desired path.

 Returns a list of vertex indecies describing the calculated path.


    private List<int> GenericAStar(GenericDigraph graph, float time = 0f, float height = 1, bool shouldPrint = false)



 #### List<Vector2> OptimizePath(List<Vector2>)

 Assumes path input is valid with non-zero length, applies rubber-banding to optimize path length


    private List<Vector2> OptimizePath(PathInfo pathInfo, float time = 0.2f, bool bPrint = false)



 List<Vector2> VisibilitySimplification(PathInfo, List<Vector2>)

 Method which performs "visibility" checks on path segments and removes unessesary points which don't affect path validity

 Specifically, removes points which, when skipped, allow for the path to remain within the same quads as before.


    private List<Vector2> VisibilitySimplification(PathInfo originalPath, List<Vector2> pathPoints, bool bShouldPrint = false, float time = 0.2f, float height = 1)



 bool Intersects(Vector2, Vector2, Vector2, Vector2, out Vector2)

 Line intersection helper method, takes in two lines and outputs the boolean intersaction and intersection location


    private bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)



 GenericDigraph GenerateGraphFromQuadTree(Vector2, Vector2, QuadTree)

 Generates a graphical representation of a QuadTree along with a start and end location for pathfinding

 Creates the graph as two uni-directional edges connecting the centers of each neighboring quad in the tree.


    private GenericDigraph GenerateGraphFromQuadTree(Vector2 startLoc, Vector2 endLoc, QuadTree tree)



 #### GenericDigraph GenerateGraphFromQuadTreePath(PathInfo)

 Method for constructing a weighted digraph from the edges of the quads determined by A


    private GenericDigraph GenerateGraphFromQuadTreePath(PathInfo pathInfo)



 #### Vector2[] GetEdgePoints(QuadTreeNode, QuadTreeNode)

 Determines the points of the smallest quad which lie on the boundry of the largest quad


    private Vector2[] GetEdgePoints(QuadTreeNode curr, QuadTreeNode next)



 #### bool Move(Vector2, DynamicCoordinateGrid, Agent)

 Checks for a valid path and interfaces with agent locomotion to traverse to the destination

 Returns based on ability to conduct a move


    public bool Move(QuadTree tree, Vector2 initialLocation, Vector2 location, DynamicCoordinateGrid mapping, float time = 0.2f, bool bPrint = false)



 #### void CancelPath(bool = false, Vector3 = new Vector3())

 Cancels current pathfinding operation


    public void CancelPath(bool bCollision = false, Vector3 collisionDir = new Vector3())

### ReactiveCollisionPrevention
-------



 Class which ensures no collisions happen in environment by detecting them a frame early and taking necessary prevention measures


    public class ReactiveCollisionPrevention

## Camera

### LookAround
-------



 Simple class for player mouse directed camera


    public class LookAround : MonoBehaviour



 #### Start

 Unity engine event called at start of runtime

 Locks cursor to window


    void Start()



 #### Update

 Unity engine event called every frame

 Handles user IO


    void Update()

### PlayerMove
-------



 Simple class for limited Player camera movement


    public class PlayerMove : MonoBehaviour

### UIHandler
-------



 Helper Script for managing the various UI objects in player view


    public class UIHandler : MonoBehaviour

## Generation

### ObjectPool
-------



 Implementation of an abstract object pool for efficient mass object management


    public class ObjectPool : MonoBehaviour



 #### void OnDestroy

 Unity message for when an object is destroyed

 Calls destroy on each owned object in order to prevent an object cascadememory leak


    private void OnDestroy()



 #### void initializePool

 Creates the set of objects of a given size and prepares the data structure for their retrieval


    public void initializePool(int poolSize, GameObject objectClass) Initial instantiation



 #### GameObject getObject

 Handles the retrieval of a new object from the pool


    public GameObject getObject()



 #### void disableObject

 A method for returning an unused object to the pool


    public void disableObject(GameObject obj)

### GenerateChunks
-------



 Class for managing terrain sections in a cohesive way procedurally


    public class GenerateChunks : MonoBehaviour



 #### void Awake

 Unity engine event which launches a seperate thread for in editor initialization


    void Awake() {



 #### IEnumerator init

 Multi-threaded method to initialize within editor


    IEnumerator init(bool inEditor = false)



 #### void DestroyAllChildren

 Destroys the generated terrain sections (chunks)


    private void DestroyAllChildren(bool inEditor)



 #### int[] getChunkCoords

 Retrieves the transformed coordinates of the player in terms of the chunk array


    int[] getChunkCoords() {



 #### int fixNegativeZero

 Pre-truncation comparison for player position to avoid rounding error


    int fixNegativeZero(float pos, int oneI) {



 #### void initGenerateNewChunks

 Initialization method for the TerrainSection array, creates the landscape


    void initGenerateNewChunks() {



 #### GameObject createChunk

 Utilises the ObjectPool to retrieve and initialize a new chunk in its appropriate position


    GameObject createChunk(int i, int j, int x) {





 #### void resizeChunks

 Ensures square terrain


    protected virtual void resizeChunks(GenerateTerrain terrainLogic)



 #### void Update

 Unity editor event called every frame

 Calls verification method to check for Player movement


    void Update()



 #### void OnValidate

 Checks for editor value RESET in order to allow editor regeneration of terrain


    void OnValidate()



 #### void verifyChunkState

 Checks if Player's transformed chunk coordinates are within the allowed range, and if not generates chunks to follow the Player


    void verifyChunkState() {



 #### void updateChunks

 Iterates through furthest side of the chunk array destroying it, then iterates through opposite side creating new chunks


    void updateChunks(Direction dir) {



 #### void DestroyTerrainSection

 "Destroys" a chunk by returning it to the ObjectPool


    void DestroyTerrainSection(GameObject terrain)

### GenerateMazeChunks
-------



 Adaptation of the generic GenerateChunks for creating mazesrooms


    public class GenerateMazeChunks : GenerateChunks



 #### void resizeChunks

 Overriden method from GenerateChunks which interfaces with the MazeGenerator subclass


    protected override void resizeChunks(GenerateTerrain terrainLogic)

### GenerateTerrain
-------



 Class which handles the terrain generation for a specific TerrainSection within the array of terrain chunks


    public class GenerateTerrain : MonoBehaviour



 #### void initialize

 Creates the section of terrain using the prescribed algoritms


    public void initialize(ObjectPool[] foliagePools) {



 #### void getYAtLocation

 Helper method for utilizing Linecasts to determine the precise height at any x, y (x, z in world space) location


    public float getYAtLocation(Vector2 Location)



 #### void setSeed

 Mutator method for the SEED value


    public void setSeed(int inSeed) 



 #### void InitializeComp

 Sets up mesh component for terrain generation and vertextriangle insertion


    void InitializeComp() {



 #### void AddSection

 Interface method for use within GenerateChunks


    public void AddSection() {



 #### struct Side

 Data structure for passing all relevent data to represent one side of the TerrainSection


    public struct Side {



 #### void CreateShape

 Master method for creating the terrain


    void CreateShape() {



 #### void CreateSideContainers

 Initialized sub vertex arrays for holding the side vertices for easy alignment


    void CreateSideContainers() {



 #### float[] GetParams

 Gets information from array indexes


    float[] GetParams(int i, int j)



 #### void YOperator

 Virtual method to allow customization of the specifics of the terrain height choices


    protected virtual float YOperator(float inY)



 #### void CreateUVs

 Maps the texture coordinates to the newly generated terrain


    protected virtual void CreateUVs()



 #### void CreateVerts

 Creates the verticies of the terrain in a grid in x, z space and using perlin noise for the y height


    protected virtual void CreateVerts() {



 #### void CreateTris

 Creates triangles for rendering from the vertex array


    protected virtual void CreateTris() {



 #### void UpdateMesh

 Safely regenerates terrain at runtime


    void UpdateMesh() {



 #### void OnDrawGizmos

 Unity engine event which is called for debugging purposes

 Draws debugging icons on vertices 


    private void OnDrawGizmos() {

### Foliage
-------



 Empty class for future custom foliage mesh generation


    public class Foliage : MonoBehaviour

### GenerateFoliage
-------



 Utilizes ObjectPools to create dynamic and procedural foliage on a TerrainSection


    public class GenerateFoliage : MonoBehaviour



 #### void LateUpdate

 Unity editor event called at the end of every frame

 Calculates whether each foliage instance is needed to be rendered based on Player position


    void LateUpdate()



 #### void initialize

 Sets up foliage generation


    public void initialize()



 #### void setFoliagePool

 Mutator method for allowing external allocation of an ObjectPool for foliage generation


    public void setFoliagePool(ObjectPool pool)



 #### void SpawnFoliage

 Instantiates foliage instances across the owning terrain area


    void SpawnFoliage()



 #### void ClearFoliage

 Returns all foliage to its ObjectPool


    void ClearFoliage()



 #### void instantiateFoliageInstance

 Creates a foliage instance from its ObjectPool


    void instantiateFoliageInstance(float xVal, float zVal, int count)

### GenerateTree
-------



 Empty class for future custom foliage mesh generation


    public class Tree : GenerateFoliage

### BlockyTerrain
-------



 Class which adapts GenerateTerrain to provide descrete, perlin noise random terrain with sharp edges (blocky)


    public class BlockyTerrain : GenerateTerrain



 #### float YOperator

 An overriden method which sets the operation done on the terrain Y value


    protected override float YOperator(float inY)

### MazeGenerator
-------



 Class which adapts the GenerateTerrain archetype to utilize a png image in order to construct maze-like structures


    public class MazeGenerator : GenerateTerrain



 #### void setSourceTex

 Mutator for the Texture2D value


    public void setSourceTex(Texture2D inTex) 



 #### int getSourceTextWidth

 Accessor for the Texture2D width


    public int getSourceTextWidth() 



 #### int getSourceTextHeight

 Accessor for the Texture2D height


    public int getSourceTextHeight() 



 #### void CreateVerts

 Overriden method from GenerateTerrain which creates the baseplate and initializes maze generation


    protected override void CreateVerts()



 #### void CreateUVs

 Overriden method from GenerateTerrain which maps the texture coordinates to the generated terrain


    protected override void CreateUVs()



 #### void createBaseUVs

 Overriden method from GenerateTerrain which maps the base texture coordinates


    private void createBaseUVs()



 #### void createWallUVs

 Overriden method from GenerateTerrain which maps the wall texture coordinates


    private void createWallUVs()



 #### void CreateTris

 Overriden method from GenerateTerrain which maps the vertex indecies together into graphical triangles for rendering


    protected override void CreateTris()



 #### void createTrianglesFromVertexIndex

 Abstraction of triangle generation for range of vertex indecies


    private void createTrianglesFromVertexIndex(int[] tris, int j  start tri index , int k  start vert index )



 #### void GenerateMaze

 Utilizes helper method to generate a box mesh on the terrain

 in a corresponding place to each black pixel in the input image


    void GenerateMaze()



 #### void GenerateBoxOnMesh

 Helper method for GenerateMaze to create a box of specified size on the mesh


    void GenerateBoxOnMesh(Vector3 topLeft, float x, float z)

