using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPreguntas : MonoBehaviour, IPreguntaView
{
    public static UIPreguntas Instancia;

    [Header("UI Elements")]
    public GameObject panelPregunta;
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI textoTemporizador;
    public Button[] botonesOpciones;

    [Header("Datos")]
    public TextAsset archivoJson;

    [Header("Temporizador")]
    public float tiempoPorPregunta = 15f;

    private BancoPreguntas banco;
    private PreguntaPresenter presentador;

    private Coroutine coroutineTemporizador;

    private bool respuestaProcesada = false;

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
        respuestaProcesada = false;

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
                () => SeleccionarOpcion(indiceRespuesta)
            );
        }

        panelPregunta.SetActive(true);

        IniciarTemporizador();
    }

    private void SeleccionarOpcion(int indiceRespuesta)
    {
        if (respuestaProcesada)
        {
            return;
        }

        respuestaProcesada = true;

        DetenerTemporizador();

        OpcionSeleccionada?.Invoke(indiceRespuesta);
    }

    public void OcultarPregunta()
    {
        DetenerTemporizador();

        panelPregunta.SetActive(false);
    }

    private void IniciarTemporizador()
    {
        DetenerTemporizador();

        if (textoTemporizador != null)
        {
            textoTemporizador.text =
                "TIEMPO: " + Mathf.CeilToInt(tiempoPorPregunta);
        }

        coroutineTemporizador =
            StartCoroutine(TemporizadorPregunta());
    }

    private void DetenerTemporizador()
    {
        if (coroutineTemporizador != null)
        {
            StopCoroutine(coroutineTemporizador);
            coroutineTemporizador = null;
        }
    }

    private IEnumerator TemporizadorPregunta()
    {
        float tiempoRestante = tiempoPorPregunta;

        while (tiempoRestante > 0f)
        {
            if (textoTemporizador != null)
            {
                textoTemporizador.text =
                    "TIEMPO: " + Mathf.CeilToInt(tiempoRestante);
            }

            yield return null;

            tiempoRestante -= Time.deltaTime;
        }

        if (respuestaProcesada)
        {
            yield break;
        }

        respuestaProcesada = true;

        if (textoTemporizador != null)
        {
            textoTemporizador.text = "TIEMPO: 0";
        }

        OpcionSeleccionada?.Invoke(-1);

        coroutineTemporizador = null;
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