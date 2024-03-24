using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GroundingInfo 
{
    public int ID;
    public Grounding obj;
    public int localConsistency;
    public GroundingInfo(Grounding obj, int ID)
    {
        this.obj = obj;
        this.ID = ID;
        localConsistency = 0;
    }
}

/**
 * Class representing the simulation of a physical agent within the digital environment
 */
[RequireComponent(typeof(BroadcastGoal))]
[RequireComponent(typeof(GroundingMethod))]
public class Agent : MonoBehaviour
{
    public float visionDist = 4.0f;
    public float collisionDist = 1.5f;
    public float movementSpeed = 1;
    public float AgentSize = 1;
    public bool ArrivedAtGoal = false;
    public SpawnAgents master;

    [SerializeField] public bool ShouldPrint = false;
    [SerializeField] private float GroundingDemonstrationCooldown = 0.5f;
    [SerializeField] private float GroundingCreationCooldown = 1;
    [SerializeField] public float GroundingUniqueDist = 3;

    public List<GroundingInfo> groundings;
    private DynamicCoordinateGrid mapping;
    private PathPlanner planner;
    private Vector2 initLocation; //TESTING
    private Vector3 currLocation;
    public Vector3 movementDirection = Vector3.zero;
    private List<QuadTreeNode> visitedNodes = new List<QuadTreeNode>();
    private int visitedCount = 0;
    public bool bAwake = false;

    QuadTree tree;

    public DynamicCoordinateGrid GetMapping() { return mapping; }
    public PathPlanner GetPlanner() { return planner; }

    /**
     * #### void Init()
     * Initializes a 3x3 re-allocatable grid
     */
    public void Init()
    {
        planner = new PathPlanner(this);
        groundings = new List<GroundingInfo>();
        mapping = GetComponent<DynamicCoordinateGrid>();
        mapping.Origin = GetComponent<Collider>().bounds.center;
        mapping.Init(this);
        initLocation = new Vector2(transform.position.x, transform.position.z);
        currLocation = transform.position;
        bAwake = true;

        Bounds bounds = GetComponent<Collider>().bounds;
        float radius = Vector3.Distance(bounds.center + bounds.extents, bounds.center);
        tolerance = radius;
        collisionDist = radius*2.1f;

        Debug.Log("TEST");
    }

    public void ResetGroundingCreationCooldown()
    {
        groundingCreationCount = 0;
    }

    public GroundingInfo AddGrounding(Grounding newGrounding, int localConsistency = 1)
    {
        int id = master.GetUniqueID();
        GroundingInfo info = new GroundingInfo(newGrounding, id);
        info.localConsistency = localConsistency;
        if (ShouldPrint) Debug.Log("Recieved a grounding: Name: " + id + " Consistency: " + info.localConsistency);
        groundings.Add(info);
        groundingCreationCount = 0;
        return info;
    }

    public GroundingInfo AddGrounding(GroundingInfo grounding)
    {
        if (grounding.obj == null) Debug.Log("NULL1");
        if (ShouldPrint) Debug.Log("Recieved a grounding: Name: " + grounding.ID + " Consistency: " + grounding.localConsistency);
        groundings.Add(grounding);
        return grounding;
    }

    public bool CanGround()
    {
        if (groundingCreationCount >= GroundingDemonstrationCooldown) return true;
        return false;
    }

    public bool RequestRecieveDemonstration()
    {
        if (groundingDemonstrationCount >= GroundingDemonstrationCooldown) return true;
        return false;
    }

