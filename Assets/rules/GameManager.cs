using System.Collections.Generic; //[cite: 2]
using UnityEngine; //[cite: 2]

public class GameManager : MonoBehaviour //[cite: 2]
{
    private List<CasilleroBase> tablero = new List<CasilleroBase>(); //[cite: 2]
    private Jugador jugadorPrueba; //[cite: 2]

    // Referencia al script del objeto 3D que se moverá visualmente
    public FichaVisual fichaVisual3D;

    private void Start() //[cite: 2]
    {
        Debug.Log("--- INICIANDO SIMULACIÓN DEL JUEGO DE LA OCA ---"); //[cite: 2]

        // 1. Instanciar Jugador
        jugadorPrueba = new Jugador(1, "Santi"); //[cite: 2]

        // 2. Construir un tablero básico de prueba (IDs del 0 al 4)
        tablero.Add(new CasilleroNormal(0, new List<int> { 1 })); //[cite: 2]
        tablero.Add(new CasilleroNormal(1, new List<int> { 2 })); //[cite: 2]
        tablero.Add(new CasilleroPregunta(2, new List<int> { 3 })); //[cite: 2]
        tablero.Add(new CasilleroEspecial(3, new List<int> { 4 })); //[cite: 2]
        tablero.Add(new CasilleroNormal(4, new List<int>())); //[cite: 2]

        // 3. Simular un Turno
        EjecutarTurnoPrueba(); //[cite: 2]
    }

    private void EjecutarTurnoPrueba() //[cite: 2]
    {
        // A. Lanzar Dado
        int dado = jugadorPrueba.LanzarDado(); //[cite: 2]

        // B. Mover al jugador según el dado
        jugadorPrueba.Moverse(dado); //[cite: 2]

        // NUEVO: Sincronizar la posición del modelo 3D con la posición lógica del jugador
        if (fichaVisual3D != null)
        {
            fichaVisual3D.ActualizarPosicionVisual(jugadorPrueba.PosicionActualId);
        }

        // C. Buscar la casilla en la que cayó
        CasilleroBase casilleroActual = tablero.Find(c => c.Id == jugadorPrueba.PosicionActualId); //[cite: 2]

        if (casilleroActual != null) //[cite: 2]
        {
            Debug.Log($"El jugador cayó en la Casilla ID: {casilleroActual.Id} (Tipo: {casilleroActual.Tipo})"); //[cite: 2]
            
            // D. Ejecutar la lógica de la casilla
            casilleroActual.EjecutarEfecto(jugadorPrueba); //[cite: 2]
        }
        else //[cite: 2]
        {
            Debug.LogWarning($"El jugador avanzó a la posición {jugadorPrueba.PosicionActualId}, pero supera el límite del tablero."); //[cite: 2]
        }
    }
}