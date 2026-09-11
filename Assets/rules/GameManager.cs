using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Lista lógica de casilleros
    private List<CasilleroBase> tablero = new List<CasilleroBase>();

    // Lista de jugadores
    private List<Jugador> jugadores = new List<Jugador>();

    // Indica qué jugador tiene el turno
    private int jugadorActual = 0;

    // Referencia a la ficha visual
    public FichaVisual fichaVisual3D;

    private void Start()
    {
        Debug.Log("--- INICIANDO JUEGO DE LA OCA ---");

        InicializarJugadores();
        InicializarTablero();

        MostrarJugadorActual();
    }

    // --------------------------------------------------
    // JUGADORES
    // --------------------------------------------------

    private void InicializarJugadores()
    {
        jugadores.Add(new Jugador(1, "Palo"));
        jugadores.Add(new Jugador(2, "Aye"));
        jugadores.Add(new Jugador(3, "Juan"));
        jugadores.Add(new Jugador(4, "Dal"));

        Debug.Log($"Se crearon {jugadores.Count} jugadores.");
    }

    // --------------------------------------------------
    // TABLERO
    // --------------------------------------------------

    private void InicializarTablero()
    {
        // Creamos los 28 casilleros.
        // Los IDs coinciden con las posiciones del tablero visual:
        // posición 0 = primer casillero
        // posición 1 = segundo casillero
        // ...
        // posición 27 = casillero 28

        for (int i = 0; i < 28; i++)
        {
            int siguiente = i + 1;

            // El último casillero no tiene siguiente
            if (i == 27)
            {
                tablero.Add(
                    new CasilleroNormal(i, new List<int>())
                );
            }
            else
            {
                tablero.Add(
                    new CasilleroNormal(i, new List<int> { siguiente })
                );
            }
        }

        Debug.Log($"Se crearon {tablero.Count} casilleros.");
    }

    // --------------------------------------------------
    // TURNOS
    // --------------------------------------------------

    private void MostrarJugadorActual()
    {
        Jugador jugador = jugadores[jugadorActual];

        Debug.Log($"Es el turno de: {jugador.Nombre}");
    }

    public void TirarDado()
    {
        Jugador jugador = jugadores[jugadorActual];

        Debug.Log($"Es el turno de {jugador.Nombre}");

        int resultado = jugador.LanzarDado();

        Debug.Log($"{jugador.Nombre} sacó {resultado}");

        // Si obtuvo 0, significa que estaba penalizado
        if (resultado <= 0)
        {
            SiguienteTurno();
            return;
        }

        // Mover al jugador
        jugador.Moverse(resultado);

        Debug.Log(
            $"{jugador.Nombre} llegó a la posición {jugador.PosicionActualId}"
        );

        // Actualizar visualmente la ficha
        if (fichaVisual3D != null)
        {
            fichaVisual3D.ActualizarPosicionVisual(
                jugador.PosicionActualId
            );
        }

        // Buscar el casillero donde cayó
        CasilleroBase casilleroActual =
            tablero.Find(c => c.Id == jugador.PosicionActualId);

        if (casilleroActual != null)
        {
            Debug.Log(
                $"{jugador.Nombre} cayó en el casillero " +
                $"{casilleroActual.Id} " +
                $"(Tipo: {casilleroActual.Tipo})"
            );

            // Ejecutar el efecto del casillero
            casilleroActual.EjecutarEfecto(jugador);
        }
        else
        {
            Debug.LogWarning(
                $"No se encontró el casillero " +
                $"{jugador.PosicionActualId}."
            );
        }

        // Termina el turno
        SiguienteTurno();
    }

    private void SiguienteTurno()
    {
        jugadorActual++;

        // Si llegamos al final de la lista,
        // volvemos al primer jugador
        if (jugadorActual >= jugadores.Count)
        {
            jugadorActual = 0;
        }

        MostrarJugadorActual();
    }
}