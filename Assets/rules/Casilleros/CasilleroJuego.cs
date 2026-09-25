using System.Collections.Generic;

public class CasilleroJuego : CasilleroBase
{
    public CasilleroJuego(int id, List<int> siguientesIds) : base(id, siguientesIds)
    {
        Tipo = TipoCasillero.Juego; // Asegúrate de que este enum exista
    }

    public override void EjecutarEfecto(Jugador jugador)
    {
        // Lógica para abrir el minijuego o aplicar el efecto correspondiente
    }
}