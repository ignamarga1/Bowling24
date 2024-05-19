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

public class AprendizLento_2_incognitas : MonoBehaviour
{
    private string ESTADO = "Sin conocimiento";
    public GameObject bola, bolosObjetivo;
    Rigidbody r;
    Text texto;

    GameObject instanciaBola, puntoObjetivo;
    public float valorMaximoFz, valorMaximoFy, paso, Velocidad_Simulacion = 1;
    float mejorFuerzaZ, mejorFuerzaY, mejorGiroY, distanciaObjetivo;
    float alturaInicialBola = 1.5f;     // Altura inicial de la bola
    float distanciaInicialBola = 2f;    // Distancia inicial de la bola desde el personaje

    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirFuerzaZ;
    weka.core.Instances casosEntrenamiento;

    void Start()
    {
        Time.timeScale = Velocidad_Simulacion;                              // Opcional: hace que se vea más rápido (recomendable hasta 5)
        texto = Canvas.FindObjectOfType<Text>();
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");  // Lanza el proceso de entrenamiento                                                                                    
    }

    IEnumerator Entrenamiento()
    {
        // Definir los atributos
        FastVector atributos = new FastVector(4);
        atributos.addElement(new Attribute("Fz"));
        atributos.addElement(new Attribute("Fy"));
        atributos.addElement(new Attribute("giroY"));
        atributos.addElement(new Attribute("distancia"));

        // Crear la tabla vacía
        casosEntrenamiento = new weka.core.Instances("Entrenamiento", atributos, 0);

        if (casosEntrenamiento.numInstances() < 10)
        {
            texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fz, Fy y el giro Y utilizados y las distancias alcanzadas.";
            print("Datos de entrada: valorMaximoFz = " + valorMaximoFz + " valorMaximoFy = " + valorMaximoFy + " " + ((valorMaximoFz == 0 || valorMaximoFy == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for (float Fz = 1; Fz <= valorMaximoFz; Fz += paso)                             // Bucle de planificación de la fuerza FZ durante el entrenamiento
            {
                for (float Fy = 1; Fy <= valorMaximoFy; Fy = Fy + paso)                     // Bucle de planificación de la fuerza FY durante el entrenamiento
                {
                    for (float giroY = -5; giroY <= 5; giroY += 5)                          // Bucle de planificación del giro en el eje Y durante el entrenamiento
                    {
                        instanciaBola = Instantiate(bola) as GameObject;
                        instanciaBola.transform.position = new Vector3(transform.position.x, alturaInicialBola, transform.position.z + distanciaInicialBola); // Posición inicial en frente del personaje
                        Rigidbody rb = instanciaBola.GetComponent<Rigidbody>();             // Crea una bola con físicas
                        rb.AddForce(new Vector3(giroY, Fy, Fz), ForceMode.Impulse);         // y la lanza con las fuerza Fz, Fy y el giro Y
                        yield return new WaitUntil(() => (rb.transform.position.z > distanciaObjetivo || rb.transform.position.z < 0 || rb.transform.position.x < -5 || rb.transform.position.x > 5)); // Espera a que la bola llegue a los bolos, se detenga o se caiga por los carriles laterales

                        Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                        casoAaprender.setDataset(casosEntrenamiento);                           // Crea un registro de experiencia
                        casoAaprender.setValue(0, Fz);                                          // Guarda los datos de las fuerzas Fz, Fy y el giro Y utilizados
                        casoAaprender.setValue(1, Fy);
                        casoAaprender.setValue(2, giroY);
                        casoAaprender.setValue(3, rb.transform.position.z);                     // Anota la distancia alcanzada
                        casosEntrenamiento.add(casoAaprender);                                  // Guarda el registro en la lista casosEntrenamiento

                        rb.isKinematic = true; rb.GetComponent<Collider>().isTrigger = true;    // Opcional: paraliza la bola
                        Destroy(instanciaBola, 1);                                              // Opcional: destruye la bola
                    }                                                                           // FIN bucle de lanzamientos con diferentes giros en el eje Y
                }                                                                               // FIN bucle de lanzamientos con diferentes fuerzas
            }

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
        saberPredecirFuerzaZ = new M5P();                                               // Crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(0);                                            // y hace que aprenda Fz dada la distancia, Fy y el giro en el eje Y
        saberPredecirFuerzaZ.buildClassifier(casosEntrenamiento);                       // REALIZA EL APRENDIZAJE DE FZ A PARTIR DE LA DISTANCIA, FY Y EL GIRO EN EL EJE Y

        saberPredecirDistancia = new M5P();                                             // Crea otro algoritmo de aprendizaje M5P (árboles de regresión)  
        casosEntrenamiento.setClassIndex(3);                                            // La variable a aprender es la distancia dada Fz, Fy y el giro en el eje Y                                                                                         
        saberPredecirDistancia.buildClassifier(casosEntrenamiento);                     // Este algoritmo aprende un "modelo físico aproximado"

        ESTADO = "Con conocimiento";

        print(casosEntrenamiento.numInstances() + " espers " + saberPredecirDistancia.toString());

        // EVALUACIÓN DEL CONOCIMIENTO APRENDIDO 
        if (casosEntrenamiento.numInstances() >= 10)
        {
            casosEntrenamiento.setClassIndex(0);
            Evaluation evaluador = new Evaluation(casosEntrenamiento);                   // Opcional: si tiene más de 10 ejemplos, estima la posible precisión
            evaluador.crossValidateModel(saberPredecirFuerzaZ, casosEntrenamiento, 10, new java.util.Random(1));

            print("El Error Absoluto Promedio con Fz durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " N");
            casosEntrenamiento.setClassIndex(3);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " m");
        }

        // Generación aleatoria de una distancia dependiendo de sus límites        
        AttributeStats estadisticasDistancia = casosEntrenamiento.attributeStats(3);        //Opcional: Inicializa las estadisticas de las distancias
        float maximaDistanciaAlcanzada = (float)estadisticasDistancia.numericStats.max;     //Opcional: Obtiene el valor máximo de las distancias alcanzadas
        distanciaObjetivo = UnityEngine.Random.Range(maximaDistanciaAlcanzada * 0.2f, maximaDistanciaAlcanzada * 0.8f);  //Opcional: calculo aleatorio de la distancia 

        //puntoObjetivo.transform.position = bolosObjetivo.transform.position;
    }

    // DURANTE EL JUEGO: aplica lo aprendido para lanzar los bolos
    void FixedUpdate()
    {
        if ((ESTADO == "Con conocimiento") && (distanciaObjetivo > 0))
        {
            Time.timeScale = 1;                                                                                 // Durante el juego, el NPC razona así:
            mejorFuerzaZ = 0; mejorFuerzaY = 0; mejorGiroY = 0;                                                  // INICIALIZA LAS VARIABLES (Nota: Si no cambia, el objetivo aún no se ha alcanzado)
            float menorDistancia = 1e9f;
            print("-- OBJETIVO: LANZAR LA bola A UNA DISTANCIA DE " + distanciaObjetivo + " m.");

            // Si usa dos bucles Fz y Fy con "modelo físico aproximado", complejidad n^2
            // Reduce la complejidad con un solo bucle FOR, así
            for (float Fy = 1; Fy < valorMaximoFy; Fy = Fy + paso)                                              // Bucle FOR con fuerza Fy, deduce Fz = f(Fy, distancia) y escoge mejor combinación         
            {
                for (float giroY = -5; giroY <= 5; giroY += 5)                                                  // Bucle FOR con el giro en el eje Y
                {
                    Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba.setDataset(casosEntrenamiento);
                    casoPrueba.setValue(1, Fy);                                                                 // Crea un registro con una Fy
                    casoPrueba.setValue(2, giroY);                                                              // y el giro en el eje Y
                    casoPrueba.setValue(3, distanciaObjetivo);                                                  // y la distancia
                    float Fz = (float)saberPredecirFuerzaZ.classifyInstance(casoPrueba);                        // Predice Fz a partir de la distancia, Fy y el giro en el eje Y
                    if ((Fz >= 1) && (Fz <= valorMaximoFz))
                    {
                        Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                        casoPrueba2.setDataset(casosEntrenamiento);                                             // Utiliza el "modelo físico aproximado" con Fz y Fy
                        casoPrueba2.setValue(0, Fz);                                                            // Crea una registro con una Fz
                        casoPrueba2.setValue(1, Fy);                                                            // Crea una registro con una Fy
                        casoPrueba2.setValue(2, giroY);                                                         // Crea una registro con el giro en el eje Y
                        float prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPrueba2); // Predice la distancia dada Fz, Fy y el giro en el eje Y
                        if (Mathf.Abs(prediccionDistancia - distanciaObjetivo) < menorDistancia)                // Busca la Fy con una distancia más cercana al objetivo
                        {
                            menorDistancia = Mathf.Abs(prediccionDistancia - distanciaObjetivo);                // Si encuentra una buena toma nota de esta distancia
                            mejorFuerzaZ = Fz;                                                                  // de las fuerzas que uso, Fz
                            mejorFuerzaY = Fy;                                                                  // también Fy
                            mejorGiroY = giroY;                                                                 // y el giro en el eje Y
                            print("RAZONAMIENTO: Una posible acción es ejercer una fuerza Fz = " + mejorFuerzaZ + " y Fy = " + mejorFuerzaY + " con un giro Y de " + mejorGiroY + " se alcanzaría una distancia de " + prediccionDistancia);
                        }
                    }
                }
            }                                                                                                    // FIN DEL RAZONAMIENTO PREVIO

            if ((mejorFuerzaZ == 0) && (mejorFuerzaY == 0) && (mejorGiroY == 0))
            {
                texto.text = "NO SE LANZÓ LA bola: La distancia de " + distanciaObjetivo + " m no se ha alcanzado muchas veces.";
                print(texto.text);
            }
            else
            {
                instanciaBola = Instantiate(bola) as GameObject;
                instanciaBola.transform.position = new Vector3(transform.position.x, alturaInicialBola, transform.position.z + distanciaInicialBola); // Posición inicial en frente del personaje
                r = instanciaBola.GetComponent<Rigidbody>();                                                            // EN EL JUEGO: utiliza la bola física del juego (si no existe la crea)
                r.AddForce(new Vector3(mejorGiroY, mejorFuerzaY, mejorFuerzaZ), ForceMode.Impulse);                      // la lanza en el videojuego con la fuerza encontrada y el giro en el eje Y
                print("DECISION REALIZADA: Se lanzó bola con fuerza Fz = " + mejorFuerzaZ + ", Fy = " + mejorFuerzaY + " y giro Y = " + mejorGiroY);
                ESTADO = "Acción realizada";
            }
        }

        if (ESTADO == "Acción realizada")
        {
            texto.text = "Para una canasta a " + distanciaObjetivo.ToString("0.000") + " m, las fuerzas Fz y Fy a utilizar serán: " + mejorFuerzaZ.ToString("0.000") + "N y " + mejorFuerzaY.ToString("0.000") + "N, respectivamente, con un giro en el eje Y de " + mejorGiroY.ToString("0.000") + "°";
            if (r.transform.position.y < 0)                                            // Cuando la bola cae por debajo de 0 m
            {                                                                          // Escribe la distancia en x alcanzada
                print("Los bolos están a una distancia de " + distanciaObjetivo + " m");
                print("La bola lanzada llegó a " + r.transform.position.z + ". El error fue de " + (r.transform.position.z - distanciaObjetivo).ToString("0.000000") + " m");
                r.isKinematic = true;
                ESTADO = "FIN";
            }
        }
    }
}
