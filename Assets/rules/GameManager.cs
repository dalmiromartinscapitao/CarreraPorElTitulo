using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    private List<CasilleroBase> tablero =
        new List<CasilleroBase>();

    private List<Jugador> jugadores =
        new List<Jugador>();

    private int jugadorActual = 0;

    public bool EsperandoRespuesta { get; private set; }
        = false;

    public bool JuegoTerminado { get; private set; }
        = false;

    public Jugador Ganador { get; private set; }

    [Header("Fichas de Jugadores (Asignar 4)")]
    public FichaVisual[] fichasVisuales3D;

    [Header("Cámaras de Jugadores (Asignar 4)")]
    public Camera[] camarasJugadores;

    [Header("Configuración del Tablero")]
    public Transform contenedorCasillas;

    private Vector3[] rutaPosiciones;

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        Debug.Log(
            "--- INICIANDO JUEGO DE LA OCA ---"
        );
        if (contenedorCasillas == null)
        {
            Debug.LogError(
                "[GameManager] No se asignó el " +
                "Contenedor de Casillas."
            );

            return;
        }
        ObtenerRutaDesdeContenedor();
        InicializarJugadores();
        InicializarTablero();

        for (
            int i = 0;
            i < fichasVisuales3D.Length;
            i++
        )
        {
            if (
                fichasVisuales3D[i] != null &&
                rutaPosiciones.Length > 0
            )
            {
                fichasVisuales3D[i]
                    .SincronizarPosicion(
                        0,
                        rutaPosiciones
                    );
            }
        }

        MostrarJugadorActual();
    }

    private void ObtenerRutaDesdeContenedor()
    {
        int cantidad =
            contenedorCasillas.childCount;

        rutaPosiciones =
            new Vector3[cantidad];

        for (
            int i = 0;
            i < cantidad;
            i++
        )
        {
            rutaPosiciones[i] =
                contenedorCasillas
                    .GetChild(i)
                    .position;
        }
    }

    private void InicializarJugadores()
    {
        jugadores.Add(
            new Jugador(
                1,
                "Jugador Rojo"
            )
        );

        jugadores.Add(
            new Jugador(
                2,
                "Jugador Azul"
            )
        );

        jugadores.Add(
            new Jugador(
                3,
                "Jugador Verde"
            )
        );

        jugadores.Add(
            new Jugador(
                4,
                "Jugador Amarillo"
            )
        );
    }

    private void InicializarTablero()
    {
        int cantidadCasilleros =
            rutaPosiciones.Length;

        for (
            int i = 0;
            i < cantidadCasilleros;
            i++
        )
        {
            int siguiente =
                i + 1;

            List<int> siguientesIds =
                (
                    i ==
                    cantidadCasilleros - 1
                )
                ?
                new List<int>()
                :
                new List<int>
                {
                    siguiente
                };

            if (
                i == 0 ||
                i == cantidadCasilleros - 1
            )
            {
                tablero.Add(
                    new CasilleroNormal(
                        i,
                        siguientesIds
                    )
                );
            }
            else if (i % 3 == 0)
            {
                tablero.Add(
                    new CasilleroPregunta(
                        i,
                        siguientesIds
                    )
                );
            }
            else if (i % 5 == 0)
            {
                tablero.Add(
                    new CasilleroEspecial(
                        i,
                        siguientesIds
                    )
                );
            }
            else
            {
                tablero.Add(
                    new CasilleroNormal(
                        i,
                        siguientesIds
                    )
                );
            }
        }
    }

    public Jugador ObtenerJugador(int indice)
{
    if (
        indice < 0 ||
        indice >= jugadores.Count
    )
    {
        return null;
    }

    return jugadores[indice];
}

    public Jugador ObtenerJugadorActual()
{
    if (
        jugadorActual < 0 ||
        jugadorActual >= jugadores.Count
    )
    {
        return null;
    }

    return jugadores[jugadorActual];
}

    private void MostrarJugadorActual()
    {
        if (JuegoTerminado)
            return;

        Jugador jugador =
            jugadores[jugadorActual];

        CambiarCamaraJugador();

        Debug.Log(
            $"[TURNO] {jugador.Nombre} | " +
            $"Vuelta: {jugador.RondaActual}/3 | " +
            $"Correctas: {jugador.RespuestasCorrectas} | " +
            $"Objetivo: {jugador.ObtenerObjetivoDeRonda()} | " +
            $"Casillero: {jugador.PosicionActualId + 1}"
        );

        if (UIJuego.Instancia != null)
    {
            UIJuego.Instancia.ActualizarInterfaz();
    }
    }

    private void CambiarCamaraJugador()
    {
        if (
            camarasJugadores == null ||
            camarasJugadores.Length == 0
        )
        {
            Debug.LogWarning(
                "[CAMARAS] No hay cámaras asignadas."
            );

            return;
        }

        for (
            int i = 0;
            i < camarasJugadores.Length;
            i++
        )
        {
            if (camarasJugadores[i] != null)
            {
                camarasJugadores[i]
                    .gameObject
                    .SetActive(false);
            }
        }

        if (
            jugadorActual < 0 ||
            jugadorActual >= camarasJugadores.Length
        )
        {
            Debug.LogWarning(
                "[CAMARAS] No existe una cámara " +
                "para el jugador actual."
            );

            return;
        }

        if (camarasJugadores[jugadorActual] != null)
        {
            camarasJugadores[jugadorActual]
                .gameObject
                .SetActive(true);


            Debug.Log(
                $"[CAMARA] Cámara cambiada a " +
                $"Jugador {jugadorActual + 1}"
            );
        }
        else
        {
            Debug.LogWarning(
                $"[CAMARAS] La cámara del jugador " +
                $"{jugadorActual + 1} no está asignada."
            );
        }
    }

    public void TirarDado()
    {
        if (JuegoTerminado)
            return;

        if (
            contenedorCasillas == null ||
            EsperandoRespuesta
        )
        {
            return;
        }

        Jugador jugador =
            jugadores[jugadorActual];

        if (jugador.DebeRepetirPreguntas)
        {
            Debug.Log(
                $"[BLOQUEADO] {jugador.Nombre} " +
                "debe responder una pregunta " +
                "en el casillero 28."
            );

            Debug.Log(
                $"[BLOQUEADO] Tiene " +
                $"{jugador.RespuestasCorrectas} " +
                $"correctas de " +
                $"{jugador.ObtenerObjetivoDeRonda()}."
            );
            EsperandoRespuesta = true;
            UIPreguntas.Instancia
                .MostrarPregunta(jugador);


            return;
        }

        int resultado =
            jugador.LanzarDado();

        if (resultado <= 0)
        {
            SiguienteTurno();
            return;
        }

        int posicionAnterior =
            jugador.PosicionActualId;

        int ultimaPosicion =
            tablero.Count - 1;

        int posicionCalculada =
            posicionAnterior + resultado;

        if (
            posicionCalculada >=
            ultimaPosicion
        )
        {
            jugador.EstablecerPosicion(
                ultimaPosicion
            );

            MoverFichaAIndice(
                jugadorActual,
                ultimaPosicion
            );


            Debug.Log(
                $"[VUELTA] {jugador.Nombre} llegó " +
                "al casillero 28."
            );

            if (
                !jugador.TieneRespuestasNecesarias()
            )
            {
                jugador.IntentarCompletarVuelta();

                Debug.Log(
                    $"[BLOQUEADO] {jugador.Nombre} " +
                    "debe quedarse en el casillero 28."
                );

                Debug.Log(
                    $"[BLOQUEADO] Le faltan " +
                    $"{jugador.ObtenerRespuestasFaltantes()} " +
                    "respuestas correctas."
                );
                SiguienteTurno();
                return;
            }

            int rondaAntes =
                jugador.RondaActual;
            bool completo =
                jugador.IntentarCompletarVuelta();
            if (!completo)
            {
                SiguienteTurno();
                return;
            }
            if (rondaAntes == 3)
            {
                FinalizarJuego(jugador);
                return;
            }
            MoverFichaAIndice(
                jugadorActual,
                jugador.PosicionActualId
            );
            Debug.Log(
                $"[VUELTA] {jugador.Nombre} ahora " +
                $"está en la Vuelta " +
                $"{jugador.RondaActual}."
            );
            Debug.Log(
                $"[VUELTA] {jugador.Nombre} volvió " +
                "al casillero 1."
            );
            SiguienteTurno();
            return;
        }
        jugador.Moverse(
            resultado,
            tablero.Count
        );
        MoverFichaAIndice(
            jugadorActual,
            jugador.PosicionActualId
        );
        CasilleroBase casilleroActual =
            tablero.Find(
                c =>
                    c.Id ==
                    jugador.PosicionActualId
            );


        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(
                jugador
            );
            if (
                casilleroActual.Tipo ==
                TipoCasillero.EfectoEspecial
            )
            {
                if (jugador.PosicionActualId < 0)
                {
                    jugador.EstablecerPosicion(0);
                }


                if (
                    jugador.PosicionActualId >=
                    tablero.Count
                )
                {
                    jugador.EstablecerPosicion(
                        tablero.Count - 1
                    );
                }


                MoverFichaAIndice(
                    jugadorActual,
                    jugador.PosicionActualId
                );
            }

            if (
                casilleroActual.Tipo ==
                TipoCasillero.Pregunta
            )
            {
                EsperandoRespuesta = true;

                return;
            }
        }

        SiguienteTurno();
    }

    private void MoverFichaAIndice(
        int indiceJugador,
        int indiceCasillero
    )
    {
        if (
            indiceJugador < 0 ||
            indiceJugador >= fichasVisuales3D.Length
        )
        {
            return;
        }


        if (
            fichasVisuales3D[indiceJugador] == null
        )
        {
            return;
        }


        if (
            rutaPosiciones == null ||
            rutaPosiciones.Length == 0
        )
        {
            return;
        }

        indiceCasillero =
            Mathf.Clamp(
                indiceCasillero,
                0,
                rutaPosiciones.Length - 1
            );


        fichasVisuales3D[indiceJugador]
            .MoverAIndice(
                indiceCasillero,
                rutaPosiciones
            );
    }
    private void FinalizarJuego(
        Jugador ganador
    )
    {
        JuegoTerminado = true;
        Ganador = ganador;
        ganador.MarcarComoGanador();
        EsperandoRespuesta = false;
        Debug.Log(
            "===================================="
        );
        Debug.Log(
            "          ¡JUEGO TERMINADO!"
        );
        Debug.Log(
            $"          GANADOR: {ganador.Nombre}"
        );
        Debug.Log(
            "===================================="
        );
        VictoriaManager.NombreGanador =
            ganador.Nombre;
        SceneManager.LoadScene(
            "Victoria"
        );
    }
    public void ReanudarTurno()
    {
        if (JuegoTerminado)
            return;
        EsperandoRespuesta = false;
        Jugador jugador =
            jugadores[jugadorActual];

        if (jugador.DebeRepetirPreguntas)
        {
            if (
                !jugador.TieneRespuestasNecesarias()
            )
            {
                Debug.Log(
                    $"[BLOQUEADO] {jugador.Nombre} " +
                    "todavía no consigue las respuestas " +
                    "necesarias."
                );
                Debug.Log(
                    $"[BLOQUEADO] Correctas: " +
                    $"{jugador.RespuestasCorrectas}/" +
                    $"{jugador.ObtenerObjetivoDeRonda()}"
                );
                SiguienteTurno();
                return;
            }
            int rondaAntes =
                jugador.RondaActual;
            bool completo =
                jugador.IntentarCompletarVuelta();
            if (!completo)
            {
                SiguienteTurno();
                return;
            }
            if (rondaAntes == 3)
            {
                FinalizarJuego(jugador);
                return;
            }
            MoverFichaAIndice(
                jugadorActual,
                jugador.PosicionActualId
            );
            Debug.Log(
                $"[VUELTA] {jugador.Nombre} pasó " +
                $"a la Vuelta {jugador.RondaActual}."
            );
            Debug.Log(
                "[VUELTA] La ficha volvió al casillero 1."
            );
        }
        SiguienteTurno();
    }
    private void SiguienteTurno()
    {
        if (JuegoTerminado)
            return;


        jugadorActual++;
        if (
            jugadorActual >=
            jugadores.Count
        )
        {
            jugadorActual = 0;
        }
        MostrarJugadorActual();
    }
}