using System;
using System.Collections.Generic;

public class CasilleroEspecial : CasilleroBase{
    private static readonly Random random = new Random();
    private const int CantidadMovimiento = 2;

    private enum TipoEfecto{
        Avanzar,
        Retroceder,
        Penalizacion
    }

    private static readonly TipoEfecto[] efectosDisponibles = {
        TipoEfecto.Avanzar,
        TipoEfecto.Retroceder,
        TipoEfecto.Penalizacion
    };

    public CasilleroEspecial(int id,List<int> siguientesIds): base(id, siguientesIds){
        Tipo = TipoCasillero.EfectoEspecial;
    }

    public override void EjecutarEfecto(Jugador jugador){
        if (jugador == null){
            return;
        }

        TipoEfecto efecto = efectosDisponibles[random.Next(efectosDisponibles.Length)];

        switch (efecto){
            case TipoEfecto.Avanzar:EfectoAvanzar(jugador,CantidadMovimiento);
            break;

            case TipoEfecto.Retroceder:EfectoRetroceder(jugador,CantidadMovimiento);
            break;

            case TipoEfecto.Penalizacion:EfectoPerderTurno(jugador);
            break;
        }
    }

    private void EfectoAvanzar(Jugador jugador,int cantidad){
        jugador.MoverInstantanio(cantidad);
    }

    private void EfectoRetroceder(Jugador jugador,int cantidad){
        jugador.MoverInstantanio(-cantidad);
    }

    private void EfectoPerderTurno(Jugador jugador){
        jugador.TienePenalizacion = true;
    }
}