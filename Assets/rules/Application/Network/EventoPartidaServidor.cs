public class EventoPartidaServidor
{
    public bool EsValido { get; private set; }
    public TipoEventoPartida Tipo { get; private set; }
    public string Mensaje { get; private set; }
    public int ResultadoDado { get; private set; }
    public ulong ClientIdTurno { get; private set; }
    public PreguntaVisible Pregunta { get; private set; }

    private EventoPartidaServidor(
        bool esValido,
        TipoEventoPartida tipo,
        string mensaje,
        int resultadoDado,
        ulong clientIdTurno,
        PreguntaVisible pregunta)
    {
        EsValido = esValido;
        Tipo = tipo;
        Mensaje = mensaje;
        ResultadoDado = resultadoDado;
        ClientIdTurno = clientIdTurno;
        Pregunta = pregunta;
    }

    public static EventoPartidaServidor Crear(
        TipoEventoPartida tipo,
        string mensaje,
        int resultadoDado,
        ulong clientIdTurno)
    {
        return new EventoPartidaServidor(
            true,
            tipo,
            mensaje,
            resultadoDado,
            clientIdTurno,
            null
        );
    }

    public static EventoPartidaServidor CrearPregunta(
        string mensaje,
        ulong clientIdTurno,
        PreguntaVisible pregunta)
    {
        return new EventoPartidaServidor(
            true,
            TipoEventoPartida.PreguntaPendiente,
            mensaje,
            0,
            clientIdTurno,
            pregunta
        );
    }

    public static EventoPartidaServidor AccionInvalida(
        string mensaje,
        ulong clientIdTurno)
    {
        return new EventoPartidaServidor(
            false,
            TipoEventoPartida.AccionInvalida,
            mensaje,
            0,
            clientIdTurno,
            null
        );
    }
}