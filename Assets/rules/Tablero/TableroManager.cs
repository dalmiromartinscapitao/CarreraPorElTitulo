using System.Collections.Generic;
using UnityEngine;

public class TableroManager : MonoBehaviour
{
    // Lista Lógica de Casilleros
    public List<CasilleroBase> ListaCasilleros { get; private set; } = new List<CasilleroBase>();

    private void Awake()
    {
        InicializarTablero();
    }

    private void InicializarTablero()
    {
        // Ejemplo creando casilleros en la lista:
        // Casillero 0: Normal -> apunta al casillero 1
        ListaCasilleros.Add(new CasilleroNormal(0, new List<int> { 1 }));

        // Casillero 1: Pregunta Multiple Choice -> apunta al casillero 2
        ListaCasilleros.Add(new CasilleroPregunta(1, new List<int> { 2 }));

        // Casillero 2: Especial Aleatorio -> apunta al casillero 3
        ListaCasilleros.Add(new CasilleroEspecial(2, new List<int> { 3 }));
    }

    // Método para activar la lógica al aterrizar en un casillero
    public void EvaluarCasillero(int idCasillero, Jugador jugador)
    {
        CasilleroBase casilleroActual = ListaCasilleros.Find(c => c.Id == idCasillero);
        
        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);
        }
    }
}