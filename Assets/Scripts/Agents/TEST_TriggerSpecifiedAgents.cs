using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Test Script for allowing hand placed agents to be included in simulations
 */
public class TEST_TriggerSpecifiedAgents : MonoBehaviour
{
    [SerializeField] private GameObject[] AGENTS;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < AGENTS.Length; i++)
        {
            if (AGENTS[i].GetComponent<Agent>()) AGENTS[i].GetComponent<Agent>().Init();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
