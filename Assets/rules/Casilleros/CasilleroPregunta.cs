using System.Collections.Generic;

public class CasilleroPregunta : CasilleroBase
{
    public CasilleroPregunta(int id,List<int> siguientesIds): base(id, siguientesIds){
        Tipo = TipoCasillero.Pregunta;
    }

    public override void EjecutarEfecto(Jugador jugador){
        // La UI se muestra desde GameManager (Visual).
    }
}