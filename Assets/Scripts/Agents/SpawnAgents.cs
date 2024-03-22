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

    [SerializeField] private float AGENT_SLEEP_INTERVAL = 20.0f;
    [SerializeField] private int NUM_ITERATIONS = 200;
    [SerializeField] private float SIM_TIMESCALE = 2.0f;

    private List<GameObject> agents = new List<GameObject>();
    private GameObject Goal;

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
        return !Physics.SphereCast(highLocation, radius*3, Vector3.down, out hit, 50 - location.y);
    }

    float count = 0;
    int iCount = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!bAwake) return;
        if (iCount == NUM_ITERATIONS - 1) CompleteSimulation();
        count += Time.fixedDeltaTime;
        if (count >= AGENT_SLEEP_INTERVAL)
        {
            Debug.Break();
            Debug.Log("PORTED");
            for (int i = 0; i < NumberOfAgents; i++)
            {
                agents[i].GetComponent<Agent>().GetPlanner().CancelPath();
                agents[i].GetComponent<Agent>().bAwake = false;
                agents[i].transform.position = new Vector3(0, 4, 0);
            }
            for (int i = 0; i < NumberOfAgents; i++)
            {
                Vector2 locationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));

                while (!CheckValidLoc(new Vector3(locationToSpawn.x, 4, locationToSpawn.y)))
                {
                    locationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));
                }
                Agent agent = agents[i].GetComponent<Agent>();
                agent.GetMapping().Move(locationToSpawn, agent, true, agent.GetPlanner(), false);
                agent.bAwake = true;
            }
            count = 0;
            iCount++;
        }
    }

    void CompleteSimulation()
    {
        Debug.Break();
    }
}
