using System;
using System.Collections.Generic;
using UnityEngine;

public class CasilleroEspecial : CasilleroBase
{
    private static readonly System.Random random = new System.Random();

    public CasilleroEspecial(int id, List<int> siguientesIds) : base(id, siguientesIds)
    {
        Tipo = TipoCasillero.EfectoEspecial;
    }

    public override void EjecutarEfecto(Jugador jugador)
    {
        int efecto = random.Next(0, 3); // Genera 0, 1 o 2

        switch (efecto)
        {
            case 0:
                EfectoAvanzar(jugador, 2);
                break;
            case 1:
                EfectoRetroceder(jugador, 2);
                break;
            case 2:
                EfectoPerderTurno(jugador);
                break;
        }
    }

    private void EfectoAvanzar(Jugador jugador, int cantidad)
    {
        Debug.Log($"[Efecto Especial] ¡{jugador.Nombre} avanza {cantidad} casilleros extra!");
        jugador.MoverInstantanio(cantidad);
    }

    private void EfectoRetroceder(Jugador jugador, int cantidad)
    {
        Debug.Log($"[Efecto Especial] ¡{jugador.Nombre} retrocede {cantidad} casilleros!");
        jugador.MoverInstantanio(-cantidad);
    }

    private void EfectoPerderTurno(Jugador jugador)
    {
        Debug.Log($"[Efecto Especial] ¡{jugador.Nombre} recibe una penalización para su próximo turno!");
        jugador.TienePenalizacion = true; // <--- Cambio aquí (antes decía PierdeTurno)
    }
}