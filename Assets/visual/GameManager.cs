using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private List<CasilleroBase> tablero = new List<CasilleroBase>();
    private List<Jugador> jugadores = new List<Jugador>();
    private int jugadorActual = 0;
    private GestorVueltas gestorVueltas;

    public static GameManager Instancia;
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

    private bool EsModoRed => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        configuracion = ConfiguracionPartida.Crear(SesionPartida.ModoSeleccionado);
        gestorVueltas = new GestorVueltas(configuracion);
    }

    private void Start()
    {
        if (contenedorCasillas == null)
        {
            Debug.LogError("[GameManager] No se asignó el Contenedor de Casillas.");
            return;
        }

        ObtenerRutaDesdeContenedor();

        if (tableroManager == null)
        {
            tableroManager = GetComponent<TableroManager>();
        }

        if (tableroManager != null)
        {
            tableroManager.InicializarTablero(rutaPosiciones.Length);
            tablero = tableroManager.ListaCasilleros;
        }

        if (miniJuegoManager == null)
        {
            miniJuegoManager = FindObjectOfType<MiniJuegoManager>();
        }

        // Si estamos en multijugador, desactivamos la lógica offline y esperamos órdenes de la red
        if (EsModoRed)
        {
            for (int i = 0; i < fichasVisuales3D.Length; i++)
            {
                if (fichasVisuales3D[i] != null)
                {
                    fichasVisuales3D[i].gameObject.SetActive(false);
                }
            }
            return;
        }

        // --- MODO OFFLINE (UN JUGADOR / LOCAL) ---
        InicializarJugadores();

        if (UIPreguntas.Instancia != null)
        {
            UIPreguntas.Instancia.RespuestaProcesada += ProcesarRespuestaPregunta;
        }

        for (int i = 0; i < jugadores.Count; i++)
        {
            if (i < fichasVisuales3D.Length && fichasVisuales3D[i] != null && rutaPosiciones.Length > 0)
            {
                fichasVisuales3D[i].SincronizarPosicion(0, rutaPosiciones);
            }
        }

        MostrarJugadorActual();
    }

    // --- MÉTODO LLAMADO POR LA RED PARA ACTUALIZAR EL ENTORNO 3D ---
    public void SincronizarEstadoDesdeRed(int indiceJugadorTurno, int[] posicionesCasilleros, int valorDado)
    {
        if (rutaPosiciones == null || rutaPosiciones.Length == 0) return;

        // 1. Activar fichas según cantidad real de jugadores en la sala y moverlas
        for (int i = 0; i < fichasVisuales3D.Length; i++)
        {
            if (fichasVisuales3D[i] == null) continue;

            if (i < posicionesCasilleros.Length)
            {
                if (!fichasVisuales3D[i].gameObject.activeSelf)
                {
                    fichasVisuales3D[i].gameObject.SetActive(true);
                    fichasVisuales3D[i].SincronizarPosicion(posicionesCasilleros[i], rutaPosiciones);
                }
                else
                {
                    MoverFichaAIndice(i, posicionesCasilleros[i]);
                }
            }
            else
            {
                fichasVisuales3D[i].gameObject.SetActive(false);
            }
        }

        // 2. Animar dado 3D si hubo tirada en este evento
        if (valorDado > 0 && dadoVisual != null && indiceJugadorTurno < fichasVisuales3D.Length)
        {
            dadoVisual.Lanzar(valorDado, fichasVisuales3D[indiceJugadorTurno].transform, null);
        }

        // 3. Enfocar cámara en el jugador que tiene el turno actual
        jugadorActual = indiceJugadorTurno;
        if (camaraPrincipalScript != null && indiceJugadorTurno >= 0 && indiceJugadorTurno < fichasVisuales3D.Length && fichasVisuales3D[indiceJugadorTurno] != null)
        {
            Transform fichaObjetivo = fichasVisuales3D[indiceJugadorTurno].transform;
            camaraPrincipalScript.objetivo = fichaObjetivo;
            camaraPrincipalScript.transform.position = fichaObjetivo.position + camaraPrincipalScript.offset;
        }
    }

    private void ObtenerRutaDesdeContenedor()
    {
        int cantidad = contenedorCasillas.childCount;
        rutaPosiciones = new Vector3[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            rutaPosiciones[i] = contenedorCasillas.GetChild(i).position;
        }
    }

    private void InicializarJugadores()
    {
        jugadores.Clear();

        string[] nombres = { "Jugador Rojo", "Jugador Azul", "Jugador Verde", "Jugador Amarillo" };
        int cantidadJugadores = SesionPartida.CantidadJugadores;

        for (int i = 0; i < fichasVisuales3D.Length; i++)
        {
            if (i < cantidadJugadores)
            {
                if (fichasVisuales3D[i] != null)
                {
                    fichasVisuales3D[i].gameObject.SetActive(true);
                }
                jugadores.Add(new Jugador(i + 1, nombres[i], configuracion));
            }
            else
            {
                if (fichasVisuales3D[i] != null)
                {
                    fichasVisuales3D[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void TirarDado()
    {
        if (EsModoRed)
        {
            NetworkLobbyServer servidor = FindObjectOfType<NetworkLobbyServer>();
            if (servidor != null)
            {
                servidor.SolicitarTirarDadoServerRpc();
            }
            return;
        }

        if (JuegoTerminado || contenedorCasillas == null || EsperandoRespuesta) return;

        Jugador jugador = jugadores[jugadorActual];

        if (jugador.DebeRepetirPreguntas)
        {
            EsperandoRespuesta = true;
            UIPreguntas.Instancia.MostrarPregunta(jugador);
            return;
        }

        if (jugador.EstaPenalizado())
        {
            jugador.CumplirPenalizacion();
            SiguienteTurno();
            return;
        }

        int resultado = Random.Range(1, 7);
        jugador.RegistrarTirada(resultado);
        EsperandoRespuesta = true;

        Transform fichaObjetivo = (jugadorActual < fichasVisuales3D.Length && fichasVisuales3D[jugadorActual] != null)
            ? fichasVisuales3D[jugadorActual].transform
            : null;

        if (dadoVisual != null)
        {
            dadoVisual.Lanzar(resultado, fichaObjetivo, () =>
            {
                EsperandoRespuesta = false;
                ContinuarMovimiento(jugador, resultado);
            });
        }
        else
        {
            EsperandoRespuesta = false;
            ContinuarMovimiento(jugador, resultado);
        }
    }

    private void ProcesarRespuestaPregunta(Jugador jugador, bool esCorrecta)
    {
        if (EsModoRed || jugador == null) return;

        if (UIJuego.Instancia != null)
        {
            UIJuego.Instancia.MostrarResultado(jugador, esCorrecta);
        }
        ReanudarTurno();
    }

    public Jugador ObtenerJugador(int indice)
    {
        if (indice < 0 || indice >= jugadores.Count) return null;
        return jugadores[indice];
    }

    public Jugador ObtenerJugadorActual()
    {
        if (jugadorActual < 0 || jugadorActual >= jugadores.Count) return null;
        return jugadores[jugadorActual];
    }

    public int ObtenerVueltasTotales()
    {
        return configuracion.VueltasTotales;
    }

    private void MostrarJugadorActual()
    {
        if (JuegoTerminado || EsModoRed) return;

        Jugador jugador = jugadores[jugadorActual];

        if (camaraPrincipalScript != null && jugadorActual >= 0 && jugadorActual < fichasVisuales3D.Length && fichasVisuales3D[jugadorActual] != null)
        {
            Transform nuevaFicha = fichasVisuales3D[jugadorActual].transform;
            camaraPrincipalScript.objetivo = nuevaFicha;
            camaraPrincipalScript.transform.position = nuevaFicha.position + camaraPrincipalScript.offset;
        }

        if (UIJuego.Instancia != null)
        {
            UIJuego.Instancia.ActualizarInterfaz();
        }
    }

    private void ContinuarMovimiento(Jugador jugador, int resultado)
    {
        int posicionAnterior = jugador.PosicionActualId;
        int ultimaPosicion = tablero.Count - 1;
        int posicionCalculada = posicionAnterior + resultado;

        if (posicionCalculada >= ultimaPosicion)
        {
            jugador.EstablecerPosicion(ultimaPosicion);
            MoverFichaAIndiceConCallback(jugadorActual, ultimaPosicion, () =>
            {
                StartCoroutine(EsperarYContinuarVuelta(jugador));
            });
            return;
        }

        jugador.Moverse(resultado, tablero.Count);
        MoverFichaAIndiceConCallback(jugadorActual, jugador.PosicionActualId, () =>
        {
            StartCoroutine(EsperarYProcesarCasillero(jugador));
        });
    }

    private System.Collections.IEnumerator EsperarYProcesarCasillero(Jugador jugador)
    {
        yield return new WaitForSeconds(1.0f);

        CasilleroBase casilleroActual = tablero.Find(c => c.Id == jugador.PosicionActualId);

        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);

            if (casilleroActual.Tipo == TipoCasillero.EfectoEspecial)
            {
                if (jugador.PosicionActualId < 0) jugador.EstablecerPosicion(0);
                if (jugador.PosicionActualId >= tablero.Count) jugador.EstablecerPosicion(tablero.Count - 1);

                MoverFichaAIndice(jugadorActual, jugador.PosicionActualId);

                CasilleroEspecial especial = casilleroActual as CasilleroEspecial;
                if (especial != null)
                {
                    if (especial.UltimoEfectoFuePenalizacion)
                    {
                        UIJuego.Instancia.MostrarPenalizacion(jugador);
                    }
                    else if (especial.UltimoMovimiento != 0)
                    {
                        UIJuego.Instancia.MostrarMovimientoEspecial(jugador, especial.UltimoMovimiento);
                    }
                }
            }

            if (casilleroActual.Tipo == TipoCasillero.Pregunta)
            {
                EsperandoRespuesta = true;
                if (UIPreguntas.Instancia != null)
                {
                    UIPreguntas.Instancia.MostrarPregunta(jugador);
                }
                else
                {
                    EsperandoRespuesta = false;
                    SiguienteTurno();
                }
                yield break;
            }

            if (casilleroActual.Tipo == TipoCasillero.Juego)
            {
                EsperandoRespuesta = true;
                if (miniJuegoManager == null) miniJuegoManager = FindObjectOfType<MiniJuegoManager>();

                if (miniJuegoManager != null)
                {
                    miniJuegoManager.IniciarMinijuego(jugador);
                }
                else
                {
                    EsperandoRespuesta = false;
                    SiguienteTurno();
                }
                yield break;
            }
        }

        SiguienteTurno();
    }

    private System.Collections.IEnumerator EsperarYContinuarVuelta(Jugador jugador)
    {
        yield return new WaitForSeconds(1.0f);

        ResultadoVuelta resultado = gestorVueltas.ResolverLlegadaMeta(jugador);

        if (resultado == ResultadoVuelta.NecesitaPreguntas)
        {
            SiguienteTurno();
            yield break;
        }

        if (resultado == ResultadoVuelta.Gano)
        {
            FinalizarJuego(jugador);
            yield break;
        }

        MoverFichaAIndice(jugadorActual, jugador.PosicionActualId);
        SiguienteTurno();
    }

    private void MoverFichaAIndice(int indiceJugador, int indiceCasillero)
    {
        MoverFichaAIndiceConCallback(indiceJugador, indiceCasillero, null);
    }

    private void MoverFichaAIndiceConCallback(int indiceJugador, int indiceCasillero, System.Action alTerminar)
    {
        if (indiceJugador < 0 || indiceJugador >= fichasVisuales3D.Length || fichasVisuales3D[indiceJugador] == null || rutaPosiciones == null || rutaPosiciones.Length == 0)
        {
            alTerminar?.Invoke();
            return;
        }

        indiceCasillero = Mathf.Clamp(indiceCasillero, 0, rutaPosiciones.Length - 1);
        fichasVisuales3D[indiceJugador].MoverAIndice(indiceCasillero, rutaPosiciones, alTerminar);
    }

    private void FinalizarJuego(Jugador ganador)
    {
        if (EsModoRed) return;

        JuegoTerminado = true;
        Ganador = ganador;
        ganador.MarcarComoGanador();
        EsperandoRespuesta = false;
        VictoriaManager.NombreGanador = ganador.Nombre;
        SceneManager.LoadScene("Victoria");
    }

    public void ReanudarTurno()
    {
        if (JuegoTerminado || EsModoRed) return;

        EsperandoRespuesta = false;
        Jugador jugador = jugadores[jugadorActual];

        bool estaEnLaUltimaCasilla = jugador.PosicionActualId >= tablero.Count - 1;

        if (jugador.DebeRepetirPreguntas || estaEnLaUltimaCasilla)
        {
            ResultadoVuelta resultado = gestorVueltas.ResolverLlegadaMeta(jugador);

            if (resultado == ResultadoVuelta.NecesitaPreguntas)
            {
                SiguienteTurno();
                return;
            }

            if (resultado == ResultadoVuelta.Gano)
            {
                FinalizarJuego(jugador);
                return;
            }

            MoverFichaAIndice(jugadorActual, jugador.PosicionActualId);
        }

        SiguienteTurno();
    }

    private void SiguienteTurno()
    {
        if (JuegoTerminado || EsModoRed) return;

        jugadorActual++;
        if (jugadorActual >= jugadores.Count)
        {
            jugadorActual = 0;
        }

        MostrarJugadorActual();
    }
}