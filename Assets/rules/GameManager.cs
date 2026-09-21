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
    public bool JuegoTerminado { get; private set; } = false;
    public Jugador Ganador { get; private set; }

    [Header("Fichas de Jugadores (Asignar 4)")]
    public FichaVisual[] fichasVisuales3D;

    [Header("Script de la Cámara Principal")]
    public CamaraSigue camaraPrincipalScript; // Arrastra aquí la Main Camera que tiene el script CamaraSigue

    [Header("Configuración del Tablero")]
    public Transform contenedorCasillas;

    private Vector3[] rutaPosiciones;

    private ConfiguracionPartida configuracion;

    private void Awake(){
        if (Instancia != null && Instancia != this){
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        configuracion = ConfiguracionPartida.Crear(SesionPartida.ModoSeleccionado);
    }

    private void Start(){
        Debug.Log("--- INICIANDO JUEGO DE LA OCA ---");

        if (contenedorCasillas == null){
            Debug.LogError("[GameManager] No se asignó el " + "Contenedor de Casillas.");
            return;
        }

        ObtenerRutaDesdeContenedor();
        InicializarJugadores();
        InicializarTablero();

        if (UIPreguntas.Instancia != null){
            UIPreguntas.Instancia.RespuestaProcesada += ProcesarRespuestaPregunta;
        }

        for (int i = 0;
        i < fichasVisuales3D.Length;
        i++)
        {
        if (fichasVisuales3D[i] != null &&
            rutaPosiciones.Length > 0)
        {
            fichasVisuales3D[i].SincronizarPosicion(0,rutaPosiciones);
        }
        }

        MostrarJugadorActual();
    }

    private void ProcesarRespuestaPregunta(Jugador jugador,bool esCorrecta){

        if (jugador == null){
            Debug.LogWarning("[GameManager] Jugador nulo al procesar respuesta.");

            return;
        }

        if (esCorrecta){
            Debug.Log($"[RESPUESTA] {jugador.Nombre} " + "respondió correctamente.");
        }
        else{
            Debug.Log($"[RESPUESTA] {jugador.Nombre} " +"respondió incorrectamente.");
        }

        if (UIJuego.Instancia != null){
            UIJuego.Instancia.MostrarResultado(jugador,esCorrecta);
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
        jugadores.Add(new Jugador(1, "Jugador Rojo", configuracion));
        jugadores.Add(new Jugador(2, "Jugador Azul", configuracion));
        jugadores.Add(new Jugador(3, "Jugador Verde", configuracion));
        jugadores.Add(new Jugador(4, "Jugador Amarillo", configuracion));
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

        // Asignamos directamente el objetivo a la cámara de forma segura
        if (
            camaraPrincipalScript != null &&
            jugadorActual >= 0 &&
            jugadorActual < fichasVisuales3D.Length &&
            fichasVisuales3D[jugadorActual] != null
        )
        {
            Transform nuevaFicha = fichasVisuales3D[jugadorActual].transform;
            camaraPrincipalScript.objetivo = nuevaFicha;
            
            // Coloca la cámara instantáneamente sobre el jugador al cambiar de turno
            camaraPrincipalScript.transform.position = nuevaFicha.position + camaraPrincipalScript.offset;
        }

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

            MoverFichaAIndiceConCallback(
                jugadorActual,
                ultimaPosicion,
                () => {
                    StartCoroutine(EsperarYContinuarVuelta(jugador));
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
            () => {
                StartCoroutine(EsperarYProcesarCasillero(jugador));
            }
        );
    }

    // =========================================================
    // CORRUTINAS DE PAUSA DESPUÉS DEL MOVIMIENTO
    // =========================================================

    private System.Collections.IEnumerator EsperarYProcesarCasillero(Jugador jugador)
    {
        // Espera 1 segundo antes de ejecutar efectos o cambiar de turno
        yield return new WaitForSeconds(1.0f);

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
                yield break;
            }
        }

        SiguienteTurno();
    }

    private System.Collections.IEnumerator EsperarYContinuarVuelta(Jugador jugador)
    {
        // Espera 1 segundo al llegar al final
        yield return new WaitForSeconds(1.0f);

        Debug.Log(
        $"[FINAL DE VUELTA] {jugador.Nombre} | " +
        $"Ronda: {jugador.RondaActual} | " +
        $"Correctas: {jugador.RespuestasCorrectas} | " +
        $"Objetivo: {jugador.ObtenerObjetivoDeRonda()} | " +
        $"Debe repetir: {jugador.DebeRepetirPreguntas}"
        );

        if (
            !jugador.TieneRespuestasNecesarias()
        )
        {
            jugador.IntentarCompletarVuelta();
            SiguienteTurno();
            yield break;
        }

        int rondaAntes =
            jugador.RondaActual;
        bool completo =
            jugador.IntentarCompletarVuelta();
        
        if (!completo)
        {
            SiguienteTurno();
            yield break;
        }

        if (configuracion.EsUltimaVuelta(rondaAntes))
        {
            FinalizarJuego(jugador);
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
        MoverFichaAIndiceConCallback(indiceJugador, indiceCasillero, null);
    }

    private void MoverFichaAIndiceConCallback(
        int indiceJugador,
        int indiceCasillero,
        System.Action alTerminar
    )
    {
        if (
            indiceJugador < 0 ||
            indiceJugador >= fichasVisuales3D.Length ||
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

    public void ReanudarTurno(){

        Debug.Log("[FLUJO] ReanudarTurno fue llamado.");


        if (JuegoTerminado){
        return;
        }

        EsperandoRespuesta = false;

        Jugador jugador = jugadores[jugadorActual];

        Debug.Log($"[REANUDAR] {jugador.Nombre} | " +
        $"Ronda: {jugador.RondaActual} | " +
        $"Correctas: {jugador.RespuestasCorrectas} | " +
        $"Debe repetir: {jugador.DebeRepetirPreguntas}");
    
        bool estaEnLaUltimaCasilla = jugador.PosicionActualId >= tablero.Count - 1;

        if (jugador.DebeRepetirPreguntas || estaEnLaUltimaCasilla){
            if (!jugador.TieneRespuestasNecesarias()){
            jugador.IntentarCompletarVuelta();
            SiguienteTurno();
            return;
            }

            int rondaAntes = jugador.RondaActual;

            bool completo = jugador.IntentarCompletarVuelta();

            if (!completo){
            SiguienteTurno();
            return;
            }

            if (configuracion.EsUltimaVuelta(rondaAntes)){
            FinalizarJuego(jugador);
            return;
            }

            MoverFichaAIndice(jugadorActual,jugador.PosicionActualId);
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
    //hola
}
