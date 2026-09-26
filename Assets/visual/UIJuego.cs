using UnityEngine;
using TMPro;

public class UIJuego : MonoBehaviour
{
    public static UIJuego Instancia;

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

    private void Awake()
    {
        Instancia = this;

        if (cuadroResultado != null){
            cuadroResultado.SetActive(false);
        }
    }

    public void ActualizarInterfaz(){
        if (GameManager.Instancia == null)
            return;


        Jugador jugadorActual = GameManager.Instancia.ObtenerJugadorActual();


        if (jugadorActual != null){
            ActualizarTurno(jugadorActual);
        }

        ActualizarJugador(GameManager.Instancia.ObtenerJugador(0),textoRojo);

        ActualizarJugador(GameManager.Instancia.ObtenerJugador(1),textoAzul);

        ActualizarJugador(GameManager.Instancia.ObtenerJugador(2),textoVerde);


        ActualizarJugador(GameManager.Instancia.ObtenerJugador(3),textoAmarillo);
    }


    private void ActualizarTurno(Jugador jugador){
        if (textoTurno == null)
            return;

        string nombre = ObtenerNombreCorto(jugador.Nombre);

        string simbolo = ObtenerSimbolo(jugador.Id);

        textoTurno.text = $"TURNO DE: {simbolo} {nombre}";
    }

    private void ActualizarJugador(Jugador jugador,TextMeshProUGUI texto){

        if (jugador == null || texto == null)
            return;

        string nombre = ObtenerNombreCorto(jugador.Nombre);

        string simbolo = ObtenerSimbolo(jugador.Id);

        int vueltasTotales = GameManager.Instancia.ObtenerVueltasTotales();

        texto.text =
            $"{simbolo} {nombre}\n" +
            $"{jugador.RespuestasCorrectas} " +
            $"{TextoRespuestas(jugador.RespuestasCorrectas)}\n" +
            $"Vuelta {jugador.RondaActual}/" +
            $"{vueltasTotales}";
    }

    public void MostrarResultado(Jugador jugador,bool respuestaCorrecta){
        if (jugador == null)
            return;

        string nombre = ObtenerNombreCorto(jugador.Nombre);

        if (cuadroResultado != null){
            cuadroResultado.SetActive(true);
        }

        if (textoResultado == null)
            return;

        if (respuestaCorrecta){
            textoResultado.text =
                $"{ObtenerSimbolo(jugador.Id)} " +
                $"{nombre} respondió correctamente";
        }
        else{
            textoResultado.text =
                $"{ObtenerSimbolo(jugador.Id)} " +
                $"{nombre} respondió incorrectamente";
        }

        ActualizarInterfaz();
    }

    public void MostrarMovimientoEspecial(Jugador jugador,int cantidadCasilleros){

        if (jugador == null)
            return;
        
        string nombre = ObtenerNombreCorto(jugador.Nombre);

        if (cuadroResultado != null){
            cuadroResultado.SetActive(true);
        }

        if (textoResultado == null)
            return;

        if (cantidadCasilleros > 0){
            textoResultado.text =
                $"{ObtenerSimbolo(jugador.Id)} " +
                $"{nombre} avanza {cantidadCasilleros} " +
                TextoCasilleros(cantidadCasilleros);
        }
        else if (cantidadCasilleros < 0)
        {
            int retroceso = Mathf.Abs(cantidadCasilleros);
            
            textoResultado.text =
                $"{ObtenerSimbolo(jugador.Id)} " +
                $"{nombre} retrocede {retroceso} " +
                TextoCasilleros(retroceso) + ".";
        }

        ActualizarInterfaz();
    }

    public void MostrarPenalizacion(Jugador jugador){
        if (jugador == null)
            return;

        string nombre = ObtenerNombreCorto(jugador.Nombre);

        if (cuadroResultado != null){
            cuadroResultado.SetActive(true);
        }

        if (textoResultado == null)
            return;

        textoResultado.text =
            $"{ObtenerSimbolo(jugador.Id)} " +
            $"{nombre} está penalizado y pierde un turno";

        ActualizarInterfaz();
    }

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


    private string TextoRespuestas(
        int cantidad
    )
    {
        if (cantidad == 1)
            return "respuesta";

        return "respuestas";
    }


    private string TextoCasilleros(
        int cantidad
    )
    {
        if (cantidad == 1)
            return "casillero";

            return "casilleros";
    }
}