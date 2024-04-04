using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpawnAgents : MonoBehaviour
{
    public GameObject AgentType;
    public Vector2 WorldOrigin; //Top left of world
    public Vector2 WorldDimensions; //Width then Height
    [SerializeField] private GameObject GoalType;

    public int NumberOfAgents = 10;
    public int AgentsArrived = 0;

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
    private List<int> globalGroundingConsistency = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = SIM_TIMESCALE;
        agents.AddRange(GameObject.FindGameObjectsWithTag("Agent"));
        fileLines.Add("Iteration 1");
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

    public void RemoveGrounding(GroundingInfo grounding)
    {
        if (globalGroundings.Contains(grounding.obj))
        {
            globalGroundings.Remove(grounding.obj);
            usedIDs.Remove(grounding.ID);
            Destroy(grounding.obj);
        }
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (Time.timeScale > 1) Time.timeScale -= 1;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Time.timeScale += 1;
        }
    }

    int numAllAgentsMadeIt = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!bAwake) return;
        TimeToComplete += Time.fixedDeltaTime;

        Debug_UpdateGroundings();

        if (iCount == NUM_ITERATIONS) {CompleteSimulation(); return; }
        count += Time.fixedDeltaTime;
        IterationTimeToComplete += Time.fixedDeltaTime;
        if (count >= AGENT_SLEEP_INTERVAL || AgentsArrived == NumberOfAgents)
        {
            if (AgentsArrived == NumberOfAgents) numAllAgentsMadeIt++;
            CompleteIteration();
            //Debug.Break();
            iterationNum++;
            Debug.Log("Iteration Finished");
            for (int i = 0; i < NumberOfAgents; i++)
            {
                agents[i].GetComponent<Agent>().GetPlanner().CancelPath();
                agents[i].GetComponent<Agent>().bAwake = false;
                agents[i].GetComponent<Agent>().goalPathPoints.Clear();
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
            AgentsArrived = 0;
            iCount++;
            if (iCount != NUM_ITERATIONS)
                fileLines.Add("Iteration " + (iCount + 1));
        }
    }

    public bool CheckAgentFinished(int index)
    {
        return agents[index].GetComponent<Agent>().ArrivedAtGoal;
    }

    void Debug_UpdateGroundings()
    {
        List<Grounding> emptyGroundings = new List<Grounding>();
        globalGroundingConsistency.Clear();
        for (int i = 0; i < globalGroundings.Count; i++)
        {
            globalGroundings[i].gameObject.SetActive(false);
            globalGroundingConsistency.Add(0);
            string text = "";
            for (int j = 0; j < NumberOfAgents; j++)
            {
                bool bFoundGrounding = false;
                bool bBad = false;
                GroundingInfo DEBUG_GI = new GroundingInfo();
                for (int k = 0; k < agents[j].GetComponent<Agent>().groundings.Count; k++)
                {
                    if (agents[j].GetComponent<Agent>().groundings[k].obj == globalGroundings[i])
                    {
                        if (bFoundGrounding)
                        {
                            bBad = true;
                        }
                        else
                        {
                            if (j == agentViewingNum)
                            {
                                globalGroundings[i].gameObject.SetActive(true);
                                text = "" + agents[j].GetComponent<Agent>().groundings[k].localConsistency;
                            }
                            globalGroundingConsistency[i]++;
                            bFoundGrounding = true;
                            DEBUG_GI = agents[j].GetComponent<Agent>().groundings[k];
                        }
                    }
                }
                if (bBad) agents[j].GetComponent<Agent>().groundings.Remove(DEBUG_GI);
            }
            if (agentViewingNum == -1) text = "" + globalGroundingConsistency[i];

            if (globalGroundingConsistency[i] == 0) emptyGroundings.Add(globalGroundings[i]);

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

    List<string> fileLines = new List<string>();
    float IterationTimeToComplete = 0f;
    void CompleteIteration()
    {
        fileLines.Add("Iteration Time Taken: " + IterationTimeToComplete);
        float IterationConsistency = 0;
        for (int i = 0; i < globalGroundings.Count; i++)
        {
            IterationConsistency += (float)globalGroundingConsistency[i] / (float)NumberOfAgents;
        }
        IterationConsistency /= (float)globalGroundingConsistency.Count;
        fileLines.Add("Iteration Grounding Consistency: " + (IterationConsistency * 100) + "%");
        IterationTimeToComplete = 0;
    }

    float TimeToComplete = 0f;
    void CompleteSimulation()
    {
        fileLines.Add("Number Rounds Successful: " + numAllAgentsMadeIt);
        float IterationConsistency = 0;
        for (int i = 0; i < globalGroundings.Count; i++)
        {
            IterationConsistency += (float)globalGroundingConsistency[i] / (float)NumberOfAgents;
        }
        IterationConsistency /= (float)globalGroundingConsistency.Count;
        fileLines.Add("Final Grounding Consistency: " + (IterationConsistency * 100) + "%");
        fileLines.Add("Total Time Taken: " + (TimeToComplete) + "s");
        OutputResult();
        Debug.Break();
    }

    bool SAFETEY = false;
    void OutputResult()
    {
        if (SAFETEY) { Debug.Log("DANGER"); return; }
        string filename = "Results_" + System.DateTime.Now.ToString().Replace(" ", "").Replace(":", "").Replace(".", "") + ".txt";
        SAFETEY = true;
        if (File.Exists(filename)) return;
        var newFile = File.CreateText(filename);
        for (int i = 0; i < fileLines.Count; i++) newFile.WriteLine(fileLines[i]);
        newFile.Close();
    }
}
