using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgents : MonoBehaviour
{
    public GameObject AgentType;
    public Vector2 WorldOrigin; //Top left of world
    public Vector2 WorldDimensions; //Width then Height

    public int NumberOfAgents = 10;

    [SerializeField] private float AGENT_SLEEP_INTERVAL = 20.0f;

    private List<GameObject> agents = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        agents.AddRange(GameObject.FindGameObjectsWithTag("Agent"));
        int temp = agents.Count;
        InitAgents(true);
        NumberOfAgents += temp;
    }

    public void InitAgents(bool bStart = false)
    {
        for (int i = 0; i < NumberOfAgents; i++)
        {
            Vector3 locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 3f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));

            while (!CheckValidLoc(locationToSpawn))
            {
                locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 3f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));
            }
            if (bStart)
            {
                GameObject agent = Instantiate(AgentType, locationToSpawn, Quaternion.identity);
                agent.GetComponent<Agent>().Init();
                agents.Add(agent);
            }
        }
    }

    private bool CheckValidLoc(Vector3 location)
    {
        RaycastHit hit;
        Bounds bounds = agents[0].GetComponent<Collider>().bounds;
        float radius = Vector2.Distance(new Vector2(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y), new Vector2(bounds.center.x, bounds.center.y));
        Vector3 highLocation = new Vector3(location.x, 50, location.z);
        return !Physics.SphereCast(highLocation, radius, Vector3.down, out hit, 50 - location.y);
    }

    float count = 0;
    // Update is called once per frame
    void Update()
    {
        count += Time.deltaTime;
        if (count >= AGENT_SLEEP_INTERVAL)
        {
            Debug.Log("PORTED");
            for (int i = 0; i < NumberOfAgents; i++)
            {
                Vector2 locationToSpawn = new Vector2((int)Mathf.Round(Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x)), (int)Mathf.Round(Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y)));

                while (!CheckValidLoc(locationToSpawn))
                {
                    locationToSpawn = new Vector2((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));
                }
                Agent agent = agents[i].GetComponent<Agent>();
                agent.GetMapping().Move(locationToSpawn, agent, true, agent.GetPlanner(), false);
            }
            count = 0;
        }
    }
}
