using System.Collections.Generic;
using UnityEngine;

public class TableroManager : MonoBehaviour
{
    public List<CasilleroBase> ListaCasilleros { get; private set; } = new List<CasilleroBase>();

    public void InicializarTablero(int cantidadCasilleros){
        ListaCasilleros = TableroFactory.CrearTablero(cantidadCasilleros);
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

