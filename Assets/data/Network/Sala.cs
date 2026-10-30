using System.Collections.Generic;

public enum EstadoSala
{
    EsperandoJugadores,
    EnJuego,
    Terminada
}

public enum ResultadoUnionSala
{
    Jugador,
    Espectador,
    YaEstaEnLaSala,
    SalaLlena,
    SalaTerminada
}

public class Sala
{
    private readonly List<ulong> jugadores =
        new List<ulong>();

    private readonly List<ulong> espectadores =
        new List<ulong>();

    public string Id { get; private set; }
    public string Nombre { get; private set; }
    public ulong CreadorClientId { get; private set; }
    public int MaxJugadores { get; private set; }
    public EstadoSala Estado { get; private set; }

    public IReadOnlyList<ulong> Jugadores => jugadores;
    public IReadOnlyList<ulong> Espectadores => espectadores;

    public Sala(
        string id,
        string nombre,
        ulong creadorClientId,
        int maxJugadores)
    {
        Id = id;
        Nombre = nombre;
        CreadorClientId = creadorClientId;
        MaxJugadores = maxJugadores;
        Estado = EstadoSala.EsperandoJugadores;

        jugadores.Add(creadorClientId);
    }

    public ResultadoUnionSala IntentarUnir(
        ulong clientId)
    {
        if (
            jugadores.Contains(clientId) ||
            espectadores.Contains(clientId)
        )
        {
            return ResultadoUnionSala.YaEstaEnLaSala;
        }

        if (Estado == EstadoSala.EnJuego)
        {
            espectadores.Add(clientId);

            return ResultadoUnionSala.Espectador;
        }

        if (Estado == EstadoSala.Terminada)
        {
            return ResultadoUnionSala.SalaTerminada;
        }

        if (jugadores.Count >= MaxJugadores)
        {
            return ResultadoUnionSala.SalaLlena;
        }

        jugadores.Add(clientId);

        return ResultadoUnionSala.Jugador;
    }

    public bool IntentarIniciar(
        ulong clientId)
    {
        bool esCreador =
            clientId == CreadorClientId;

        bool hayJugadoresSuficientes =
            jugadores.Count >= 2;

        if (
            !esCreador ||
            !hayJugadoresSuficientes ||
            Estado != EstadoSala.EsperandoJugadores
        )
        {
            return false;
        }

        Estado = EstadoSala.EnJuego;

        return true;
    }
}