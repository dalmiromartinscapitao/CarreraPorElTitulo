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

            // Determinar si es el último casillero para no asignarle un siguiente
            List<int> siguientesIds = (i == cantidadCasilleros - 1) 
                ? new List<int>() 
                : new List<int> { siguiente };

            // Forzar que el inicio y el final siempre sean normales
            if (i == 0 || i == cantidadCasilleros - 1)
            {
                tablero.Add(new CasilleroNormal(i, siguientesIds));
                continue;
            }

            // Distribuir los casilleros por el tablero
            if (i % 3 == 0) 
            {
                // Cada 3 espacios, una pregunta
                tablero.Add(new CasilleroPregunta(i, siguientesIds));
            }
            else if (i % 5 == 0) 
            {
                // Cada 5 espacios, un evento especial
                tablero.Add(new CasilleroEspecial(i, siguientesIds));
            }
            else 
            {
                // El resto son normales
                tablero.Add(new CasilleroNormal(i, siguientesIds));
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