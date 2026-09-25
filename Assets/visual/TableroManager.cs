using System.Collections.Generic;
using UnityEngine;

public class TableroManager : MonoBehaviour
{
    public List<CasilleroBase> ListaCasilleros { get; private set; } = new List<CasilleroBase>();

    public void InicializarTablero(int cantidadCasilleros)
    {
        ListaCasilleros.Clear();

        if (cantidadCasilleros <= 0)
            return;

        for (int i = 0; i < cantidadCasilleros; i++)
        {
            int siguiente = i + 1;
            List<int> siguientesIds;

            if (i == cantidadCasilleros - 1)
            {
                siguientesIds = new List<int>();
            }
            else
            {
                siguientesIds = new List<int> { siguiente };
            }

            // Delegamos la creación al Factory en lugar de usar if/else aquí
            CasilleroBase nuevoCasillero = CasilleroFactory.CrearCasillero(i, siguientesIds);
            ListaCasilleros.Add(nuevoCasillero);
        }
    }

    public CasilleroBase ObtenerCasillero(int idCasillero)
    {
        return ListaCasilleros.Find(c => c.Id == idCasillero);
    }

    public void EvaluarCasillero(int idCasillero, Jugador jugador)
    {
        CasilleroBase casilleroActual = ObtenerCasillero(idCasillero);

        if (casilleroActual != null)
        {
            casilleroActual.EjecutarEfecto(jugador);
        }
    }
}

// Implementación del Patrón Simple Factory
public static class CasilleroFactory
{
    // Utilizamos HashSet por su eficiencia O(1) al buscar elementos con Contains()
    private static readonly HashSet<int> casillerosPregunta = new HashSet<int> { 2, 5, 7, 9, 12, 14, 16, 19, 21, 23, 26 };
    private static readonly HashSet<int> casillerosJuego = new HashSet<int> { 3, 17 };
    private static readonly HashSet<int> casillerosEspeciales = new HashSet<int> { 10, 24 };

    public static CasilleroBase CrearCasillero(int id, List<int> siguientesIds)
    {
        if (casillerosPregunta.Contains(id))
        {
            return new CasilleroPregunta(id, siguientesIds);
        }
        
        if (casillerosJuego.Contains(id))
        {
            return new CasilleroJuego(id, siguientesIds);
        }
        
        if (casillerosEspeciales.Contains(id))
        {
            return new CasilleroEspecial(id, siguientesIds);
        }

        // Si el id no está en ninguna lista (incluyendo el 0 inicial), por defecto es Normal
        return new CasilleroNormal(id, siguientesIds);
    }
}