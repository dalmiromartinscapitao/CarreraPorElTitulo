using System.Collections.Generic;

public class CasilleroNormal : CasilleroBase
{
    public CasilleroNormal(int id, List<int> siguientesIds) : base(id, siguientesIds)
    {
        Tipo = TipoCasillero.Normal;
    }

    public override void EjecutarEfecto(Jugador jugador)
    {
        // No realiza ninguna acción especial
        UnityEngine.Debug.Log($"Jugador {jugador.Nombre} cayó en casillero Normal #{Id}. Pasa el turno.");
    }
}