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

    private GestorVueltas gestorVueltas;

    public bool EsperandoRespuesta { get; private set; } = false;

    public bool JuegoTerminado { get; private set; } = false;

    public Jugador Ganador { get; private set; }

    [Header("Fichas de Jugadores (Asignar 4)")]
    public FichaVisual[] fichasVisuales3D;

    [Header("Script de la Cámara Principal")]
    public CamaraSigue camaraPrincipalScript;

    [Header("Dado 3D")]
    public DadoVisual dadoVisual;

    [Header("Configuración del Tablero")]
    public Transform contenedorCasillas;

    public TableroManager tableroManager;

    [Header("MINIJUEGO")]
    public MiniJuegoManager miniJuegoManager;

    private Vector3[] rutaPosiciones;

    private ConfiguracionPartida configuracion;


    private void Awake()
    {
        if (
            Instancia != null &&
            Instancia != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        configuracion =
            ConfiguracionPartida.Crear(
                SesionPartida.ModoSeleccionado
            );

        gestorVueltas =
            new GestorVueltas(
                configuracion
            );
    }


    private void Start()
    {
        Debug.Log(
            "--- INICIANDO JUEGO DE LA OCA ---"
        );

        if (contenedorCasillas == null)
        {
            Debug.LogError(
                "[GameManager] No se asignó el Contenedor de Casillas."
            );

            return;
        }

        ObtenerRutaDesdeContenedor();

        InicializarJugadores();


        if (tableroManager == null)
        {
            tableroManager =
                GetComponent<TableroManager>();
        }


        if (tableroManager == null)
        {
            Debug.LogError(
                "[GameManager] No se encontró TableroManager."
            );

            return;
        }


        tableroManager.InicializarTablero(
            rutaPosiciones.Length
        );

        tablero =
            tableroManager.ListaCasilleros;


        if (UIPreguntas.Instancia != null)
        {
            UIPreguntas.Instancia
                .RespuestaProcesada +=
                ProcesarRespuestaPregunta;
        }


        // -------------------------------------------------
        // BUSCAR EL MINIJUEGO AUTOMÁTICAMENTE
        // -------------------------------------------------

        if (miniJuegoManager == null)
        {
            miniJuegoManager =
                FindObjectOfType<MiniJuegoManager>();
        }


        // -------------------------------------------------
        // COLOCAR FICHAS
        // -------------------------------------------------

        for (
            int i = 0;
            i < jugadores.Count;
            i++
        )
        {
            if (
                i < fichasVisuales3D.Length &&
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


    private void ProcesarRespuestaPregunta(
        Jugador jugador,
        bool esCorrecta
    )
    {
        if (jugador == null)
        {
            Debug.LogWarning(
                "[GameManager] Jugador nulo al procesar respuesta."
            );

            return;
        }


        if (esCorrecta)
        {
            Debug.Log(
                $"[RESPUESTA] {jugador.Nombre} respondió correctamente."
            );
        }
        else
        {
            Debug.Log(
                $"[RESPUESTA] {jugador.Nombre} respondió incorrectamente."
            );
        }


        if (UIJuego.Instancia != null)
        {
            UIJuego.Instancia.MostrarResultado(
                jugador,
                esCorrecta
            );
        }


        ReanudarTurno();
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
        jugadores.Clear();


        string[] nombres =
        {
            "Jugador Rojo",
            "Jugador Azul",
            "Jugador Verde",
            "Jugador Amarillo"
        };


        int cantidadJugadores =
            SesionPartida.CantidadJugadores;


        for (
            int i = 0;
            i < fichasVisuales3D.Length;
            i++
        )
        {
            if (i < cantidadJugadores)
            {
                if (
                    fichasVisuales3D[i] != null
                )
                {
                    fichasVisuales3D[i]
                        .gameObject
                        .SetActive(true);
                }


                jugadores.Add(
                    new Jugador(
                        i + 1,
                        nombres[i],
                        configuracion
                    )
                );
            }
            else
            {
                if (
                    fichasVisuales3D[i] != null
                )
                {
                    fichasVisuales3D[i]
                        .gameObject
                        .SetActive(false);
                }
            }
        }
    }


    public Jugador ObtenerJugador(
        int indice
    )
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


    public int ObtenerVueltasTotales()
    {
        return configuracion.VueltasTotales;
    }


    private void MostrarJugadorActual()
    {
        if (JuegoTerminado)
            return;


        Jugador jugador =
            jugadores[jugadorActual];


        if (
            camaraPrincipalScript != null &&
            jugadorActual >= 0 &&
            jugadorActual <
                fichasVisuales3D.Length &&
            fichasVisuales3D[jugadorActual] != null
        )
        {
            Transform nuevaFicha =
                fichasVisuales3D[jugadorActual]
                    .transform;


            camaraPrincipalScript.objetivo =
                nuevaFicha;


            camaraPrincipalScript.transform
                .position =
                nuevaFicha.position +
                camaraPrincipalScript.offset;
        }


        Debug.Log(
            $"[TURNO] {jugador.Nombre} | " +
            $"Vuelta: {jugador.RondaActual}/" +
            $"{configuracion.VueltasTotales} | " +
            $"Correctas: {jugador.RespuestasCorrectas} | " +
            $"Objetivo: {jugador.ObtenerObjetivoDeRonda()} | " +
            $"Casillero: {jugador.PosicionActualId + 1}"
        );


        if (UIJuego.Instancia != null)
        {
            UIJuego.Instancia
                .ActualizarInterfaz();
        }
    }


    public void TirarDado()
    {
        if (
            JuegoTerminado ||
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
            EsperandoRespuesta = true;

            UIPreguntas.Instancia
                .MostrarPregunta(jugador);

            return;
        }


        if (jugador.EstaPenalizado())
        {
            jugador.CumplirPenalizacion();

            SiguienteTurno();

            return;
        }


        int resultado =
            Random.Range(1, 7);


        jugador.RegistrarTirada(
            resultado
        );


        EsperandoRespuesta = true;


        Transform fichaObjetivo = null;


        if (
            jugadorActual <
                fichasVisuales3D.Length &&
            fichasVisuales3D[jugadorActual] != null
        )
        {
            fichaObjetivo =
                fichasVisuales3D[jugadorActual]
                    .transform;
        }


        if (dadoVisual != null)
        {
            dadoVisual.Lanzar(
                resultado,
                fichaObjetivo,
                () =>
                {
                    EsperandoRespuesta =
                        false;

                    ContinuarMovimiento(
                        jugador,
                        resultado
                    );
                }
            );
        }
        else
        {
            EsperandoRespuesta =
                false;

            ContinuarMovimiento(
                jugador,
                resultado
            );
        }
    }


    private void ContinuarMovimiento(
        Jugador jugador,
        int resultado
    )
    {
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


            MoverFichaAIndiceConCallback(
                jugadorActual,
                ultimaPosicion,
                () =>
                {
                    StartCoroutine(
                        EsperarYContinuarVuelta(
                            jugador
                        )
                    );
                }
            );

            return;
        }


        jugador.Moverse(
            resultado,
            tablero.Count
        );


        MoverFichaAIndiceConCallback(
            jugadorActual,
            jugador.PosicionActualId,
            () =>
            {
                StartCoroutine(
                    EsperarYProcesarCasillero(
                        jugador
                    )
                );
            }
        );
    }


    private System.Collections.IEnumerator
        EsperarYProcesarCasillero(
            Jugador jugador
        )
    {
        yield return new WaitForSeconds(
            1.0f
        );


        CasilleroBase casilleroActual =
            tablero.Find(
                c =>
                    c.Id ==
                    jugador.PosicionActualId
            );


        if (casilleroActual != null)
        {
            casilleroActual
                .EjecutarEfecto(
                    jugador
                );


            // =============================================
            // EFECTO ESPECIAL
            // =============================================

            if (
                casilleroActual.Tipo ==
                TipoCasillero.EfectoEspecial
            )
            {
                if (
                    jugador.PosicionActualId < 0
                )
                {
                    jugador.EstablecerPosicion(
                        0
                    );
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


            // =============================================
            // PREGUNTA
            // =============================================

            if (
                casilleroActual.Tipo ==
                TipoCasillero.Pregunta
            )
            {
                EsperandoRespuesta =
                    true;


                if (
                    UIPreguntas.Instancia != null
                )
                {
                    UIPreguntas.Instancia
                        .MostrarPregunta(
                            jugador
                        );
                }
                else
                {
                    Debug.LogError(
                        "[GameManager] No existe UIPreguntas."
                    );

                    EsperandoRespuesta =
                        false;

                    SiguienteTurno();
                }

                yield break;
            }


            // =============================================
            // NUEVO: MINIJUEGO PONG
            // =============================================

            if (
                casilleroActual.Tipo ==
                TipoCasillero.Juego
            )
            {
                EsperandoRespuesta =
                    true;


                if (
                    miniJuegoManager == null
                )
                {
                    miniJuegoManager =
                        FindObjectOfType<
                            MiniJuegoManager
                        >();
                }


                if (
                    miniJuegoManager != null
                )
                {
                    miniJuegoManager
                        .IniciarMinijuego(
                            jugador
                        );
                }
                else
                {
                    Debug.LogError(
                        "[GameManager] " +
                        "No se encontró MiniJuegoManager."
                    );

                    EsperandoRespuesta =
                        false;

                    SiguienteTurno();
                }


                yield break;
            }
        }


        SiguienteTurno();
    }


    private System.Collections.IEnumerator
        EsperarYContinuarVuelta(
            Jugador jugador
        )
    {
        yield return new WaitForSeconds(
            1.0f
        );


        ResultadoVuelta resultado =
            gestorVueltas
                .ResolverLlegadaMeta(
                    jugador
                );


        if (
            resultado ==
            ResultadoVuelta.NecesitaPreguntas
        )
        {
            SiguienteTurno();

            yield break;
        }


        if (
            resultado ==
            ResultadoVuelta.Gano
        )
        {
            FinalizarJuego(
                jugador
            );

            yield break;
        }


        MoverFichaAIndice(
            jugadorActual,
            jugador.PosicionActualId
        );


        SiguienteTurno();
    }


    private void MoverFichaAIndice(
        int indiceJugador,
        int indiceCasillero
    )
    {
        MoverFichaAIndiceConCallback(
            indiceJugador,
            indiceCasillero,
            null
        );
    }


    private void MoverFichaAIndiceConCallback(
        int indiceJugador,
        int indiceCasillero,
        System.Action alTerminar
    )
    {
        if (
            indiceJugador < 0 ||
            indiceJugador >=
                fichasVisuales3D.Length ||
            fichasVisuales3D[indiceJugador] == null ||
            rutaPosiciones == null ||
            rutaPosiciones.Length == 0
        )
        {
            alTerminar?.Invoke();
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
                rutaPosiciones,
                alTerminar
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

        VictoriaManager.NombreGanador =
            ganador.Nombre;

        SceneManager.LoadScene(
            "Victoria"
        );
    }


    public void ReanudarTurno()
    {
        Debug.Log(
            "[FLUJO] ReanudarTurno fue llamado."
        );


        if (JuegoTerminado)
            return;


        EsperandoRespuesta =
            false;


        Jugador jugador =
            jugadores[jugadorActual];


        Debug.Log(
            $"[REANUDAR] {jugador.Nombre} | " +
            $"Ronda: {jugador.RondaActual} | " +
            $"Correctas: {jugador.RespuestasCorrectas} | " +
            $"Debe repetir: {jugador.DebeRepetirPreguntas}"
        );


        bool estaEnLaUltimaCasilla =
            jugador.PosicionActualId >=
            tablero.Count - 1;


        if (
            jugador.DebeRepetirPreguntas ||
            estaEnLaUltimaCasilla
        )
        {
            ResultadoVuelta resultado =
                gestorVueltas
                    .ResolverLlegadaMeta(
                        jugador
                    );


            if (
                resultado ==
                ResultadoVuelta.NecesitaPreguntas
            )
            {
                SiguienteTurno();
                return;
            }


            if (
                resultado ==
                ResultadoVuelta.Gano
            )
            {
                FinalizarJuego(
                    jugador
                );

                return;
            }


            MoverFichaAIndice(
                jugadorActual,
                jugador.PosicionActualId
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