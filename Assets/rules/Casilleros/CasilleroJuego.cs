using System.Collections.Generic;
using UnityEngine;

public class CasilleroJuego : CasilleroBase
{
    public CasilleroJuego(
        int id,
        List<int> siguientesIds
    ) : base(id, siguientesIds)
    {
        Tipo = TipoCasillero.Juego;
    }

    public override void EjecutarEfecto(
        Jugador jugador
    )
    {
        // Llamamos al minijuego de Pong pasándole el jugador que cayó en la casilla
        if (MiniJuegoManager.Instancia != null)
        {
            MiniJuegoManager.Instancia.IniciarMinijuego(jugador);
        }
        else
        {
            Debug.LogError("[CasilleroJuego] No se encontró una instancia de MiniJuegoManager en la escena.");
        }
    }
}