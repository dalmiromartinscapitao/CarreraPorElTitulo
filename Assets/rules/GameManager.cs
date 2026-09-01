using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<CasilleroBase> tablero = new List<CasilleroBase>();
    private Jugador jugadorPrueba;

    private void Start()
    {
        Debug.Log("--- INICIANDO SIMULACIÓN DEL JUEGO DE LA OCA ---");

        // 1. Instanciar Jugador
        jugadorPrueba = new Jugador(1, "Santi");

        // 2. Construir un tablero básico de prueba (IDs del 0 al 4)
        tablero.Add(new CasilleroNormal(0, new List<int> { 1 }));
        tablero.Add(new CasilleroNormal(1, new List<int> { 2 }));
        tablero.Add(new CasilleroPregunta(2, new List<int> { 3 }));
        tablero.Add(new CasilleroEspecial(3, new List<int> { 4 }));
        tablero.Add(new CasilleroNormal(4, new List<int>()));

        // 3. Simular un Turno
        EjecutarTurnoPrueba();
    }

    private void EjecutarTurnoPrueba()
    {
        // A. Lanzar Dado
        int dado = jugadorPrueba.LanzarDado();

        // B. Mover al jugador según el dado
        jugadorPrueba.Moverse(dado);

        // C. Buscar la casilla en la que cayó
        CasilleroBase casilleroActual = tablero.Find(c => c.Id == jugadorPrueba.PosicionActualId);

        if (casilleroActual != null)
        {
            Debug.Log($"El jugador cayó en la Casilla ID: {casilleroActual.Id} (Tipo: {casilleroActual.Tipo})");
            
            // D. Ejecutar la lógica de la casilla
            casilleroActual.EjecutarEfecto(jugadorPrueba);
        }
        else
        {
            Debug.LogWarning($"El jugador avanzó a la posición {jugadorPrueba.PosicionActualId}, pero supera el límite del tablero.");
        }
    }
}