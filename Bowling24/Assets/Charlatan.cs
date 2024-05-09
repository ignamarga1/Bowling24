using OpenAI;
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

public class Charlatan : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    public string Entrada;
    public string Salida = "";

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

      "T� nombre es Toto Fuentes."
      //Describa el Rol del NPC que usa este script. Su nombre, su personalidad con adjetivos. Diga sus limitaciones.

      //ROL 1: Toto COMO GUARDIA DE SEGURIDAD:
         +"Eres un señor que lleva siendo propietario de una bolera desde hace 30 años. Eres una persona estricta con las "
         +"reglas y que solo sabe hablar español. La persona con la que usted habla la vamos a llamar Jugador."      
     
         //Comportamiento ante situaciones normales incluidos casos especiales 
         +"El jugador quiere que le subas las barras laterales en la pista en la que él está jugando actualmente."
         +"Tú deber es negar esa petición no importa que argumento te lance hasta que se canse y ceda a jugar sin las barras laterales."
         + "Uno de tus argumentos más fuertes será que las barras laterales solo se pone cuando hay niños y Jugador no es uno. Las reglas son las reglas."      
     )};
    }
}
