using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ComportamientoJugador : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject[] pathPositions;
    public int actualObjective;

    // Start is called before the first frame update
    void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.SetDestination(pathPositions[0].transform.position);   // The first path position the agent has to go is the 0
        }
    }

    // Update is called once per frame
    void Update()
    {
        // When the agent reaches the position
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            actualObjective++;

            // Sets the next position of the path
            if (actualObjective >= pathPositions.Length)
            {
                actualObjective = 0;
            }
            agent.destination = pathPositions[actualObjective].transform.position;
        }
    }
}
