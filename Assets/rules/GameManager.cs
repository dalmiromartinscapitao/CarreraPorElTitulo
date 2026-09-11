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

    // Referencia al objeto 3D de la ficha
    public FichaVisual fichaVisual3D;

    private void Start()
    {
        Debug.Log("--- INICIANDO JUEGO DE LA OCA ---");

        // Crear jugadores
        InicializarJugadores();

        // Crear tablero
        InicializarTablero();

        // Mostrar quién empieza
        MostrarJugadorActual();
    }

    private void InicializarJugadores()
    {
        jugadores.Add(new Jugador(1, "Santi"));
        jugadores.Add(new Jugador(2, "Aye"));

        Debug.Log($"Se crearon {jugadores.Count} jugadores.");
    }

    private void InicializarTablero()
    {
        tablero.Add(new CasilleroNormal(0, new List<int> { 1 }));
        tablero.Add(new CasilleroNormal(1, new List<int> { 2 }));
        tablero.Add(new CasilleroPregunta(2, new List<int> { 3 }));
        tablero.Add(new CasilleroEspecial(3, new List<int> { 4 }));
        tablero.Add(new CasilleroNormal(4, new List<int>()));
    }

    private void MostrarJugadorActual()
    {
        Jugador jugador = jugadores[jugadorActual];

        Debug.Log($"Es el turno de: {jugador.Nombre}");
    }

    // Este método será llamado por el botón "TIRAR DADO"
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
            $"{jugador.Nombre} terminó su movimiento en el casillero {jugador.PosicionActualId}"
        );

        // Por ahora, si tenemos una ficha visual conectada,
        // la actualizamos
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
                $"{jugador.Nombre} llegó a la posición " +
                $"{jugador.PosicionActualId}, pero ese casillero " +
                $"todavía no está creado en el tablero lógico."
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