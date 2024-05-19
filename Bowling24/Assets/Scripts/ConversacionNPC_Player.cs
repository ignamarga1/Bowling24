
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

public class ConversacionNPC_Player : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public string Entrada;
    public string Salida = "";
    ConversacionNPC_Bolero conversacionNPC_Bolero;
    public GameObject pedro;

    //Poner true si Juan dialoga solamente con el usuario humano. 
    //Si esta variable es false, Juan estar�a dialogando con uno o varios NPCs (por ejemplo, cuando se usa el Rol 3 abajo)
    public bool dialogoSoloConHumano=true;
   
    void Start() 
    {
        conversacionNPC_Bolero = pedro.GetComponent<ConversacionNPC_Bolero>();

        // Esta linea suministra tu API key (y podr�a ser ligeramente diferente sobre Mac/Linux
        api = new OpenAIAPI("");
        StartConversation();
    }
    private void StartConversation() 
    {
        messages = new List<ChatMessage> {    new ChatMessage(ChatMessageRole.System,
            "Tu nombre es Tony Montañas."
            
            //Describa el Rol del NPC que usa este script. Su nombre, su personalidad con adjetivos. Diga sus limitaciones.
            //ROL 1: Jugador de bolos:
             +"Eres un señor de mediana edad que quiere aprender a jugar a los bolos pero sabes que se te da muy mal y todas las bolas se van hacia los lados. Por eso, quieres que"
             +"el señor de la bolera te subas las barras laterales de tu pista."
     
             //Comportamiento ante situaciones normales incluidos casos especiales 
             +"Tendrás que preguntarle si puede subir las barreras de la pista de los bolos."
             +"Al recibir respuestas negativas, tendrás que insistir usando argumentos distintos."
             +"Al final, te cansarás de escuchar como te dice que no y te irás a jugar a tu pista sin las barras laterales subidas o te podrás ir de la bolera." 
             +"Dependiendo como de enfadado estés y cuanto hayas luchado porque las suba."
     )};    
    }

    public async void GetResponse()
    {
        //Opcional por si el programador quiere que cuando no haya entrada el NPC no responda nada:
        //if (Entrada.Length < 1)  { return; }
 
        // Prepara una instancia de un ChatMessage desde el texto en la variable Entrada (en el Inspector)
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.TextContent = Entrada;
        if (userMessage.TextContent.Length > 200)
        {
            // Limita los mensages a 120 caracteres
            userMessage.TextContent = userMessage.TextContent.Substring(0, 200);
        }
         if (dialogoSoloConHumano)  Debug.Log(string.Format("{0}: {1}", userMessage.Role, userMessage.TextContent));

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

        //Lee los detalles de la respuesta de OpenAI
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
        //Si se hace clic sobre Juan, la variable Entrada       (Entrada puede ser introducida por el Jugador en el Inspector o, si se descomenta, se lee de la salida de la conversacion de Pedro

        if (!dialogoSoloConHumano)
        {   //Si el dialogo es con otros NPCs,  Juan debe leer lo que dicen los otros NPCs. 
            //Por ejemplo, aqui aparece lee lo que dice Pedro...
            try { Entrada = conversacionNPC_Bolero.Salida; } catch (Exception ex) { Entrada = "Repite la pregunta"; ex.ToString(); }
        }


        GetResponse();

    }

}