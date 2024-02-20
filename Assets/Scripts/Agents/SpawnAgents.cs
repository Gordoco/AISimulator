using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAgents : MonoBehaviour
{
    public GameObject AgentType;
    public Vector2 WorldOrigin; //Top left of world
    public Vector2 WorldDimensions; //Width then Height

    public int NumberOfAgents = 10;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < NumberOfAgents; i++)
        {
            Vector3 locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 0, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));

            while (!CheckValidLoc(locationToSpawn))
            {
                locationToSpawn = new Vector3((int)Random.Range(WorldOrigin.x, WorldOrigin.x + WorldDimensions.x), 0, (int)Random.Range(WorldOrigin.y, WorldOrigin.y + WorldDimensions.y));
            }
            GameObject agent = Instantiate(AgentType, locationToSpawn, Quaternion.identity);
            agent.GetComponent<Agent>().Init();
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
