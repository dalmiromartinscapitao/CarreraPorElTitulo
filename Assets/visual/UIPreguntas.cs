using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPreguntas : MonoBehaviour, IPreguntaView
{
    public static UIPreguntas Instancia;

    [Header("UI Elements")]
    public GameObject panelPregunta;
    public TextMeshProUGUI textoPregunta;
    public Button[] botonesOpciones;

    [Header("Datos")]
    public TextAsset archivoJson;

    private BancoPreguntas banco;
    private PreguntaPresenter presentador;

    public event Action<int> OpcionSeleccionada;
    public event Action<Jugador, bool> RespuestaProcesada;

    private void Awake()
    {
        Instancia = this;

        panelPregunta.SetActive(false);

        CargarPreguntas();

        presentador = new PreguntaPresenter(this, banco);

        presentador.RespuestaProcesada += NotificarRespuesta;
    }

    private void CargarPreguntas()
    {
        banco = JsonUtility.FromJson<BancoPreguntas>(
            archivoJson.text
        );
    }

    public void MostrarPregunta(Jugador jugador)
    {
        presentador.MostrarPregunta(jugador);
    }

    public void MostrarPreguntaEnPantalla(PreguntaData pregunta)
    {
        textoPregunta.text = pregunta.texto;

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            int indiceRespuesta = i;

            botonesOpciones[i]
                .GetComponentInChildren<TextMeshProUGUI>()
                .text = pregunta.opciones[i];

            botonesOpciones[i].onClick.RemoveAllListeners();

            botonesOpciones[i].onClick.AddListener(
                () => OpcionSeleccionada?.Invoke(indiceRespuesta)
            );
        }

        panelPregunta.SetActive(true);
    }

    public void OcultarPregunta()
    {
        panelPregunta.SetActive(false);
    }

    private void NotificarRespuesta(
        Jugador jugador,
        bool respuestaCorrecta)
    {
        RespuestaProcesada?.Invoke(
            jugador,
            respuestaCorrecta
        );
    }
}