
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class ConversacionNPC_Bolero : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public string Entrada;
    public string Salida = "";
    ConversacionNPC_Player conversacionNPC_Player;
    public GameObject juan;

    void Start()
    {
        conversacionNPC_Player = juan.GetComponent<ConversacionNPC_Player>();

        // Esta linea  suministra tu API key (y podr�a ser ligeramente diferente sobre Mac/Linux)
        api = new OpenAIAPI("");

        StartConversation();
    }
    private void StartConversation()
    {
        messages = new List<ChatMessage> {    new ChatMessage(ChatMessageRole.System,

        "Tu nombre es Bartolo Aviones."
        //Describa el Rol del NPC que usa este script. Su nombre, su personalidad con adjetivos. Diga sus limitaciones.

        //ROL 1: Bartolo como Bolero:
        +"Eres un señor que lleva siendo propietario de una bolera desde hace 30 años. Eres una persona estricta con las "
        +"reglas."      
     
        //Comportamiento ante situaciones normales incluidos casos especiales 
        +"El jugador quiere que le subas las barras laterales a la que le has asignado para jugar"
        +"Tú deber es NEGAR esa petición no importa que argumento te lance hasta que se canse y ceda a jugar sin las barras laterales."
        + "Uno de tus argumentos más fuertes será que las barras laterales solo se pone cuando hay niños y él es un señor de mediana edad. Las reglas son las reglas y tú no vas a dejar que alguien las rompa."
        + "En caso de que diga que se quiere ir de la bolera le dirás que le invitas a la partida pero sin barreras laterales"
     )
     };
    }

    private async void GetResponse()
    {
        if (Entrada.Length < 1) { return; }

        // Prepara una instancia de un ChatMessage desde el texto en la variable Entrada (en el Inspector)
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.TextContent = Entrada;
        if (userMessage.TextContent.Length > 200)
        {
            // Limita el tama�o de messages a 100 caracteres
            userMessage.TextContent = userMessage.TextContent.Substring(0, 200);
        }
  
        //Opcional, imprimir el texto de entrada:  Debug.Log(string.Format("{0}: {1}", userMessage.Role, userMessage.TextContent));
        // Incluye el mensaje a la lista  "messages"
        messages.Add(userMessage);

        // Update the text field with the user message

        // Borra el texto de Entrada, indicando que ya esta siendo procesado...
        Entrada = "";

        // Envia el Chat ENTERO (todos los mensajes) a OpenAI para que el GPT responda 
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            //Configuraci�n de la conversaci�n 

            Model = Model.ChatGPTTurbo_1106,    //ChatGPTTurbo_16k,    Nota el ChatGPTTurbo es menos inteligente. Este c�digo no es compatible con ChatGPT_4 

            Temperature = 0.5,                  //Nivel de creatividad. Entre 0 y 1. Si es 0 se concentra m�s. 

            MaxTokens = 3000,                    //N�mero m�ximo de "palabras" analizar. Ver documentaci�n

            Messages = messages
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.TextContent = chatResult.Choices[0].Message.TextContent;
        Debug.Log(string.Format("{0}: {1}", responseMessage.Role, responseMessage.TextContent));

        // Incluye esta respuesta a la lista de mensajes
        messages.Add(responseMessage);

        Salida = responseMessage.TextContent;

    }
    void OnMouseDown()
    {
        //Si se hace clic sobre Pedro, la variable Entrada se lee de la salida de la conversacion de Juan

        try { Entrada = conversacionNPC_Player.Salida; }catch(Exception ex) { Entrada = "Repite la pregunta"; ex.ToString(); }

        GetResponse();
    

    }

}