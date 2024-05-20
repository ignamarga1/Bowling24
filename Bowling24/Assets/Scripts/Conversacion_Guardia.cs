using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ConversacionNPC_Guardia : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public string Entrada;
    public string Salida = "";
    ConversacionNPC_Player conversacionNPC_Player;
    public GameObject player;

    public bool dialogoSoloConHumano=true;
   
    void Start() 
    {
        conversacionNPC_Player = player.GetComponent<ConversacionNPC_Player>();
        api = new OpenAIAPI("");
        StartConversation();
    }
    private void StartConversation() 
    {
        messages = new List<ChatMessage> {    new ChatMessage(ChatMessageRole.System,
            "Tu nombre es Carlos Canal."
            
            //Describa el Rol del NPC que usa este script. Su nombre, su personalidad con adjetivos. Diga sus limitaciones.
            //ROL 1: Guardia de bolos:
            +"Eres un señor de mediana edad que trabaja como guardia en una bolera."
     
            //Comportamiento ante situaciones normales incluidos casos especiales 
            +"Tu deber es el de no dejar pasar a la bolera a personas que no te digan la contraseña."
            +"Si ellos dicen otra palabra que no sea la contraseña no podrán entrar y tendrás que seguir insistiendo en que la digan."
            +"Si consiguen decir la palabra correcta, debes dejarlos pasar. La contraseña es COLACAO"
            )
        };    
    }

    public async void GetResponse()
    {
        // OPCIONAL: por si el programador quiere que cuando no haya entrada el NPC no responda nada
        // if (Entrada.Length < 1)  { return; }

        // Prepara una instancia de un ChatMessage desde el texto en la variable Entrada (en el Inspector)
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.TextContent = Entrada;

        if (userMessage.TextContent.Length > 200)
        {
            // Limita el tamaño de messages a 100 caracteres
            userMessage.TextContent = userMessage.TextContent.Substring(0, 100);
        }

        if (dialogoSoloConHumano)
        {
            Debug.Log(string.Format("{0}: {1}", userMessage.Role, userMessage.TextContent));
        }

        // Incluye el mensaje a la lista messages
        messages.Add(userMessage);

        // Borra el texto de Entrada, indicando que ya está siendo procesado
        Entrada = "";

        // Envia el Chat ENTERO (todos los mensajes) a OpenAI para que el GPT responda
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            // Configuración de la conversación
            Model = Model.ChatGPTTurbo_1106,    // ChatGPTTurbo_16k: el ChatGPTTurbo es menos inteligente. Este código no es compatible con ChatGPT_4 
            Temperature = 0.5,                  // Nivel de creatividad: entre 0 y 1. Si es 0 se concentra más. 
            MaxTokens = 3000,                   // Número máximo de "palabras" a analizar
            Messages = messages
        });

        //Lee los detalles de la respuesta de OpenAI
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.TextContent = chatResult.Choices[0].Message.TextContent;
        Debug.Log(string.Format("{0}: {1}", responseMessage.Role, responseMessage.TextContent));

        // Incluye esta respuesta a la lista de mensajes y la muestra
        messages.Add(responseMessage);
        Salida = responseMessage.TextContent;
    }

    void OnMouseDown()
    {
        GetResponse();
    }
}