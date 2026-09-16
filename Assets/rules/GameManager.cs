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

        // Guardamos la posición antes de moverse para calcular si da la vuelta
        int posicionAnterior = jugador.PosicionActualId;

        // Movimiento lógico
        jugador.Moverse(resultado, tablero.Count);

        // NUEVO: Evaluar si el jugador cruzó o cayó en la línea de meta (dio una vuelta completa)
        if (posicionAnterior + resultado >= tablero.Count)
        {
            Debug.Log($"[Tablero] ¡{jugador.Nombre} ha completado una vuelta al tablero!");
            EvaluarCambioDeRondaActual(); 
        }

        // Movimiento visual: Le pasamos los pasos del dado, NO la posición final
        if (fichaVisual3D != null)
        {
            fichaVisual3D.MoverAdelante(resultado, mapaCasilleros.posiciones);
        }

        CasilleroBase casilleroActual = tablero.Find(c => c.Id == jugador.PosicionActualId);
        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);
        }

        SiguienteTurno();
    }

    // NUEVO: Método público para evaluar si el jugador actual cumple con el >70% y pasa de ronda
    public void EvaluarCambioDeRondaActual()
    {
        if (jugadores.Count == 0) return;

        Jugador jugador = jugadores[jugadorActual];
        bool logroAvanzar = jugador.IntentarAvanzarDeRonda();

        if (logroAvanzar)
        {
            Debug.Log($"[GameManager] ¡{jugador.Nombre} ha avanzado con éxito a la Ronda {jugador.RondaActual}!");
            // Aquí puedes agregar lógica adicional de cambio de nivel o reinicio visual si lo deseas
        }
        else
        {
            Debug.Log($"[GameManager] {jugador.Nombre} aún no cumple con el porcentaje necesario para cambiar de ronda.");
        }
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