using System.Collections.Generic;

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
        Jugador jugador)
    {
        // La capa Visual inicia el minijuego.
        // La capa Rules solo identifica este tipo de casillero.
    }
}