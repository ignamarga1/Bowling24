//Programación de Videojuegos, Universidad de Málaga (Prof. M. Nuñez, mnunez@uma.es)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using weka.classifiers.trees;
using weka.classifiers.evaluation;
using weka.core;
using java.io;
using java.lang;
using java.util;
using weka.classifiers.functions;
using weka.classifiers;
using weka.core.converters;

public class Aprendiz_2_incognitas : MonoBehaviour
{
    private string ESTADO = "Sin conocimiento";
    public GameObject bola, bolosObjetivo;
    Rigidbody rb;
    Text texto;
    
    GameObject instanciaBola, puntoObjetivo;
    float distanciaObjetivo, mejorFuerzaX;
    public float valorMaximoFx = 10, pasoFx;
    float Fy_calculada, valorMaximoFy = 18;                             // Es un ejemplo: Se asume que este valor es extremo para ese problema
    
    weka.classifiers.trees.M5P saberPredecirFuerzaX;
    weka.core.Instances casosEntrenamiento;

    // Calcula una "Fy válida" usando algún método simple
    float calculoFyMetodoSimple(float valorMaximoFy)                        
    {
        float minimoValor = 1f;
        float valorFactible = (minimoValor + valorMaximoFy) / 2f;       // Por ejemplo, la fuerza media entre el mínimo y el máximo.
        
        return valorFactible;
    }

    void Start()
    {
        Fy_calculada = calculoFyMetodoSimple(valorMaximoFy);                // Se va a aprender Fx, hay que seleccionar Fy factible
        texto = Canvas.FindObjectOfType<Text>();

        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");  // Lanza el proceso de entrenamiento                                          
    }

    IEnumerator Entrenamiento()
    {
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Aprendizaje Datos/Experiencias.arff"));  // Lee fichero con las variables y experiencias

        texto.text = "ENTRENAMIENTO: crea una tabla con las Fx utilizadas y distancias alcanzadas (Fy calculada = " + Fy_calculada.ToString("0.00") + " N)";
        print("DATOS DE ENTRADA: Fy = " + Fy_calculada + " Fx variables de 1 a " + valorMaximoFx + " " + ((valorMaximoFx == 0 || Fy_calculada == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
        
        if (casosEntrenamiento.numInstances() < 10)
        {

            // BUCLE de planificación de la fuerza FX durante el entrenamiento
            for (float Fx = 1; Fx <= valorMaximoFx; Fx = Fx + pasoFx)               
            {
                instanciaBola = Instantiate(bola) as GameObject;
                Rigidbody rb = instanciaBola.GetComponent<Rigidbody>();                 // Crea una bola con físicas
                rb.AddForce(new Vector3(Fx, Fy_calculada, 0), ForceMode.Impulse);       // y la lanza con esa fuerza Fx (Fy se escoge en el Start())
                yield return new WaitUntil(() => (rb.transform.position.y < 0));        // y espera a que la bola llegue al suelo

                Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                print("con fuerzas: Fy_fijo = " + Fy_calculada + " y Fx = " + Fx + " se alcanzó una distancia de " + rb.transform.position.x);
                casoAaprender.setDataset(casosEntrenamiento);                           // Crea un registro de experiencia
                casoAaprender.setValue(0, Fx);                                          // Guarda el dato de la fuerza utilizada
                casoAaprender.setValue(1, rb.transform.position.x);                     // Anota la distancia alcanzada
                
                casosEntrenamiento.add(casoAaprender);                                  // Guarda el registro de experiencia 
                rb.isKinematic = true; rb.GetComponent<Collider>().isTrigger = true;    // Opcional: paraliza la bola
                Destroy(instanciaBola, 1f);                                           // Opcional: destruye la bola en 1 seg para que ver donde cayó.            
            }                                                                           // FIN bucle de lanzamientos con diferentes de fuerzas
        }

        //APRENDIZAJE CONOCIMIENTO
        saberPredecirFuerzaX = new M5P();                           // Crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(0);                        // La variable a aprender será la fuerza Fx (id=0) dada la distancia
        saberPredecirFuerzaX.buildClassifier(casosEntrenamiento);   // REALIZA EL APRENDIZAJE DE FX A PARTIR DE LAS EXPERIENCIAS

        File salida = new File("Assets/Finales_Experiencias.arff");
        if (!salida.exists())
        {
            System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
        }
        ArffSaver saver = new ArffSaver();
        saver.setInstances(casosEntrenamiento);
        saver.setFile(salida);
        saver.writeBatch();

        //EVALUACIÓN DEL CONOCIMIENTO APRENDIDO
        print("intancias = " + casosEntrenamiento.numInstances());
        if (casosEntrenamiento.numInstances() >= 10)
        {
            Evaluation evaluador = new Evaluation(casosEntrenamiento);                   // Opcional: si tiene más de 10 ejemplos, estima la posible precisión
            evaluador.crossValidateModel(saberPredecirFuerzaX, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " N");
        }

        distanciaObjetivo = bolosObjetivo.transform.position.x;     // Distancia de los bolos 
        ESTADO = "Con conocimiento";

    }

    void FixedUpdate()                                                                    
    {
        // Aplica conocimiento aprendido para lanzar a los bolos
        if ((ESTADO == "Con conocimiento") && (distanciaObjetivo > 0))
        {
            Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());     // Crea un registro de experiencia durante el juego
            casoPrueba.setDataset(casosEntrenamiento);
            casoPrueba.setValue(1, distanciaObjetivo);                                  // Pone el dato de la distancia a alcanzar

            mejorFuerzaX = (float)saberPredecirFuerzaX.classifyInstance(casoPrueba);    // Predice la fuerza dada la distancia utilizando el algoritmo M5P
            print("Durante el juego, se observó Y = " + distanciaObjetivo + ". El NPC calcula la fuerza X = " + mejorFuerzaX);

            instanciaBola = Instantiate(bola) as GameObject;                            // Utiliza la bola física del juego (si no existe la crea)
            rb = instanciaBola.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(mejorFuerzaX, Fy_calculada, 0), ForceMode.Impulse); // Y por fin la lanza en el videojuego con la fuerza encontrada
            print("Se lanzó una bola con fuerzas: Fy_fijo = " + Fy_calculada + " y Fx = " + mejorFuerzaX);
            ESTADO = "Acción realizada";
        }

        if (ESTADO == "Acción realizada")
        {
            texto.text = "Para unos bolos a " + distanciaObjetivo.ToString("0.000") + " m, la fuerza Fx a utilizar será de " + mejorFuerzaX.ToString("0.000") + "N  (Fy calculada = " + Fy_calculada.ToString("0.00") + " N)";
            // Cuando la bola cae por debajo de 0 m
            if (rb.transform.position.y < 0)    
            {
                // Escribe la distancia en x alcanzada
                print("Los bolos están a una distancia de " + distanciaObjetivo + " m");
                print("La bola lanzada llegó a " + rb.transform.position.x + ". El error fue de " + (rb.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m");
                rb.isKinematic = true;
                ESTADO = "FIN";
            }
        }
    }
}
