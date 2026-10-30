using System.Collections.Generic;

public class GestorSalas
{
    private readonly Dictionary<string, Sala> salas =
        new Dictionary<string, Sala>();

    private int siguienteNumeroDeSala = 1;

    public IReadOnlyCollection<Sala> Salas =>
        salas.Values;

    public Sala CrearSala(
        ulong creadorClientId,
        string nombre,
        int maxJugadores)
    {
        if (
            string.IsNullOrWhiteSpace(nombre)
        )
        {
            return null;
        }

        if (
            maxJugadores < 2 ||
            maxJugadores > 4
        )
        {
            return null;
        }

        string idSala =
            $"SALA-{siguienteNumeroDeSala:000}";

        siguienteNumeroDeSala++;

        Sala nuevaSala = new Sala(
            idSala,
            nombre.Trim(),
            creadorClientId,
            maxJugadores
        );

        salas.Add(
            nuevaSala.Id,
            nuevaSala
        );

        return nuevaSala;
    }

    public Sala ObtenerSala(
        string idSala)
    {
        if (
            string.IsNullOrWhiteSpace(idSala)
        )
        {
            return null;
        }

        salas.TryGetValue(
            idSala,
            out Sala sala
        );

        return sala;
    }
}