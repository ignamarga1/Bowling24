using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Completions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChatPlayer : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public GameObject NPCBolera;

    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAIAPI("");
        IniciarConversacion();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void IniciarConversacion()
    {
        messages = new List<ChatMessage> {    new ChatMessage(ChatMessageRole.System,

        "Tu nombre es Vallecillo."
        //Describa el Rol del NPC que usa este script. Su nombre, su personalidad con adjetivos. Diga sus limitaciones.

        //ROL 1: Toto COMO BOLERO:
             +"Eres un señor que quiere jugar a los bolos pero sabes que se te da muy mal y todas las bolas se van hacia los lados. Por eso, quieres que"
             +"el señor de la bolera te subas las barras laterales de tu pista"
     
             //Comportamiento ante situaciones normales incluidos casos especiales 
             +"Tendrás que preguntarle si pueda subirlas."
             +"Al recibir respuestas negativas, tendrás que insistir usando argumentos distintos."
             +"Al final, te cansarás de escuchar como te dice que no y te irás a jugar a tu pista sin las barras laterales subidas o te podrás ir de la bolera. Dependiendo como de enfadado estés y cuanto hayas luchado por que las suba."
        )};

        string mensajeInicial = "Tu rol es el de un jugador que está en una bolera y quieres que el bolero (el otro NPC con quien hablas) te suba las barras laterales porque no eres capaz de tirar ni un bolo sin ellas. Todas tus respuestas deberán ser hacia el señor de la bolera, en modo de petición";

        // Agregar el mensaje inicial a la lista de mensajes
        messages.Add(new ChatMessage(ChatMessageRole.System, mensajeInicial));

        // Enviar el mensaje inicial al NPCJugador para iniciar la conversación
        ResponderPregunta(mensajeInicial);
    }

    public async void ResponderPregunta(string mensaje)
    {
        // Enviar el mensaje a la API de OpenAI para generar una respuesta
        var response = await api.Completions.CreateCompletionAsync(new CompletionRequest
        {
            Model = "babbage-002",
            MaxTokens = 200,
            Temperature = 0.2,
            Prompt = mensaje
        });

        // Imprimir la respuesta generada por OpenAI
        UnityEngine.Debug.Log("NPCJugador: " + response.Completions[0].Text.Trim());

        // Reenviar la respuesta al NPCBolera
        NPCBolera.GetComponent<Charlatan>().ResponderPregunta(response.Completions[0].Text.Trim());
    }
}
