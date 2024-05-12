using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
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

public class ChatJugador : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public GameObject NPCBolera;

    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAIAPI("aqui va el secreto (api key)");
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

      //ROL 1: Toto COMO GUARDIA DE SEGURIDAD:
         +"Eres un señor que quiere jugar a los bolos pero sabes que se te da muy mal y todas las bolas se van hacia los lados. Por eso, quieres que"
         +"el señor de la bolera te subas las barras laterales de tu pista"
     
         //Comportamiento ante situaciones normales incluidos casos especiales 
         +"Tendrás que preguntarle si pueda subirlas."
         +"Al recibir respuestas negativas, tendrás que insistir usando argumentos distintos."
         +"Al final, te cansarás de escuchar como te dice que no y te irás a jugar a tu pista sin las barras laterales subidas o te podrás ir de la bolera. Dependiendo como de enfadado estés y cuanto hayas luchado por que las suba."
     )};
    }

    public async void ResponderPregunta(string mensaje)
    {
        // Enviar el mensaje a la API de OpenAI para generar una respuesta
        //var request = new CompletionRequest
        //{
        //    Model = "text-davinci-003", // Este es el modelo de lenguaje a utilizar, "davinci" es uno de los modelos disponibles
        //    MaxTokens = 50,
        //    Temperature = 0.7,
        //    Prompt = mensaje
        //};

        //var response = await api.CreateCompletion(request);

        // Imprimir la respuesta generada por OpenAI
        //UnityEngine.Debug.Log("NPCBolera: " + response.choices[0].text.Trim());

        //NPCBolera.ResponderPregunta(mensaje);
    }
}