    public GroundingInfo RecieveDemonstration(GroundingInfo input)
    {
        //Debug.Log("Recieving a demonstration");
        GroundingInfo finalVersion = new GroundingInfo(null, 0);
        bool bFound = false;
        for (int i = 0; i < groundings.Count; i++)
        {
            GroundingInfo info = groundings[i];
            if (Vector2.Distance(mapping.toVector2(info.obj.transform.position), mapping.toVector2(input.obj.transform.position)) <= GroundingUniqueDist) //Has a version of the same grounding
            {
                //Debug.Log("I know this grounding");
                if (info.ID == input.ID && info.localConsistency == input.localConsistency)
                {
                    finalVersion = new GroundingInfo(null, 0); //Same name and everything, nothing needed to be done
                    //Debug.Log("We know the same grounding, all good");
                }
                else if (info.localConsistency > input.localConsistency) //Local version is better than the shared version
                {
                    info.localConsistency++;
                    finalVersion = info;
                    //Debug.Log("I know a better grounding or simply have shared mine more");
                }
                else // Input is a better version of the grounding
                {
                    //Debug.Log("Damn yours is way better high key");
                    if (info.obj == null || input.obj == null) Debug.Log("NULL2");
                    if (ShouldPrint) Debug.Log("Updated a given grounding: Name: " + input.ID + " Consistency: " + input.localConsistency);
                    info.ID = input.ID;
                    info.obj = input.obj;
                    info.localConsistency = input.localConsistency;
                    finalVersion = input;
                }
                bFound = true;
                break;
            }
        }
        if (!bFound) //Doesn't know anything about this grounding
        {
            //Debug.Log("Never seen this grounding before in my entire life gawddamn");
            AddGrounding(input);
            finalVersion = input;
            if (ShouldPrint) Debug.Log("Recieved a grounding: Name: " + finalVersion.ID + " Consistency: " + finalVersion.localConsistency);
        }
        groundingDemonstrationCount = 0;
        //Debug.Log("\n\n\n\n\n");
        return finalVersion;
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
        if (ShouldPrint) Debug.DrawLine(new Vector3((int)x, 5, (int)z), new Vector3((int)x, -5, (int)z), Color.yellow, 0.05f);
        if (Physics.Raycast(new Vector3((int)x, 100, (int)z), transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            return hit.point.y;
        }
        return -1;
    }

    float count = 0; //Continuous timer
    int progress = 0; //Progress in index units through current path
    float updateInterval = 0.05f;
    float groundingDemonstrationCount = 10;
    float groundingCreationCount = 10;
    float tolerance = 0.05f; //Acceptable vector equivalence value
    /**
     * #### void Update()
     * Unity event running every frame
     * Implements wandering functionality (mainly for debugging)
     */
    void FixedUpdate()
    {
        if (!bAwake) return; //Simply wait for simulation initialization
        count += Time.fixedDeltaTime; //Time counter used for various methods within Update
        groundingDemonstrationCount += Time.fixedDeltaTime;
        groundingCreationCount += Time.fixedDeltaTime;
        //Goal Reference
        GameObject goal = GameObject.FindGameObjectWithTag("Goal");

        if (Vector3.Distance(transform.position, goal.transform.position) <= visionDist && Vector3.Distance(transform.position, goal.transform.position) > collisionDist)
        {
            //If the goal is detected in local vision, head there
            //Debug.Log("FOUND GOAL AT DISTANCE: " + Vector3.Distance(transform.position, goal.transform.position));
            planner.CancelPath();
            movementDirection = (goal.transform.position - transform.position).normalized;
        }
        else if (Vector3.Distance(transform.position, goal.transform.position) <= collisionDist)
        {
            Vector3 origPos = transform.position;
            mapping.Move(new Vector2((int)origPos.x - 1, (int)origPos.z - 1), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x) + 2, (int)origPos.z - 1), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x) + 2, ((int)origPos.z) + 2), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x - 1), ((int)origPos.z) + 2), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(origPos.x, origPos.z), this, true, planner, false, ShouldPrint);

            ArrivedAtGoal = true;
            bAwake = false;
            mapping.Move(new Vector3(0, 4, 0), this, true, planner, false, false);
            //BROADCAST
            GetComponent<BroadcastGoal>().Broadcast();
        }
        else if (groundingCreationCount > GroundingCreationCooldown && GetComponent<GroundingMethod>().ExecuteGrounding(this))
        {
            //If the criteria for generating a grounding are met, create a new grounding
        }
        else if (count > updateInterval && !planner.OnPath)
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

        if (groundingDemonstrationCount >= GroundingDemonstrationCooldown)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, visionDist, Vector3.down, 0);
            if (hits.Length > 1)
            {
                GameObject otherAgent = null;
                GroundingInfo knownGrounding = new GroundingInfo(null, 0);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.gameObject.GetComponent<Agent>() && hits[i].collider.gameObject != gameObject)
                    {
                        otherAgent = hits[i].collider.gameObject;
                        break;
                    }
                }
                for (int i = 0; i < groundings.Count; i++)
                {
                    if (Vector2.Distance(mapping.toVector2(transform.position), mapping.toVector2(groundings[i].obj.gameObject.transform.position)) <= visionDist)
                    {
                        knownGrounding = groundings[i];
                    }
                }
                if (otherAgent != null && knownGrounding.obj != null)
                {
                    Agent other = otherAgent.GetComponent<Agent>();
                    if (other.RequestRecieveDemonstration())
                    {
                        ResetGroundingCreationCooldown();
                        other.ResetGroundingCreationCooldown();
                        //Debug.Log("DEMONSTRATING AND ACCEPTED");
                        knownGrounding.localConsistency++;
                        GroundingInfo otherResponse = other.RecieveDemonstration(knownGrounding);
                        if (otherResponse.ID != knownGrounding.ID && otherResponse.obj != null)
                        {
                            knownGrounding.ID = otherResponse.ID;
                            knownGrounding.obj = otherResponse.obj;
                            knownGrounding.localConsistency = otherResponse.localConsistency;
                            //if (knownGrounding.obj == null) Debug.Log("NULL8");
                            if (ShouldPrint) Debug.Log("Updated a responded grounding: Name: " + knownGrounding.ID + " Consistency: " + knownGrounding.localConsistency);
                        }
                        //Release both to return to their tasks
                        groundingDemonstrationCount = 0;
                    }
                }
            }
        }

        //Execute calculated movement based on above 2D calculations, converting to relevent 3D space
        //Vector3 test1 = transform.position;
        mapping.Move(mapping.toVector2(gameObject.transform.position + (movementDirection.normalized * Time.fixedDeltaTime * movementSpeed)), this, false, planner, true, ShouldPrint);
        //if (planner.bCollisionReset && transform.position != test1) Debug.Log("COLLISION BUT WE STILL MOVED");

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
        updateInterval = 0.05f;
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

        if (tree == null || mapping.bQuadTreeNeedsRegen)
        {
            tree = new QuadTree();
            tree.Construct(mapping, mapping.Origin, 1, ShouldPrint);
            mapping.bQuadTreeNeedsRegen = false;
        }

        List<QuadTreeNode> nodes;
        if (!planner.bCollisionReset || planner.collisionDir == Vector3.zero) nodes = tree.GetFurthestFreeNodes(new Vector2(transform.position.x, transform.position.z));
        else
        {
            nodes = tree.GetFurthestFreeNodesInDir(new Vector2(transform.position.x, transform.position.z), new Vector2(planner.collisionDir.x, planner.collisionDir.z));
            //temp = -planner.collisionDir;
            //Debug.DrawLine(transform.position, transform.position + temp, Color.magenta, 20);
        }
        QuadTreeNode node;

        updateInterval = 0.15f;

        if (nodes == null || nodes.Count == 0 || visitedCount >= nodes.Count)
        {
            //if (ShouldPrint) Debug.Log("Uh Oh, Seems we don't got any nodes to visit");
            node = null;
            visitedNodes.Clear();
            visitedCount = 0;
        }
        else
        {
            //Debug.Log("NUM NODES: " + nodes.Count);
            while (visitedNodes.Contains(nodes[visitedCount]) || nodes[visitedCount] == tree.GetNode(new Vector2(transform.position.x, transform.position.z)))
            {
                visitedCount++;
                if (visitedCount == nodes.Count)
                {
                    visitedNodes.Clear();
                    visitedCount = 0;
                    break;
                }
            }
            node = nodes[visitedCount];
            visitedNodes.Add(node);
            visitedCount++;
        }

        if (node == null || planner.bCollisionReset)
        {
            Vector3 origPos = transform.position;
            mapping.Move(new Vector2((int)origPos.x - 1, (int)origPos.z - 1), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x) + 2, (int)origPos.z - 1), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x) + 2, ((int)origPos.z) + 2), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x - 1), ((int)origPos.z) + 2), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(origPos.x, origPos.z), this, true, planner, false, ShouldPrint);
            /*Bounds bounds = GetComponent<Collider>().bounds;
            Vector3 temp2 = origPos + (temp * Mathf.Clamp(Time.deltaTime * movementSpeed, 0, Vector3.Distance(bounds.center, bounds.center + bounds.extents)/2));
            mapping.Move(new Vector2(temp2.x, temp2.z), this, false, planner, true, ShouldPrint);*/
            movementDirection = temp.normalized;

            planner.bCollisionReset = false;
        }
        else
        {

            planner.bCollisionReset = false;
            Vector2 init = new Vector2(transform.position.x, transform.position.z);
            /*dx = max(centerX - rectLeft, rectRight - centerX);
            dy = max(centerY - rectTop, rectBottom - centerY);*/
            float num = 0.01f;
            Vector2 bl = new Vector2(node.x + num, node.y + num);
            Vector2 br = new Vector2(node.x + node.w - num, node.y + num);
            Vector2 tl = new Vector2(node.x + num, node.y + node.h - num);
            Vector2 tr = new Vector2(node.x + node.w - num, node.y + node.h - num);

            Vector2 destination = new Vector2();

            float blf = Vector2.Distance(bl, init);
            float brf = Vector2.Distance(br, init);
            float tlf = Vector2.Distance(tl, init);
            float trf = Vector2.Distance(tr, init);

            float currMax = 0;
            if (blf > currMax)
            {
                currMax = blf;
                destination = bl;
            }
            if (brf > currMax)
            {
                currMax = brf;
                destination = br;
            }
            if (tlf > currMax)
            {
                currMax = tlf;
                destination = tl;
            }
            if (trf > currMax)
            {
                destination = tr;
            }

            if (tree == null || mapping.bQuadTreeNeedsRegen)
            {
                tree = new QuadTree();
                tree.Construct(mapping, mapping.Origin, 1, ShouldPrint);
                mapping.bQuadTreeNeedsRegen = false;
            }

            Vector2 dir = (destination - init);
            float dist = dir.magnitude;
            dir.Normalize();
         
            planner.Move(tree, init, destination, mapping, dist / movementSpeed, ShouldPrint);
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
            Vector3 origPos = transform.position;
            mapping.Move(new Vector2((int)origPos.x - 1, (int)origPos.z - 1), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x) + 2, (int)origPos.z - 1), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x) + 2, ((int)origPos.z) + 2), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(((int)origPos.x - 1), ((int)origPos.z) + 2), this, true, planner, false, ShouldPrint);
            mapping.Move(new Vector2(origPos.x, origPos.z), this, true, planner, false, ShouldPrint);

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
