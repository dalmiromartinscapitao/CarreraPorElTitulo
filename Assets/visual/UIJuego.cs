using UnityEngine;
using TMPro;

public class UIJuego : MonoBehaviour
{
    public static UIJuego Instancia;


    // =========================================================
    // TEXTOS DE LA INTERFAZ
    // =========================================================

    [Header("Turno actual")]
    public TextMeshProUGUI textoTurno;


    [Header("Jugadores")]
    public TextMeshProUGUI textoRojo;
    public TextMeshProUGUI textoAzul;
    public TextMeshProUGUI textoVerde;
    public TextMeshProUGUI textoAmarillo;


    [Header("Resultado de pregunta")]
    public GameObject cuadroResultado;
    public TextMeshProUGUI textoResultado;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instancia = this;


        // El cuadro de resultado comienza oculto.
        if (cuadroResultado != null)
        {
            cuadroResultado.SetActive(false);
        }
    }


    // =========================================================
    // ACTUALIZAR TODA LA INTERFAZ
    // =========================================================

    public void ActualizarInterfaz()
    {
        if (GameManager.Instancia == null)
            return;


        // -----------------------------------------------------
        // JUGADOR ACTUAL
        // -----------------------------------------------------

        Jugador jugadorActual =
            GameManager.Instancia
                .ObtenerJugadorActual();


        if (jugadorActual != null)
        {
            ActualizarTurno(jugadorActual);
        }


        // -----------------------------------------------------
        // JUGADORES
        // -----------------------------------------------------

        ActualizarJugador(
            GameManager.Instancia.ObtenerJugador(0),
            textoRojo
        );


        ActualizarJugador(
            GameManager.Instancia.ObtenerJugador(1),
            textoAzul
        );


        ActualizarJugador(
            GameManager.Instancia.ObtenerJugador(2),
            textoVerde
        );


        ActualizarJugador(
            GameManager.Instancia.ObtenerJugador(3),
            textoAmarillo
        );
    }


    // =========================================================
    // ACTUALIZAR TURNO
    // =========================================================

    private void ActualizarTurno(
        Jugador jugador
    )
    {
        if (textoTurno == null)
            return;


        string nombre =
            ObtenerNombreCorto(jugador.Nombre);


        string simbolo =
            ObtenerSimbolo(jugador.Id);


        textoTurno.text =
            $"TURNO DE: {simbolo} {nombre}";
    }


    // =========================================================
    // ACTUALIZAR INFORMACIÓN DE UN JUGADOR
    // =========================================================

    private void ActualizarJugador(
        Jugador jugador,
        TextMeshProUGUI texto
    )
    {
        if (jugador == null || texto == null)
            return;


        string nombre =
            ObtenerNombreCorto(jugador.Nombre);


        string simbolo =
            ObtenerSimbolo(jugador.Id);


        texto.text =
            $"{simbolo} {nombre}\n" +
            $"{jugador.RespuestasCorrectas} " +
            $"{TextoRespuestas(jugador.RespuestasCorrectas)}\n" +
            $"Vuelta {jugador.RondaActual}/3";
    }


    // =========================================================
    // MOSTRAR RESULTADO DE PREGUNTA
    // =========================================================

    public void MostrarResultado(
        Jugador jugador,
        bool respuestaCorrecta
    )
    {
        if (jugador == null)
            return;


        string nombre =
            ObtenerNombreCorto(jugador.Nombre);


        if (cuadroResultado != null)
        {
            cuadroResultado.SetActive(true);
        }


        if (textoResultado == null)
            return;


        if (respuestaCorrecta)
        {
            textoResultado.text =
                "<color=#4CAF50>" +
                "¡RESPUESTA CORRECTA!" +
                "</color>\n" +
                $"{nombre} puede continuar";
        }
        else
        {
            textoResultado.text =
                "<color=#F44336>" +
                "RESPUESTA INCORRECTA" +
                "</color>\n" +
                $"{nombre} no sumó una respuesta";
        }


        // Actualizamos inmediatamente la cantidad
        // de respuestas correctas.
        ActualizarInterfaz();
    }


    // =========================================================
    // NOMBRE CORTO
    // =========================================================

    private string ObtenerNombreCorto(
        string nombreCompleto
    )
    {
        if (
            nombreCompleto.StartsWith(
                "Jugador "
            )
        )
        {
            return nombreCompleto
                .Substring(8)
                .ToUpper();
        }


        return nombreCompleto.ToUpper();
    }


    // =========================================================
    // SÍMBOLO DEL JUGADOR
    // =========================================================

    private string ObtenerSimbolo(
        int id
    )
    {
        switch (id)
        {
            case 1:
                return "<color=#E53935>●</color>";

            case 2:
                return "<color=#1E88E5>●</color>";

            case 3:
                return "<color=#43A047>●</color>";

            case 4:
                return "<color=#FDD835>●</color>";

            default:
                return "●";
        }
    }


    // =========================================================
    // TEXTO "RESPUESTA / RESPUESTAS"
    // =========================================================

    private string TextoRespuestas(
        int cantidad
    )
    {
        if (cantidad == 1)
            return "respuesta";

        return "respuestas";
    }
}