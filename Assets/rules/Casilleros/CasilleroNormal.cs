using System.Collections.Generic;

public class CasilleroNormal : CasilleroBase
{
    public CasilleroNormal(int id,List<int> siguientesIds): base(id, siguientesIds){
        Tipo = TipoCasillero.Normal;
    }

    public override void EjecutarEfecto(Jugador jugador){
        // Un casillero normal no cambia el estado.
    }
}