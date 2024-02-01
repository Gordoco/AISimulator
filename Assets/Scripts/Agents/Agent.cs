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

    /**
     * #### void Start()
     * Unity event which runs at initialization.
     * Initializes a 3x3 re-allocatable grid
     */
    void Start()
    {
        planner = new PathPlanner();

        mapping = GetComponent<DynamicCoordinateGrid>();
        mapping.Origin = transform.position;
        mapping.Init(this);
        initLocation = new Vector2(transform.position.x, transform.position.z);
        currLocation = transform.position;

        Debug.Log("TEST");
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

    float count = 0;
    int progress = 0;
    float tolerance = 0.05f;
    /**
     * #### void Update()
     * Unity event running every frame
     * Implements wandering functionality (mainly for debugging)
     */
    void Update()
    {
        count += Time.deltaTime;
        if (count > 1 && !planner.OnPath)
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
            /*if (nodes.Count != 0) node = nodes[nodes.Count - 1].node;
            else node = null;*/

            if (nodes == null || nodes.Count == 0 || visitedCount >= nodes.Count)
            {
                Debug.Log("Uh Oh, Seems we don't got any nodes to visit");
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
        else if (planner.OnPath)
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
            }
            else {
                //Add scripted move based on path plan
                movementDirection = new Vector3(planner.currentPath[progress].x, 0, planner.currentPath[progress].y) - new Vector3(transform.position.x, transform.position.y, transform.position.z);
                movementDirection.Normalize();
            }
        }
        else
        {
            movementDirection = Vector3.zero;
        }

        //gameObject.transform.position += (movementDirection * Time.deltaTime * movementSpeed);
        mapping.Move(mapping.toVector2(gameObject.transform.position + (movementDirection * Time.deltaTime * movementSpeed)), this);

        if (Input.GetKeyDown(KeyCode.G))
        {
            Vector2 loc = new Vector2(300, 351);
            mapping.Move(loc, this, true, planner); //Teleport
            Debug.Log("Welcome to your new destination at: " + loc);
        }

        currLocation = transform.position;
    }
}
