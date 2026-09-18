using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    private List<CasilleroBase> tablero = new List<CasilleroBase>();
    private List<Jugador> jugadores = new List<Jugador>();
    private int jugadorActual = 0;

    public bool EsperandoRespuesta { get; private set; } = false;

    // Control del final del juego
    public bool JuegoTerminado { get; private set; } = false;
    public Jugador Ganador { get; private set; }

    [Header("Fichas de Jugadores (Asignar 4)")]
    public FichaVisual[] fichasVisuales3D;

    [Header("Configuración del Tablero")]
    public Transform contenedorCasillas;
    private Vector3[] rutaPosiciones;


    private void Awake()
    {
        Instancia = this;
    }


    private void Start()
    {
        Debug.Log("--- INICIANDO JUEGO DE LA OCA ---");

        if (contenedorCasillas == null)
            return;

        ObtenerRutaDesdeContenedor();
        InicializarJugadores();
        InicializarTablero();

        // Colocar las 4 fichas en la salida
        for (int i = 0; i < fichasVisuales3D.Length; i++)
        {
            if (fichasVisuales3D[i] != null && rutaPosiciones.Length > 0)
            {
                fichasVisuales3D[i].transform.position =
                    rutaPosiciones[0] +
                    fichasVisuales3D[i].offsetFicha;
            }
        }

        MostrarJugadorActual();
    }


    private void ObtenerRutaDesdeContenedor()
    {
        int cantidad = contenedorCasillas.childCount;

        rutaPosiciones = new Vector3[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            rutaPosiciones[i] =
                contenedorCasillas.GetChild(i).position;
        }
    }


    private void InicializarJugadores()
    {
        // Añadimos a los 4 jugadores
        jugadores.Add(new Jugador(1, "Jugador Rojo"));
        jugadores.Add(new Jugador(2, "Jugador Azul"));
        jugadores.Add(new Jugador(3, "Jugador Verde"));
        jugadores.Add(new Jugador(4, "Jugador Amarillo"));
    }


    private void InicializarTablero()
    {
        int cantidadCasilleros = rutaPosiciones.Length;

        for (int i = 0; i < cantidadCasilleros; i++)
        {
            int siguiente = i + 1;

            List<int> siguientesIds =
                (i == cantidadCasilleros - 1)
                ? new List<int>()
                : new List<int> { siguiente };

            if (i == 0 || i == cantidadCasilleros - 1)
            {
                tablero.Add(
                    new CasilleroNormal(i, siguientesIds)
                );
            }
            else if (i % 3 == 0)
            {
                tablero.Add(
                    new CasilleroPregunta(i, siguientesIds)
                );
            }
            else if (i % 5 == 0)
            {
                tablero.Add(
                    new CasilleroEspecial(i, siguientesIds)
                );
            }
            else
            {
                tablero.Add(
                    new CasilleroNormal(i, siguientesIds)
                );
            }
        }
    }


    private void MostrarJugadorActual()
    {
        if (JuegoTerminado)
            return;

        Jugador jugador = jugadores[jugadorActual];

        Debug.Log(
            $"Es el turno de: {jugador.Nombre} " +
            $"| Ronda: {jugador.RondaActual}/3 " +
            $"| Respuestas: {jugador.RespuestasCorrectas}/{jugador.RespuestasTotales}"
        );
    }


    public void TirarDado()
    {
        // Si el juego terminó, no se puede seguir jugando
        if (JuegoTerminado)
            return;

        // No permite tirar mientras hay una pregunta abierta
        if (contenedorCasillas == null || EsperandoRespuesta)
            return;

        Jugador jugador = jugadores[jugadorActual];

        int resultado = jugador.LanzarDado();

        if (resultado <= 0)
        {
            SiguienteTurno();
            return;
        }

        int posicionAnterior = jugador.PosicionActualId;

        jugador.Moverse(
            resultado,
            tablero.Count
        );

        // Comprobamos si completó una vuelta
        bool completoVuelta =
            posicionAnterior + resultado >= tablero.Count;

        if (completoVuelta)
        {
            bool terminoJuego =
                EvaluarCambioDeRondaActual();

            if (terminoJuego)
            {
                return;
            }
        }

        // Mover la ficha visual correspondiente
        if (jugadorActual < fichasVisuales3D.Length &&
            fichasVisuales3D[jugadorActual] != null)
        {
            fichasVisuales3D[jugadorActual].MoverAdelante(
                resultado,
                rutaPosiciones
            );
        }

        // Buscar el casillero donde cayó
        CasilleroBase casilleroActual =
            tablero.Find(
                c => c.Id == jugador.PosicionActualId
            );

        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);

            if (casilleroActual.Tipo ==
                TipoCasillero.Pregunta)
            {
                EsperandoRespuesta = true;
                return;
            }
        }

        SiguienteTurno();
    }


    // Evalúa qué ocurre cuando el jugador completa una vuelta
    public bool EvaluarCambioDeRondaActual()
    {
        if (jugadores.Count == 0)
            return false;

        Jugador jugador = jugadores[jugadorActual];

        Debug.Log(
            $"[VUELTA] {jugador.Nombre} completó " +
            $"la Vuelta {jugador.RondaActual}."
        );

        // Si completó la tercera vuelta, gana
        if (jugador.RondaActual == 3)
        {
            FinalizarJuego(jugador);
            return true;
        }

        // En las vueltas 1 y 2 necesita superar el 70%
        bool avanzo =
            jugador.IntentarAvanzarDeRonda();

        if (avanzo)
        {
            Debug.Log(
                $"[VUELTA] ¡{jugador.Nombre} avanzó a la " +
                $"Vuelta {jugador.RondaActual}!"
            );
        }
        else
        {
            Debug.Log(
                $"[VUELTA] {jugador.Nombre} debe repetir " +
                $"la Vuelta {jugador.RondaActual}."
            );
        }

        return false;
    }


    // Finaliza el juego y establece al ganador
    private void FinalizarJuego(Jugador ganador)
{
    JuegoTerminado = true;
    Ganador = ganador;

    ganador.MarcarComoGanador();

    EsperandoRespuesta = false;

    Debug.Log("      ¡JUEGO TERMINADO!");
    Debug.Log($"      GANADOR: {ganador.Nombre}");

    VictoriaManager.NombreGanador = ganador.Nombre;

    SceneManager.LoadScene("Victoria");
}

    public void ReanudarTurno()
    {
        if (JuegoTerminado)
            return;

        EsperandoRespuesta = false;

        SiguienteTurno();
    }


    private void SiguienteTurno()
    {
        if (JuegoTerminado)
            return;

        jugadorActual++;

        if (jugadorActual >= jugadores.Count)
        {
            jugadorActual = 0;
        }

        MostrarJugadorActual();
    }
    
}