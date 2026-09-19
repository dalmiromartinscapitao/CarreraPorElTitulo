using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;


    // =========================================================
    // DATOS DEL JUEGO
    // =========================================================

    private List<CasilleroBase> tablero =
        new List<CasilleroBase>();


    private List<Jugador> jugadores =
        new List<Jugador>();


    private int jugadorActual = 0;


    // =========================================================
    // ESTADOS
    // =========================================================

    public bool EsperandoRespuesta { get; private set; }
        = false;


    public bool JuegoTerminado { get; private set; }
        = false;


    public Jugador Ganador { get; private set; }


    // =========================================================
    // FICHAS
    // =========================================================

    [Header("Fichas de Jugadores (Asignar 4)")]
    public FichaVisual[] fichasVisuales3D;


    // =========================================================
    // TABLERO
    // =========================================================

    [Header("Configuración del Tablero")]
    public Transform contenedorCasillas;


    private Vector3[] rutaPosiciones;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instancia = this;
    }


    // =========================================================
    // START
    // =========================================================

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


        // -----------------------------------------------------
        // COLOCAR TODAS LAS FICHAS EN EL CASILLERO 1
        // -----------------------------------------------------

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


    // =========================================================
    // OBTENER RUTA
    // =========================================================

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


    // =========================================================
    // INICIALIZAR JUGADORES
    // =========================================================

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


    // =========================================================
    // INICIALIZAR TABLERO
    // =========================================================

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


    // =========================================================
    // MOSTRAR JUGADOR ACTUAL
    // =========================================================

    private void MostrarJugadorActual()
    {
        if (JuegoTerminado)
            return;


        Jugador jugador =
            jugadores[jugadorActual];


        Debug.Log(
            $"[TURNO] {jugador.Nombre} | " +
            $"Vuelta: {jugador.RondaActual}/3 | " +
            $"Correctas: {jugador.RespuestasCorrectas} | " +
            $"Objetivo: {jugador.ObtenerObjetivoDeRonda()} | " +
            $"Casillero: {jugador.PosicionActualId + 1}"
        );
    }


    // =========================================================
    // TIRAR DADO
    // =========================================================

    public void TirarDado()
    {
        // -----------------------------------------------------
        // EL JUEGO YA TERMINÓ
        // -----------------------------------------------------

        if (JuegoTerminado)
            return;


        // -----------------------------------------------------
        // HAY UNA PREGUNTA ABIERTA
        // -----------------------------------------------------

        if (
            contenedorCasillas == null ||
            EsperandoRespuesta
        )
        {
            return;
        }


        Jugador jugador =
            jugadores[jugadorActual];


        // -----------------------------------------------------
        // SI ESTÁ BLOQUEADO EN EL CASILLERO 28,
        // NO TIRA DADO: TIENE QUE RESPONDER.
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // LANZAR DADO
        // -----------------------------------------------------

        int resultado =
            jugador.LanzarDado();


        // Penalización
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


        // =====================================================
        // ¿LLEGÓ O PASÓ EL CASILLERO 28?
        // =====================================================

        if (
            posicionCalculada >=
            ultimaPosicion
        )
        {
            // Lo colocamos exactamente en el casillero 28.
            jugador.EstablecerPosicion(
                ultimaPosicion
            );


            // La ficha también va al 28.
            MoverFichaAIndice(
                jugadorActual,
                ultimaPosicion
            );


            Debug.Log(
                $"[VUELTA] {jugador.Nombre} llegó " +
                "al casillero 28."
            );


            // -------------------------------------------------
            // COMPROBAR SI TIENE LAS RESPUESTAS NECESARIAS
            // -------------------------------------------------

            if (
                !jugador.TieneRespuestasNecesarias()
            )
            {
                // Queda bloqueado en el casillero 28.
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


            // -------------------------------------------------
            // TIENE LAS RESPUESTAS NECESARIAS
            // -------------------------------------------------

            int rondaAntes =
                jugador.RondaActual;


            bool completo =
                jugador.IntentarCompletarVuelta();


            if (!completo)
            {
                SiguienteTurno();
                return;
            }


            // -------------------------------------------------
            // TERCERA VUELTA → VICTORIA
            // -------------------------------------------------

            if (rondaAntes == 3)
            {
                FinalizarJuego(jugador);
                return;
            }


            // -------------------------------------------------
            // NUEVA VUELTA
            // -------------------------------------------------

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


        // =====================================================
        // MOVIMIENTO NORMAL
        // =====================================================

        jugador.Moverse(
            resultado,
            tablero.Count
        );


        // -----------------------------------------------------
        // MOVER FICHA VISUAL
        // -----------------------------------------------------

        MoverFichaAIndice(
            jugadorActual,
            jugador.PosicionActualId
        );


        // =====================================================
        // BUSCAR CASILLERO
        // =====================================================

        CasilleroBase casilleroActual =
            tablero.Find(
                c =>
                    c.Id ==
                    jugador.PosicionActualId
            );


        if (casilleroActual != null)
        {
            // -------------------------------------------------
            // EJECUTAR EFECTO
            // -------------------------------------------------

            casilleroActual.EjecutarEfecto(
                jugador
            );


            // -------------------------------------------------
            // IMPORTANTE:
            // Algunos efectos especiales cambian la posición
            // lógica del jugador.
            //
            // Por eso volvemos a sincronizar la ficha visual
            // después del efecto.
            // -------------------------------------------------

            if (
                casilleroActual.Tipo ==
                TipoCasillero.EfectoEspecial
            )
            {
                // Evitar que un efecto deje al jugador
                // fuera de los límites del tablero.

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


            // -------------------------------------------------
            // CASILLERO DE PREGUNTA
            // -------------------------------------------------

            if (
                casilleroActual.Tipo ==
                TipoCasillero.Pregunta
            )
            {
                EsperandoRespuesta = true;

                return;
            }
        }


        // -----------------------------------------------------
        // TERMINA EL TURNO
        // -----------------------------------------------------

        SiguienteTurno();
    }


    // =========================================================
    // MOVER FICHA A UN ÍNDICE ESPECÍFICO
    // =========================================================

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


        // Seguridad para no salir del tablero.
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


    // =========================================================
    // FINALIZAR JUEGO
    // =========================================================

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


    // =========================================================
    // CUANDO TERMINA UNA PREGUNTA
    // =========================================================

    public void ReanudarTurno()
    {
        if (JuegoTerminado)
            return;


        EsperandoRespuesta = false;


        Jugador jugador =
            jugadores[jugadorActual];


        // =====================================================
        // SI ESTABA BLOQUEADO EN EL CASILLERO 28
        // =====================================================

        if (jugador.DebeRepetirPreguntas)
        {
            // -------------------------------------------------
            // TODAVÍA NO TIENE SUFICIENTES RESPUESTAS
            // -------------------------------------------------

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


                // Pasa el turno al siguiente jugador.
                SiguienteTurno();

                return;
            }


            // -------------------------------------------------
            // YA TIENE LAS RESPUESTAS NECESARIAS
            // -------------------------------------------------

            int rondaAntes =
                jugador.RondaActual;


            bool completo =
                jugador.IntentarCompletarVuelta();


            if (!completo)
            {
                SiguienteTurno();
                return;
            }


            // -------------------------------------------------
            // TERCERA VUELTA → VICTORIA
            // -------------------------------------------------

            if (rondaAntes == 3)
            {
                FinalizarJuego(jugador);
                return;
            }


            // -------------------------------------------------
            // PASÓ A LA SIGUIENTE VUELTA
            // -------------------------------------------------

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


        // =====================================================
        // SIGUIENTE JUGADOR
        // =====================================================

        SiguienteTurno();
    }


    // =========================================================
    // SIGUIENTE TURNO
    // =========================================================

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