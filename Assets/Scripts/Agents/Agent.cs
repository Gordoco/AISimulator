using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * #### Agent
 * -----
 * Class representing the simulation of a physical agent within the digital environment
 */
public class Agent : MonoBehaviour
{
    public float movementSpeed = 1;

    private DynamicCoordinateGrid mapping;
    private PathPlanner planner;
    private Vector2 initLocation; //TESTING
    private Vector3 currLocation;
    private Vector3 movementDirection = Vector3.zero;

    /**
     * #### void Start()
     * Unity event which runs at initialization.
     * Initializes a 3x3 re-allocatable grid
     */
    void Start()
    {
        mapping = GetComponent<DynamicCoordinateGrid>();
        mapping.Origin = transform.position;
        mapping.Move(Vector2.zero, ScanLocalArea(), 1/movementSpeed);

        planner = new PathPlanner();

        initLocation = new Vector2(transform.position.x, transform.position.z);
        currLocation = transform.position;
    }

    /**
     * #### int[][] ScanLocalArea()
     * Checks the points in a 3x3 grid around the agent and sends that information to the local grid
     */
    int[][] ScanLocalArea()
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
                if (ScanDownAtXZ(j + (int)gameObject.transform.position.x, -i + (int)gameObject.transform.position.z) == 0)
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
            var rand = Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    movementDirection = new Vector3(0, 0, 1);
                    break;
                case 1:
                    movementDirection = new Vector3(0, 0, -1);
                    break;
                case 2:
                    movementDirection = new Vector3(-1, 0, 0);
                    break;
                case 3:
                    movementDirection = new Vector3(1, 0, 0);
                    break;
            }
            
            count = 0;
        }
        else if (planner.OnPath)
        {
            if (progress >= planner.currentPath.Count)
            {
                progress = 0;
                planner.OnPath = false;
                planner.currentPath = null;
                movementDirection = Vector3.zero;
                count = -5;
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

        gameObject.transform.position += (movementDirection * Time.deltaTime * movementSpeed);

        if ((int)transform.position.x != (int)currLocation.x || (int)transform.position.z != (int)currLocation.z)
        {
            mapping.Move(new Vector2((int)transform.position.x - (int)currLocation.x, (int)transform.position.z - (int)currLocation.z), ScanLocalArea(), 1/movementSpeed);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            // TEST PATHFINDING //
            if (!planner.OnPath)
            {
                QuadTree test = new QuadTree();
                test.Construct(mapping, mapping.Origin);
                planner.Move(new Vector2(transform.position.x, transform.position.z), initLocation, mapping);
            }

            // TEST NEIGHBORS //
            /*QuadTree test = new QuadTree();
            test.Construct(mapping, mapping.Origin);
            QuadTreeNode newNode = test.GetNode(new Vector2(transform.position.x, transform.position.z));
            if (newNode == null) Debug.Log("NOT IN A QUADNODE");
            List<QuadTreeNode> neighbors = newNode.GetDirections();
            if (neighbors != null)
                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (neighbors[i] != null)
                    {
                        neighbors[i].colorOverride = Color.magenta;
                        neighbors[i].Print();
                    }
                }
            newNode.colorOverride = Color.black;
            newNode.Print();*/

            // TEST NEIGHBOR MAIN EDGE CASE //
            //Write a unit test for the case where the depths must re-allocate
        }

        currLocation = transform.position;
    }
}
