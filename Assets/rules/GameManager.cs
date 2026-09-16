using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    private List<CasilleroBase> tablero = new List<CasilleroBase>();
    private List<Jugador> jugadores = new List<Jugador>();
    private int jugadorActual = 0;
    
    public bool EsperandoRespuesta { get; private set; } = false;

    [Header("Fichas de Jugadores (Asignar 4)")]
    public FichaVisual[] fichasVisuales3D; // Cambiado a un Arreglo (Array)

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
        
        if (contenedorCasillas == null) return;

        ObtenerRutaDesdeContenedor();
        InicializarJugadores();
        InicializarTablero();

        // Colocar las 4 fichas en la salida sumando el offset de cada una
        for (int i = 0; i < fichasVisuales3D.Length; i++)
        {
            if (fichasVisuales3D[i] != null && rutaPosiciones.Length > 0)
            {
                fichasVisuales3D[i].transform.position = rutaPosiciones[0] + fichasVisuales3D[i].offsetFicha;
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
            rutaPosiciones[i] = contenedorCasillas.GetChild(i).position;
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
            List<int> siguientesIds = (i == cantidadCasilleros - 1) ? new List<int>() : new List<int> { siguiente };

            if (i == 0 || i == cantidadCasilleros - 1)
                tablero.Add(new CasilleroNormal(i, siguientesIds));
            else if (i % 3 == 0) tablero.Add(new CasilleroPregunta(i, siguientesIds));
            else if (i % 5 == 0) tablero.Add(new CasilleroEspecial(i, siguientesIds));
            else tablero.Add(new CasilleroNormal(i, siguientesIds));
        }
    }

    private void MostrarJugadorActual()
    {
        Jugador jugador = jugadores[jugadorActual];
        Debug.Log($"Es el turno de: {jugador.Nombre}");
    }

    public void TirarDado()
    {
        if (contenedorCasillas == null || EsperandoRespuesta) return;

        Jugador jugador = jugadores[jugadorActual];
        int resultado = jugador.LanzarDado();

        if (resultado <= 0)
        {
            SiguienteTurno();
            return;
        }

        int posicionAnterior = jugador.PosicionActualId;
        jugador.Moverse(resultado, tablero.Count);

        if (posicionAnterior + resultado >= tablero.Count)
        {
            EvaluarCambioDeRondaActual(); 
        }

        // Mover solo la ficha visual que corresponde al jugador de este turno
        if (jugadorActual < fichasVisuales3D.Length && fichasVisuales3D[jugadorActual] != null)
        {
            fichasVisuales3D[jugadorActual].MoverAdelante(resultado, rutaPosiciones);
        }

        CasilleroBase casilleroActual = tablero.Find(c => c.Id == jugador.PosicionActualId);
        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);
            
            if (casilleroActual.Tipo == TipoCasillero.Pregunta)
            {
                EsperandoRespuesta = true; 
                return; 
            }
        }

        SiguienteTurno();
    }

    public void ReanudarTurno()
    {
        EsperandoRespuesta = false;
        SiguienteTurno();
    }

    public void EvaluarCambioDeRondaActual()
    {
        if (jugadores.Count == 0) return;
        Jugador jugador = jugadores[jugadorActual];
        if (jugador.IntentarAvanzarDeRonda())
            Debug.Log($"[GameManager] ¡{jugador.Nombre} avanzó a la Ronda {jugador.RondaActual}!");
    }

    private void SiguienteTurno()
    {
        jugadorActual++;
        if (jugadorActual >= jugadores.Count)
        {
            jugadorActual = 0;
        }
        MostrarJugadorActual();
    }
}