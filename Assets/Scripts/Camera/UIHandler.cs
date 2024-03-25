using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text ViewingText;
    [SerializeField] private TMP_Text IterationNumText;
    [SerializeField] private GameObject AgentSpawnerObj;

    private SpawnAgents AgentSpawningScript;
    private bool bAwake = false;

    // Start is called before the first frame update
    void Start()
    {
        if (ViewingText == null || IterationNumText == null || AgentSpawnerObj == null)
        {
            Debug.Log("ERROR: Unassigned Values in Player UIHandler");
            Debug.Break();
            return;
        }

        AgentSpawningScript = AgentSpawnerObj.GetComponent<SpawnAgents>();

        bAwake = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!bAwake) return;

        IterationNumText.text = "Iteration Num: " + AgentSpawningScript.iterationNum;
        if (AgentSpawningScript.agentViewingNum >= 0)
        {
            ViewingText.text = "Currently Viewing: Agent " + (AgentSpawningScript.agentViewingNum + 1);
            if (AgentSpawningScript.CheckAgentFinished(AgentSpawningScript.agentViewingNum))
            {
                ViewingText.text += " | Found Goal";
            }
        }
        else ViewingText.text = "Currently Viewing: Global Groundings";
    }
}
