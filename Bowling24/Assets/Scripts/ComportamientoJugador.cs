using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ComportamientoJugador : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject[] pathPositions;
    public int actualObjective;

    public GameObject NPC_Bolero, NPC_Guardia;
    private float rangoDeteccion = 10f;

    enum Estado { ANDAR, DIALOGAR, JUGAR}
    Estado estado;

    // Start is called before the first frame update
    void Start()
    {
        if (agent == null)
        {
            estado = Estado.ANDAR;
            agent = GetComponent<NavMeshAgent>();
            agent.SetDestination(pathPositions[0].transform.position);   // The first path position the agent has to go is the 0
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(estado)
        {
            case Estado.ANDAR:
                Andar();
                break;

            case Estado.DIALOGAR:
                Dialogar();
                break;

            case Estado.JUGAR:
                Jugar();
                break;
        }
    }

    private void Andar()
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

        // Detectar NPC_Guardia
        RaycastHit hit;
        if (Physics.Raycast(transform.position, NPC_Guardia.transform.position - transform.position, out hit, rangoDeteccion))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                agent.speed = 0f;                                   // Se detiene
                transform.LookAt(NPC_Guardia.transform.position);   // Le mira
                estado = Estado.DIALOGAR;                           // Cambio estado a dialogar
            }
        }
    }

    private void Dialogar()
    {

    }

    private void Jugar()
    {

    }
}
