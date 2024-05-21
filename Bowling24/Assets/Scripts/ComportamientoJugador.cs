using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderData;

public class ComportamientoJugador : MonoBehaviour
{
    public NavMeshAgent agente;
    public GameObject[] caminoPosiciones;
    public int posObjetivoActual;

    public GameObject camara, NPC_Player, NPC_Bolero, NPC_Guardia;
    private float rangoDeteccion = 8f;

    enum Estado { ANDAR, DIALOGAR, JUGAR }
    Estado estado;
    private Boolean detectadoGuardia = false, detectadoBolero = false;

    // Start is called before the first frame update
    void Start()
    {
        if (agente == null)
        {
            estado = Estado.ANDAR;
            agente = GetComponent<NavMeshAgent>();
            agente.SetDestination(caminoPosiciones[0].transform.position);   // The first path position the agent has to go is the 0
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
        // Cuando el agente llega a la posición
        if (agente.remainingDistance <= agente.stoppingDistance)
        {
            // Si es la última, pasa a jugar a los bolos
            if (posObjetivoActual == caminoPosiciones.Length - 1)
            {
                agente.speed = 0f;      // Se detiene
                estado = Estado.JUGAR;  // Pasa al estado JUGAR
            }

            posObjetivoActual++;

            // Actualiza la posObjetivoActual
            if (posObjetivoActual >= caminoPosiciones.Length)
            {
                posObjetivoActual = 0;
            }
            agente.destination = caminoPosiciones[posObjetivoActual].transform.position;
        }

        // Detecta NPC_Guardia
        RaycastHit hit;
        if (!detectadoGuardia && Physics.Raycast(transform.position, NPC_Guardia.transform.position - transform.position, out hit, rangoDeteccion))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                print("Detectado NPC_Guardia con RayCast");
                detectadoGuardia = true;    
                agente.speed = 0f;          // Se detiene
                estado = Estado.DIALOGAR;   // Pasa al estado DIALOGAR
            }
        }
    }

    private void Dialogar()
    {
        // Una vez que pulsamos la tecla espacio, pasa de DIALOGAR a ANDAR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            agente.speed = 5f;      // Retoma su velocidad
            estado = Estado.ANDAR;  // Pasa al estado ANDAR
        }
    }

    private void Jugar()
    {
        // Movimiento de la cámara
        Vector3 nuevaPosicion = new Vector3(20f, 10f, -20f);
        camara.transform.position = Vector3.Lerp(camara.transform.position, nuevaPosicion, Time.deltaTime); 

        // Rotación de la cámara
        Quaternion nuevaRotacion = Quaternion.Euler(20f, -50f, 0f); 
        camara.transform.rotation = Quaternion.Lerp(camara.transform.rotation, nuevaRotacion, Time.deltaTime * 0.5f); 

        NPC_Player.GetComponent<AprendizajeBolos>().enabled = true; // Activa el script de aprendizaje
    }
}
