using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgents : MonoBehaviour
{
    public GameObject AgentType;
    public Vector2 WorldOrigin; //Top left of world
    public Vector2 WorldDimensions; //Width then Height
    [SerializeField] private GameObject GoalType;

    public int NumberOfAgents = 10;

    bool bAwake = false;

    [SerializeField] private GameObject GroundingType;
    [SerializeField] private float AGENT_SLEEP_INTERVAL = 20.0f;
    [SerializeField] private int NUM_ITERATIONS = 200;
    [SerializeField] private float SIM_TIMESCALE = 2.0f;

    public int iterationNum = 1;
    public int agentViewingNum = 0;
    private List<GameObject> agents = new List<GameObject>();
    private GameObject Goal;
    private List<Grounding> globalGroundings = new List<Grounding>();

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = SIM_TIMESCALE;
        agents.AddRange(GameObject.FindGameObjectsWithTag("Agent"));
        int temp = agents.Count;
        InitAgents(true);
        NumberOfAgents += temp;
        bAwake = true;

        Vector3 locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 4f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));

        while (!CheckValidLoc(locationToSpawn))
        {
            locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 4f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));
        }
        Goal = Instantiate(GoalType, locationToSpawn, Quaternion.identity);
    }

    public Grounding CreateGrounding(Vector3 position)
    {
        position.y = 0;
        Grounding newGrounding = Instantiate(GroundingType, position, Quaternion.identity).GetComponent<Grounding>();
        globalGroundings.Add(newGrounding);
        return newGrounding;
    }

    List<int> usedIDs = new List<int>();
    public int GetUniqueID()
    {
        int ID = Random.Range(0, 999999);
        while (usedIDs.Contains(ID))
        {
            ID = Random.Range(0, 999999);
            usedIDs.Add(ID);
        }
        return ID;
    }

    public void InitAgents(bool bStart = false)
    {
        for (int i = 0; i < NumberOfAgents; i++)
        {
            Vector3 locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 4f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));

            while (!CheckValidLoc(locationToSpawn))
            {
                locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 4f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));
            }
            if (bStart)
            {
                //Debug.DrawLine(locationToSpawn + (Vector3.up * 10), locationToSpawn - (Vector3.up * 10), Color.cyan, 50);
                GameObject agent = Instantiate(AgentType, locationToSpawn, Quaternion.identity);
                agent.GetComponent<Agent>().Init();
                agent.GetComponent<Agent>().master = this;
                if (i == 0)
                {
                    agent.GetComponent<Agent>().ShouldPrint = true;
                }
                agents.Add(agent);
            }
        }
    }

    private bool CheckValidLoc(Vector3 location)
    {
        RaycastHit hit;
        Bounds bounds = AgentType.GetComponent<Collider>().bounds;
        float radius = Vector3.Distance(bounds.center, bounds.center + bounds.extents);
        Vector3 highLocation = new Vector3(location.x, 50, location.z);
        return !Physics.SphereCast(highLocation, radius*5, Vector3.down, out hit, 50 - location.y);
    }

    float count = 0;
    int iCount = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (agentViewingNum > 0)
            {
                agents[agentViewingNum].GetComponent<Agent>().ShouldPrint = false;
                agentViewingNum--;
                agents[agentViewingNum].GetComponent<Agent>().ShouldPrint = true;
            }
            else
            {
                agentViewingNum = -1;
                agents[0].GetComponent<Agent>().ShouldPrint = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (agentViewingNum < NumberOfAgents - 1 && agentViewingNum >= 0)
            {
                agents[agentViewingNum].GetComponent<Agent>().ShouldPrint = false;
                agentViewingNum++;
                agents[agentViewingNum].GetComponent<Agent>().ShouldPrint = true;
            }
            else if (agentViewingNum == -1)
            {
                agentViewingNum = 0;
                agents[agentViewingNum].GetComponent<Agent>().ShouldPrint = true;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!bAwake) return;

        Debug_UpdateGroundings();

        if (iCount == NUM_ITERATIONS - 1) CompleteSimulation();
        count += Time.fixedDeltaTime;
        if (count >= AGENT_SLEEP_INTERVAL)
        {
            Debug.Break();
            iterationNum++;
            Debug.Log("Iteration Finished");
            for (int i = 0; i < NumberOfAgents; i++)
            {
                agents[i].GetComponent<Agent>().GetPlanner().CancelPath();
                agents[i].GetComponent<Agent>().bAwake = false;
                agents[i].transform.position = new Vector3(agents[i].transform.position.x, 9999, agents[i].transform.position.z);
            }

            Vector2 goalLocationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));
            while (!CheckValidLoc(new Vector3(goalLocationToSpawn.x, 4, goalLocationToSpawn.y)))
            {
                goalLocationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));
            }
            Goal.transform.position = new Vector3(goalLocationToSpawn.x, 4, goalLocationToSpawn.y);

            for (int i = 0; i < NumberOfAgents; i++)
            {
                Vector2 locationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));

                while (!CheckValidLoc(new Vector3(locationToSpawn.x, 4, locationToSpawn.y)))
                {
                    locationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));
                }
                agents[i].transform.position = new Vector3(agents[i].transform.position.x, 4, agents[i].transform.position.z);
                Agent agent = agents[i].GetComponent<Agent>();
                agent.GetMapping().Move(locationToSpawn, agent, true, agent.GetPlanner(), false);
                agent.bAwake = true;
                agent.ArrivedAtGoal = false;
            }
            count = 0;
            iCount++;
        }
    }

    public bool CheckAgentFinished(int index)
    {
        return agents[index].GetComponent<Agent>().ArrivedAtGoal;
    }

    void Debug_UpdateGroundings()
    {
        List<Grounding> emptyGroundings = new List<Grounding>();
        for (int i = 0; i < globalGroundings.Count; i++)
        {
            globalGroundings[i].gameObject.SetActive(false);
            int count = 0;
            string text = "";
            for (int j = 0; j < NumberOfAgents; j++)
            {
                bool bFoundGrounding = false;
                for (int k = 0; k < agents[j].GetComponent<Agent>().groundings.Count; k++)
                {
                    if (agents[j].GetComponent<Agent>().groundings[k].obj == globalGroundings[i])
                    {
                        if (bFoundGrounding)
                        {
                            Debug.Log("THIS IS BAD: MULTIPLE OF THE SAME GROUNDING IN 1 AGENT");
                            for (int w = 0; w < agents[j].GetComponent<Agent>().groundings.Count; w++) Debug.Log("GROUNDING NAME: " + agents[j].GetComponent<Agent>().groundings[w].ID);
                        }
                        if (j == agentViewingNum)
                        {
                            globalGroundings[i].gameObject.SetActive(true);
                            text = "" + agents[j].GetComponent<Agent>().groundings[k].localConsistency;
                        }
                        count++;
                        bFoundGrounding = true;
                    }
                }
            }
            if (agentViewingNum == -1) text = "" + count;

            if (count == 0) emptyGroundings.Add(globalGroundings[i]);

            globalGroundings[i].GetComponentInChildren<TextMesh>().text = text;
            if (agentViewingNum == -1) globalGroundings[i].gameObject.SetActive(true);
        }
        if (emptyGroundings.Count > 0)
        {
            foreach (Grounding empty in emptyGroundings)
            {
                Debug.Log("GROUNDING REMOVED/REPLACED");
                globalGroundings.Remove(empty);
                Destroy(empty.gameObject);
            }
        }
    }

    void CompleteSimulation()
    {
        Debug.Break();
    }
}
