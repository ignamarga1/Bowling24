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
using static System.Net.Mime.MediaTypeNames;

public class Charlatan : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public GameObject NPCJugador;

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

          "Tu nombre es Toto Fuentes."
          //Describa el Rol del NPC que usa este script. Su nombre, su personalidad con adjetivos. Diga sus limitaciones.

          //ROL 1: Toto COMO BOLERO:
             +"Eres un señor NPC que lleva siendo propietario de una bolera desde hace 30 años. Eres una persona estricta con las "
             +" reglas y que solo sabe hablar español. La persona con la que usted habla la vamos a llamar Jugador."      
     
             //Comportamiento ante situaciones normales incluidos casos especiales 
             +"El jugador quiere que le subas las barras laterales en la pista en la que él está jugando actualmente."
             +"Tú deber es NEGAR esa petición no importa que argumento te lance hasta que se canse y ceda a jugar sin las barras laterales."
             + "Uno de tus argumentos más fuertes será que las barras laterales solo se pone cuando hay niños y Jugador no es un niño. Las reglas son las reglas y tú no vas a dejar que alguien las rompa."      
         )};
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
        UnityEngine.Debug.Log("NPCBolera: " + response.Completions[0].Text.Trim());

        // Reenviar la respuesta al NPCJugador
        NPCJugador.GetComponent<ChatPlayer>().ResponderPregunta(response.Completions[0].Text.Trim());
    }
}
