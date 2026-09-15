using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<CasilleroBase> tablero = new List<CasilleroBase>();
    private List<Jugador> jugadores = new List<Jugador>();
    private int jugadorActual = 0;

    public FichaVisual fichaVisual3D;
    public Casilleros mapaCasilleros; 

    private void Start()
    {
        Debug.Log("--- INICIANDO JUEGO DE LA OCA ---");
        
        if (mapaCasilleros == null)
        {
            Debug.LogError("Error: Falta asignar 'Mapa Casilleros' en el Inspector.");
            return;
        }

        InicializarJugadores();
        InicializarTablero();

        if (fichaVisual3D != null && mapaCasilleros.posiciones.Length > 0)
        {
            fichaVisual3D.transform.position = mapaCasilleros.posiciones[0];
        }

        MostrarJugadorActual();
    }

    private void InicializarJugadores()
    {
        // Se deja un solo jugador para coincidir con la única ficha visual
        jugadores.Add(new Jugador(1, "Palo"));
    }

    private void InicializarTablero()
    {
        int cantidadCasilleros = mapaCasilleros.posiciones.Length;

        for (int i = 0; i < cantidadCasilleros; i++)
        {
            int siguiente = i + 1;

            if (i == cantidadCasilleros - 1)
            {
                tablero.Add(new CasilleroNormal(i, new List<int>()));
            }
            else
            {
                tablero.Add(new CasilleroNormal(i, new List<int> { siguiente }));
            }
        }
    }

    private void MostrarJugadorActual()
    {
        Jugador jugador = jugadores[jugadorActual];
        Debug.Log($"Es el turno de: {jugador.Nombre}");
    }

    public void TirarDado()
    {
        if (mapaCasilleros == null) return;

        Jugador jugador = jugadores[jugadorActual];
        int resultado = jugador.LanzarDado();

        if (resultado <= 0)
        {
            SiguienteTurno();
            return;
        }

        // Movimiento lógico
        jugador.Moverse(resultado, tablero.Count - 1);

        // Movimiento visual leyendo el arreglo de posiciones
        if (fichaVisual3D != null)
        {
            fichaVisual3D.MoverACasillero(jugador.PosicionActualId, mapaCasilleros.posiciones);
        }

        CasilleroBase casilleroActual = tablero.Find(c => c.Id == jugador.PosicionActualId);
        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);
        }

        SiguienteTurno();
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