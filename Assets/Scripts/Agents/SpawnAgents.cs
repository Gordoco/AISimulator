using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgents : MonoBehaviour
{
    public GameObject AgentType;
    public Vector2 WorldOrigin; //Top left of world
    public Vector2 WorldDimensions; //Width then Height

    public int NumberOfAgents = 10;

    private List<GameObject> agents = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        InitAgents(true);
    }

    public void InitAgents(bool bStart = false)
    {
        for (int i = 0; i < NumberOfAgents; i++)
        {
            Vector3 locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 2.5f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));

            while (!CheckValidLoc(locationToSpawn))
            {
                locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 2.5f, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));
            }
            if (bStart)
            {
                GameObject agent = Instantiate(AgentType, locationToSpawn, Quaternion.identity);
                agents.Add(agent);
            }
        }
    }

    private bool CheckValidLoc(Vector3 location)
    {
        RaycastHit hit;
        int layerMask = 1 << 8;
        if (!Physics.Raycast(location + Vector3.up * 500, Vector3.down, out hit, 500, layerMask) &&
            !Physics.Raycast((location + Vector3.forward * 0.5f) + Vector3.up * 500, Vector3.down, out hit, 500, layerMask) &&
            !Physics.Raycast((location + Vector3.right * 0.5f) + Vector3.up * 500, Vector3.down, out hit, 500, layerMask) &&
            !Physics.Raycast((location + Vector3.back * 0.5f) + Vector3.up * 500, Vector3.down, out hit, 500, layerMask) &&
            !Physics.Raycast((location + Vector3.left * 0.5f) + Vector3.up * 500, Vector3.down, out hit, 500, layerMask))
        {
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
