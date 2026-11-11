using System.Collections.Generic;

public abstract class CasilleroBase
{
    public int Id { get; private set; }
    public TipoCasillero Tipo { get; protected set; }
    
    // Lista de IDs de los casilleros a los que se puede ir desde aquí
    public List<int> SiguientesCasillerosIds { get; private set; }

    public CasilleroBase(int id, List<int> siguientesIds)
    {
        Id = id;
        SiguientesCasillerosIds = siguientesIds ?? new List<int>();
    }

    public abstract void EjecutarEfecto(Jugador jugador);
}