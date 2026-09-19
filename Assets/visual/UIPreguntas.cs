using UnityEngine;
using UnityEngine.UI;
using TMPro; // <-- 1. Importamos TextMeshPro
using System.Collections.Generic;

[System.Serializable]
public class PreguntaData
{
    public string texto;
    public string[] opciones;
    public int indiceCorrecta;
}

[System.Serializable]
public class BancoPreguntas
{
    public PreguntaData[] preguntas;
}

public class UIPreguntas : MonoBehaviour
{
    public static UIPreguntas Instancia; 

    [Header("UI Elements")]
    public GameObject panelPregunta; 
    public TextMeshProUGUI textoPregunta; // <-- 2. Cambiamos a TextMeshProUGUI
    public Button[] botonesOpciones;

    [Header("Datos")]
    public TextAsset archivoJson; 
    
    private BancoPreguntas banco;
    private Jugador jugadorActual;
    private PreguntaData preguntaActual;

    private void Awake()
    {
        Instancia = this;
        panelPregunta.SetActive(false); 
        CargarPreguntas();
    }

    private void CargarPreguntas()
    {
        if (archivoJson != null)
        {
            banco = JsonUtility.FromJson<BancoPreguntas>(archivoJson.text);
        }
    }

    public void MostrarPregunta(Jugador jugador)
    {
        jugadorActual = jugador;
        
        int indiceAleatorio = Random.Range(0, banco.preguntas.Length);
        preguntaActual = banco.preguntas[indiceAleatorio];

        textoPregunta.text = preguntaActual.texto;

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            int indexRespuesta = i; 
            
            // <-- 3. Buscamos TextMeshProUGUI dentro de los botones
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = preguntaActual.opciones[i];
            
            botonesOpciones[i].onClick.RemoveAllListeners();
            botonesOpciones[i].onClick.AddListener(() => AlPresionarBoton(indexRespuesta));
        }

        panelPregunta.SetActive(true); 
    }

   private void AlPresionarBoton(
    int opcionSeleccionada
)
{
    // ---------------------------------------------------------
    // REGISTRAR LA RESPUESTA
    // ---------------------------------------------------------

    bool respuestaCorrecta =
        jugadorActual.ResponderPregunta(
            opcionSeleccionada,
            preguntaActual.indiceCorrecta
        );


    // ---------------------------------------------------------
    // ACTUALIZAR LA INTERFAZ
    // ---------------------------------------------------------

    if (UIJuego.Instancia != null)
    {
        UIJuego.Instancia.MostrarResultado(
            jugadorActual,
            respuestaCorrecta
        );
    }


    // ---------------------------------------------------------
    // CERRAR PREGUNTA
    // ---------------------------------------------------------

    panelPregunta.SetActive(false);


    // ---------------------------------------------------------
    // CONTINUAR TURNO
    // ---------------------------------------------------------

    GameManager.Instancia.ReanudarTurno();
}
}