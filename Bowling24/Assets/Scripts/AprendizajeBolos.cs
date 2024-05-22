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

public class AprendizajeBolos : MonoBehaviour
{
    private string ESTADO = "Sin conocimiento";
    GameObject instanciaBola, puntoObjetivo;
    public GameObject bola, bolosObjetivo, bolosCopia;
    Rigidbody rb;
    Text texto;

    float alturaInicialBola = 3f;     // Altura inicial de la bola
    float distanciaInicialBola = 2f;  // Distancia entre la bola y el personaje
    float tiempo;

    public float valorMaximoFx, valorMaximoFy, incrementoFuerza, velocidadSimulacion = 1;
    float mejorFuerzaX, mejorFuerzaY, distanciaObjetivo;
    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirFuerzaX;
    weka.core.Instances casosEntrenamiento;

    void Start()
    {
        Time.timeScale = velocidadSimulacion;                               // Controla la velocidad de la simulación (afecta a todo el juego)
        texto = Canvas.FindObjectOfType<Text>();                            // Encuentra el texto en pantalla del canvas
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");  // Lanza la corutina de Entrenamiento
        tiempo = Time.time;                                                 // Contador para controlar el tiempo de los lanzamientos de la bola
    }

    IEnumerator Entrenamiento()
    {
        // Uso de una tabla vacía
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Aprendizaje Datos/Iniciales_Experiencias.arff")); // Lee fichero con variables. Sin instancias

        // Uso de una tabla con los datos del último entrenamiento
        // casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias.arff"));
        
        if (casosEntrenamiento.numInstances() < 10)
        {
            texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            print("Datos de entrada: valorMaximoFx = " + valorMaximoFx + " valorMaximoFy = " + valorMaximoFy + " " + ((valorMaximoFx == 0 || valorMaximoFy == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for (float Fx = 1; Fx <= valorMaximoFx; Fx += incrementoFuerza)     // Bucle de planificación de la fuerza Fx durante el entrenamiento
            {
                for (float Fy = 1; Fy <= valorMaximoFy; Fy += incrementoFuerza) // Bucle de planificación de la fuerza Fy durante el entrenamiento
                {
                    instanciaBola = Instantiate(bola) as GameObject;
                    instanciaBola.transform.position = new Vector3(transform.position.x, alturaInicialBola, transform.position.z + distanciaInicialBola); // Posición inicial en frente del personaje
                    Rigidbody rb = instanciaBola.GetComponent<Rigidbody>();     // Coge el rigidBody de la bola
                    rb.AddForce(new Vector3(0, Fy, Fx), ForceMode.Impulse);     // y la lanza con las fuerzas Fx y Fy

                    float startTime = Time.time;                                // Empieza a contar cada vez que se lanza una bola
                    yield return new WaitUntil(() => ((rb.transform.position.y <= 1.9) && (Time.time > startTime + 3)) || (rb.transform.position.z > bolosObjetivo.transform.position.z)); // Condiciones fin corutina
                    tiempo = Time.time;                                         // Actualiza el contador

                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    print("ENTRENAMIENTO: con fuerza Fx = " + Fx + " y Fy = " + Fy + " se alcanzó una distancia de " + rb.transform.position.z + "m");
                    casoAaprender.setDataset(casosEntrenamiento);                        // Crea un registro de experiencia
                    casoAaprender.setValue(0, Fx);                                       // Guarda los datos de las fuerzas Fx y Fy utilizadas
                    casoAaprender.setValue(1, Fy);
                    casoAaprender.setValue(2, rb.transform.position.z);                  // Anota la distancia alcanzada
                    casosEntrenamiento.add(casoAaprender);                               // Guarda el registro en la lista casosEntrenamiento

                    rb.isKinematic = true; rb.GetComponent<Collider>().isTrigger = true; // OPCIONAL: detiene la pelota
                    Destroy(instanciaBola, 1);                                           // OPCIONAL: destruye la pelota
                }                                                                        // FIN bucle de lanzamientos con diferentes de fuerzas
            }

            Destroy(bolosObjetivo); // Destruimos los bolos objetivos para poner unos nuevos en su posición inicial

            // Crea y guarda las experiencias finales en un archivo
            File salida = new File("Assets/Aprendizaje Datos/Finales_Experiencias.arff");
            if (!salida.exists())
            {
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            }
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        // APRENDIZAJE CONOCIMIENTO 
        saberPredecirFuerzaX = new M5P();                           // Crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(0);                        // y hace que aprenda Fx dada la distancia y Fy
        saberPredecirFuerzaX.buildClassifier(casosEntrenamiento);   // REALIZA EL APRENDIZAJE DE Fx A PARTIR DE LA DISTANCIA Y Fy

        saberPredecirDistancia = new M5P();                         // Crea otro algoritmo de aprendizaje M5P (árboles de regresión)  
        casosEntrenamiento.setClassIndex(2);                        // y hace que aprenda a calcular la distancia dada Fx y Fy                                                                                         
        saberPredecirDistancia.buildClassifier(casosEntrenamiento); // este algoritmo aprende un "modelo físico aproximado"

        ESTADO = "Con conocimiento";

        print(casosEntrenamiento.numInstances() +" espera "+ saberPredecirFuerzaX.toString());

        // EVALUACIÓN DEL CONOCIMIENTO APRENDIDO 
        if (casosEntrenamiento.numInstances() >= 10){
            casosEntrenamiento.setClassIndex(0);
            Evaluation evaluador = new Evaluation(casosEntrenamiento);  // OPCIONAL: si tiene más de 10 ejemplos, estima la posible precisión
            evaluador.crossValidateModel(saberPredecirFuerzaX, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Fx durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + "N");
            
            casosEntrenamiento.setClassIndex(2);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + "m");
        }

        // PRUEBA: estimación de la distancia a los bolos
        distanciaObjetivo = bolosObjetivo.transform.position.z + 5;    

        // 2ª opción: generación aleatoria de una distancia dependiendo de sus límites:        
        //AttributeStats estadisticasDistancia = casosEntrenamiento.attributeStats(2);        // OPCIONAL: inicializa las estadisticas de las distancias
        //float maximaDistanciaAlcanzada = (float) estadisticasDistancia.numericStats.max;    // OPCIONAL: obtiene el valor máximo de las distancias alcanzadas
        //distanciaObjetivo = UnityEngine.Random.Range(maximaDistanciaAlcanzada * 0.2f, maximaDistanciaAlcanzada * 0.8f);  // OPCIONAL: cálculo aleatorio de la distancia 

        // SITÚA EL OBJETIVO DE LOS BOLOS 
        puntoObjetivo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        puntoObjetivo.transform.position = new Vector3(0, 1, distanciaObjetivo);
        puntoObjetivo.transform.localScale = new Vector3(1.1f, 1, 1.1f);
        puntoObjetivo.GetComponent<Collider>().isTrigger = true;

        Destroy(bolosObjetivo, 1);
    }

    // DURANTE DEL JUEGO: aplica lo aprendido para lanzar a los bolos
    void FixedUpdate()                                                                                 
    {
        if ((ESTADO == "Con conocimiento") && (distanciaObjetivo > 0))
        {
            Time.timeScale = 1;                                                                                  
            float menorDistancia = 1e9f;
            print("-- OBJETIVO: LANZAR LA BOLA A UNA DISTANCIA DE " + distanciaObjetivo + "m.");

            // Con dos bucles Fx y Fy con "modelo físico aproximado" obtenemos complejidad n^2
            // Reducimos la complejidad con un solo bucle FOR de esta forma:
            for (float Fy = 1; Fy < valorMaximoFy; Fy += incrementoFuerza)                  // Bucle FOR con fuerza Fy, deduce Fx = f(Fy, distancia) y escoge mejor combinación         
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(1, Fy);                                                     // Crea un registro con una Fy
                casoPrueba.setValue(2, distanciaObjetivo);                                      // y la distancia
                float Fx = (float)saberPredecirFuerzaX.classifyInstance(casoPrueba);            // Predice Fx a partir de la distancia y una Fy 
                
                if ((Fx >= 1) && (Fx <= valorMaximoFx))
                {
                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);                                                  // Utiliza el "modelo físico aproximado" con Fx y Fy                 
                    casoPrueba2.setValue(0, Fx);                                                                 // Crea una registro con una Fx
                    casoPrueba2.setValue(1, Fy);                                                                 // Crea una registro con una Fy
                    float prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPrueba2);     // Predice la distancia dada por Fx y Fy
                    if (Mathf.Abs(prediccionDistancia - distanciaObjetivo) < menorDistancia)                     // Busca la Fy con una distancia más cercana al objetivo
                    {
                        menorDistancia = Mathf.Abs(prediccionDistancia - distanciaObjetivo);                     // Si encuentra una buena toma nota de esta distancia
                        mejorFuerzaX = Fx;                                                                       // y de la fuerzas Fx y Fy que uso
                        mejorFuerzaY = Fy;                                                                       
                        print("RAZONAMIENTO: Una posible acción es ejercer una fuerza Fx = " + mejorFuerzaX + " y Fy = " + mejorFuerzaY + " se alcanzaría una distancia de " + prediccionDistancia);
                    }
                }
            }

            // FIN DEL RAZONAMIENTO PREVIO
            if ((mejorFuerzaX == 0) && (mejorFuerzaY == 0)) { 
                texto.text = "NO SE LANZÓ LA BOLA: la distancia de " + distanciaObjetivo + "m no se ha alcanzado suficientes veces.";
                print(texto.text);
            }
            else
            {
                bolosObjetivo = Instantiate(bolosCopia) as GameObject;
                instanciaBola = Instantiate(bola) as GameObject;
                instanciaBola.transform.position = new Vector3(transform.position.x, alturaInicialBola, transform.position.z + distanciaInicialBola); // Posición inicial en frente del personaje
                rb = instanciaBola.GetComponent<Rigidbody>();                               // EN EL JUEGO: utiliza la bola física del juego (si no existe la crea)
                rb.AddForce(new Vector3(0, mejorFuerzaY, mejorFuerzaX), ForceMode.Impulse); // y la lanza en el videojuego con la fuerza encontrada
                
                print("DECISION REALIZADA: Se lanzó la bola con fuerza Fx = " + mejorFuerzaX + " y Fy = " + mejorFuerzaY);
                ESTADO = "Acción realizada";
            }
        }
        
        if (ESTADO == "Acción realizada")
        {
            texto.text = "Para unos bolos a " + distanciaObjetivo.ToString("0.000") + "m, las fuerzas Fx y Fy a utilizar serán: " + mejorFuerzaX.ToString("0.000") + "N y " + mejorFuerzaY.ToString("0.000") + "N, respectivamente";
            
            // Cuando la bola cae por debajo de 1.8 m
            if (rb.transform.position.y < 1.8)                                            
            {                                                                          
                // Escribe la distancia en x alcanzada
                print("Los bolos están a una distancia de " + distanciaObjetivo + "m");
                print("La bola lanzada llegó a " + rb.transform.position.z + ". El error fue de " + (rb.transform.position.z - distanciaObjetivo).ToString("0.000000") + "m");
                
                rb.isKinematic = true;
                ESTADO = "FIN";
            }
        }
    }
}
