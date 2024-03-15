using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class representing the simulation of a physical agent within the digital environment
 */
public class Agent : MonoBehaviour
{
    public float movementSpeed = 1;
    public float AgentSize = 1;

    private DynamicCoordinateGrid mapping;
    private PathPlanner planner;
    private Vector2 initLocation; //TESTING
    private Vector3 currLocation;
    private Vector3 movementDirection = Vector3.zero;
    private List<QuadTreeNode> visitedNodes = new List<QuadTreeNode>();
    private int visitedCount = 0;
    private bool bAwake = false;

    /**
     * #### void Init()
     * Initializes a 3x3 re-allocatable grid
     */
    public void Init()
    {
        planner = new PathPlanner();

        mapping = GetComponent<DynamicCoordinateGrid>();
        mapping.Origin = transform.position;
        mapping.Init(this);
        initLocation = new Vector2(transform.position.x, transform.position.z);
        currLocation = transform.position;
        bAwake = true;

        Bounds bounds = GetComponent<Collider>().bounds;
        float radius = Vector2.Distance(new Vector2(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y), new Vector2(bounds.center.x, bounds.center.y));
        tolerance = radius;

        Debug.Log("TEST");
    }

    /**
     * #### void OnCollisionEnter(Collision)
     * Unity method for evaluating collisions.
     * Used to error check collision prevention systems.
     */
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("ERROR: Agent collided with another object. Agent Name: " + gameObject.name + " Other Obj Name: " + collision.gameObject.name);
    }

    /**
     * #### int[][] ScanLocalArea()
     * Checks the points in a 3x3 grid around the agent and sends that information to the local grid
     */
    public int[][] ScanArea(int[] center)
    {
        int[][] localMap = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            localMap[i] = new int[3];
        }
        for (int i = 1; i >= -1; i--)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (ScanDownAtXZ(j + center[0], -i + center[1]) == 0)
                {
                    localMap[i + 1][j + 1] = (int)MappingIDs.Free;
                }
                else
                {
                    localMap[i + 1][j + 1] = (int)MappingIDs.Full;
                }
            }
        }
        return localMap;
    }

    /**
     * #### float ScanDownAtXY(float, float)
     * Utilizes Raycasts to check the height of terrain at a given point, expecting flat ground for traversal
     */
    float ScanDownAtXZ(float x, float z)
    {
        RaycastHit hit;
        int layerMask = 1 << 8;
        //Debug.DrawLine(new Vector3(x, 5, z), new Vector3(x, -5, z), Color.red, 0.2f);
        if (Physics.Raycast(new Vector3(x, 100, z), transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            return hit.point.y;
        }
        return -1;
    }

    float count = 0; //Continuous timer
    int progress = 0; //Progress in index units through current path
    float tolerance = 0.05f; //Acceptable vector equivalence value
    /**
     * #### void Update()
     * Unity event running every frame
     * Implements wandering functionality (mainly for debugging)
     */
    void Update()
    {
        if (!bAwake) return; //Simply wait for simulation initialization
        count += Time.deltaTime; //Time counter used for various methods within Update

        int updateInterval = 1; //Number of seconds to check for new pathing changes
        if (count > updateInterval && !planner.OnPath)
        {
            //Utilizes desired wandering algorithm to efficiently explore domain
            //------------------------------------------------------------------
            WanderAlgorithm();
            //------------------------------------------------------------------
        }
        //If calculated to be on a path, execute next unit-time-step movement
        else if (planner.OnPath)
        {
            //----------------------------------------------------------
            PathExecution();
            //----------------------------------------------------------
        }
        else
        {
            //If no relevent movement is available stop movement
            movementDirection = Vector3.zero;
        }

        //Directly Pre-Move, utilize reactive collision prevention to ensure no collision
        //ReactiveCollision();

        //Execute calculated movement based on above 2D calculations, converting to relevent 3D space
        mapping.Move(mapping.toVector2(gameObject.transform.position + (movementDirection * Time.deltaTime * movementSpeed)), this);

        //Teleportation test implementation for debugging wander algorithm
        if (Input.GetKeyDown(KeyCode.G))
        {

            Vector2 loc = new Vector2(300, 351);
            mapping.Move(loc, this, true, planner); //Teleport
            Debug.Log("Welcome to your new destination at: " + loc);
        }

        //Save previous frames location for use calculating position deltas
        currLocation = transform.position;
    }

    public void Teleported()
    {
        progress = 0;
        movementDirection = Vector3.zero;
        count = 1;
        visitedNodes.Clear();
        visitedCount = 0;
    }

    /**
     * #### Wander Algorithm
     * Currently in a test implementation format, should implement an efficent domain-independant exploration algorithm
     */
    private void WanderAlgorithm()
    {
        Vector3 temp = Vector3.zero;
        var rand = Random.Range(0, 4);
        switch (rand)
        {
            case 0:
                temp = new Vector3(0, 0, 1f);
                break;
            case 1:
                temp = new Vector3(0, 0, -1f);
                break;
            case 2:
                temp = new Vector3(-1f, 0, 0);
                break;
            case 3:
                temp = new Vector3(1f, 0, 0);
                break;
        }

        count = 0;
        QuadTree tree = new QuadTree();
        tree.Construct(mapping, mapping.Origin, 0);
        List<NodeDepth> nodes = tree.GetFurthestFreeNodes(new Vector2(transform.position.x, transform.position.z));
        QuadTreeNode node;

        if (nodes == null || nodes.Count == 0 || visitedCount >= nodes.Count)
        {
            //Debug.Log("Uh Oh, Seems we don't got any nodes to visit");
            node = null;
            visitedNodes.Clear();
            visitedCount = 0;
        }
        else
        {
            while (visitedNodes.Contains(nodes[visitedCount].node) || nodes[visitedCount].node == tree.GetNode(new Vector2(transform.position.x, transform.position.z)))
            {
                visitedCount++;
                if (visitedCount == nodes.Count)
                {
                    visitedNodes.Clear();
                    visitedCount = 0;
                    break;
                }
            }
            node = nodes[visitedCount].node;
            visitedNodes.Add(node);
            visitedCount++;
        }

        if (node == null)
        {
            mapping.Move(new Vector2(transform.position.x, transform.position.z), this, true);
            planner.Move(new Vector2(transform.position.x, transform.position.z), new Vector2(transform.position.x + temp.x, transform.position.z + temp.z), mapping, 0.2f);
        }
        else
        {
            Vector2 init = new Vector2(transform.position.x, transform.position.z);
            Vector2 destination = new Vector2(node.x + AgentSize / 2, node.y + AgentSize / 2);
            float dist = (destination - init).magnitude;
            planner.Move(init, destination, mapping, dist / movementSpeed);
        }
    }

    /**
     * #### PathExecution()
     * Utilizes calculated path to plan motion direction for the next timestep
     */
    private void PathExecution()
    {
        if (progress >= planner.currentPath.Count)
        {
            progress = 0;
            planner.OnPath = false;
            planner.currentPath = null;
            movementDirection = Vector3.zero;
            count = 1;
        }
        else if (transform.position.x < planner.currentPath[progress].x + tolerance && transform.position.x > planner.currentPath[progress].x - tolerance &&
            transform.position.z < planner.currentPath[progress].y + tolerance && transform.position.z > planner.currentPath[progress].y - tolerance)
        {
            progress++;
            movementDirection = Vector3.zero;
        }
        else
        {
            //Add scripted move based on path plan
            movementDirection = new Vector3(planner.currentPath[progress].x, 0, planner.currentPath[progress].y) - new Vector3(transform.position.x, 0, transform.position.z);
            movementDirection.Normalize();
            transform.rotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        }
    }
    
}
