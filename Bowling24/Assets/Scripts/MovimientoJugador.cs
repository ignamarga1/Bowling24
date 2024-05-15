using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float speed = 20f; // Velocidad de movimiento del jugador

    void Update()
    {
        // Obtener la entrada del usuario en los ejes horizontal y vertical
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Crear un vector de movimiento basado en la entrada del usuario
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Aplicar el movimiento multiplicado por la velocidad y el tiempo delta para hacerlo independiente del framerate
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
