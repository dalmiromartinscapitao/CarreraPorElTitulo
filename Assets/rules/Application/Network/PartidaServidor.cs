
using System;
using System.Collections.Generic;

public class PartidaServidor
{
    private readonly ConfiguracionPartida configuracion;
    private readonly GestorVueltas gestorVueltas;
    private readonly List<CasilleroBase> tablero;
    private readonly List<Jugador> jugadores;
    private readonly List<ulong> clientIds;
    private readonly Dictionary<ulong, int> indiceJugadorPorClientId;

    private readonly Random random = new Random();
    private readonly IReadOnlyList<PreguntaData> bancoPreguntas;

    private PreguntaData preguntaActiva;
    private int indiceJugadorActual;

    public FasePartidaServidor Fase { get; private set; }

    public IReadOnlyList<Jugador> Jugadores => jugadores;

    public ulong ClientIdDelTurno => clientIds[indiceJugadorActual];

    public PreguntaVisible ObtenerPreguntaPendiente()
    {
        if (preguntaActiva == null)
            return null;

        return PreguntaVisible.CrearDesde(preguntaActiva);
    }

    public PartidaServidor(
        ConfiguracionPartida configuracionPartida,
        IReadOnlyList<ulong> idsDeClientes,
        IReadOnlyList<PreguntaData> preguntas,
        int cantidadCasilleros)
    {
        if (configuracionPartida == null)
            throw new ArgumentNullException(nameof(configuracionPartida));

        if (idsDeClientes == null ||
            idsDeClientes.Count < 2 ||
            idsDeClientes.Count > 4)
        {
            throw new ArgumentException(
                "La partida debe tener entre 2 y 4 jugadores.");
        }

        if (preguntas == null || preguntas.Count == 0)
        {
            throw new ArgumentException(
                "La partida requiere al menos una pregunta.");
        }

        if (cantidadCasilleros <= 1)
        {
            throw new ArgumentException(
                "El tablero debe tener al menos 2 casilleros.");
        }

        configuracion = configuracionPartida;
        bancoPreguntas = preguntas;

        gestorVueltas = new GestorVueltas(configuracion);

        tablero = TableroFactory.CrearTablero(cantidadCasilleros);

        jugadores = new List<Jugador>();
        clientIds = new List<ulong>();
        indiceJugadorPorClientId = new Dictionary<ulong, int>();

        for (int i = 0; i < idsDeClientes.Count; i++)
        {
            ulong clientId = idsDeClientes[i];

            Jugador jugador = new Jugador(
                i + 1,
                $"Jugador {i + 1}",
                configuracion
            );

            jugadores.Add(jugador);
            clientIds.Add(clientId);

            indiceJugadorPorClientId.Add(clientId, i);
        }

        indiceJugadorActual = 0;

        Fase = FasePartidaServidor.EsperandoDado;

        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Partida creada. " +
            $"Jugadores: {jugadores.Count}. " +
            $"Casilleros: {tablero.Count}"
        );
    }

    public EventoPartidaServidor SolicitarTirarDado(ulong clientId)
    {
        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Solicitud de dado. " +
            $"Cliente: {clientId}. " +
            $"Turno actual: {ClientIdDelTurno}. " +
            $"Fase: {Fase}"
        );

        if (Fase != FasePartidaServidor.EsperandoDado)
        {
            return EventoPartidaServidor.AccionInvalida(
                "No es momento de tirar el dado.",
                ClientIdDelTurno
            );
        }

        if (clientId != ClientIdDelTurno)
        {
            return EventoPartidaServidor.AccionInvalida(
                "No es tu turno.",
                ClientIdDelTurno
            );
        }

        Jugador jugador = jugadores[indiceJugadorActual];

        if (jugador.DebeRepetirPreguntas)
        {
            UnityEngine.Debug.Log(
                "[PARTIDA SERVIDOR] El jugador debe responder una pregunta pendiente."
            );

            Fase = FasePartidaServidor.EsperandoRespuesta;

            return PrepararPregunta(jugador);
        }

        if (jugador.EstaPenalizado())
        {
            jugador.CumplirPenalizacion();

            CambiarTurno();

            return EventoPartidaServidor.Crear(
                TipoEventoPartida.TurnoCambiado,
                $"{jugador.Nombre} perdió su turno.",
                0,
                ClientIdDelTurno
            );
        }

        int resultadoDado = random.Next(1, 7);

        jugador.RegistrarTirada(resultadoDado);

        jugador.Moverse(resultadoDado, tablero.Count);

        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] {jugador.Nombre} avanzó. " +
            $"Dado: {resultadoDado}. " +
            $"Posición: {jugador.PosicionActualId}"
        );

        bool llegoALaMeta =
            jugador.PosicionActualId >= tablero.Count - 1;

        if (llegoALaMeta)
        {
            return ResolverLlegadaAMeta(jugador, resultadoDado);
        }

        return ResolverCasillero(jugador, resultadoDado);
    }

    public EventoPartidaServidor SolicitarRespuesta(
        ulong clientId,
        int opcionSeleccionada)
    {
        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Respuesta recibida. " +
            $"Cliente: {clientId}. Opción: {opcionSeleccionada}"
        );

        if (Fase != FasePartidaServidor.EsperandoRespuesta)
        {
            return EventoPartidaServidor.AccionInvalida(
                "No hay una pregunta pendiente.",
                ClientIdDelTurno
            );
        }

        if (clientId != ClientIdDelTurno)
        {
            return EventoPartidaServidor.AccionInvalida(
                "No te corresponde responder.",
                ClientIdDelTurno
            );
        }

        if (preguntaActiva == null ||
            opcionSeleccionada < 0 ||
            opcionSeleccionada >= preguntaActiva.opciones.Length)
        {
            return EventoPartidaServidor.AccionInvalida(
                "La opción indicada no es válida.",
                ClientIdDelTurno
            );
        }

        Jugador jugador = jugadores[indiceJugadorActual];

        bool respuestaCorrecta = jugador.ResponderPregunta(
            opcionSeleccionada,
            preguntaActiva.indiceCorrecta
        );

        preguntaActiva = null;

        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Respuesta procesada. " +
            $"Correcta: {respuestaCorrecta}"
        );

        bool estaEnLaMeta =
            jugador.PosicionActualId >= tablero.Count - 1;

        if (jugador.DebeRepetirPreguntas || estaEnLaMeta)
        {
            return ResolverLlegadaAMeta(jugador, 0);
        }

        CambiarTurno();

        return EventoPartidaServidor.Crear(
            TipoEventoPartida.TurnoCambiado,
            respuestaCorrecta
                ? $"{jugador.Nombre} respondió correctamente."
                : $"{jugador.Nombre} respondió incorrectamente.",
            0,
            ClientIdDelTurno
        );
    }

    private EventoPartidaServidor ResolverLlegadaAMeta(
        Jugador jugador,
        int resultadoDado)
    {
        ResultadoVuelta resultado =
            gestorVueltas.ResolverLlegadaMeta(jugador);

        if (resultado == ResultadoVuelta.NecesitaPreguntas)
        {
            CambiarTurno();

            return EventoPartidaServidor.Crear(
                TipoEventoPartida.TurnoCambiado,
                $"{jugador.Nombre} debe reunir más respuestas.",
                resultadoDado,
                ClientIdDelTurno
            );
        }

        if (resultado == ResultadoVuelta.Gano)
        {
            jugador.MarcarComoGanador();

            Fase = FasePartidaServidor.Terminada;

            return EventoPartidaServidor.Crear(
                TipoEventoPartida.Victoria,
                $"{jugador.Nombre} ganó la partida.",
                resultadoDado,
                ClientIdDelTurno
            );
        }

        CambiarTurno();

        return EventoPartidaServidor.Crear(
            TipoEventoPartida.TurnoCambiado,
            $"{jugador.Nombre} comenzó una nueva vuelta.",
            resultadoDado,
            ClientIdDelTurno
        );
    }

    private EventoPartidaServidor ResolverCasillero(
        Jugador jugador,
        int resultadoDado)
    {
        CasilleroBase casillero = tablero.Find(
            item => item.Id == jugador.PosicionActualId
        );

        if (casillero == null)
        {
            UnityEngine.Debug.LogError(
                "[PARTIDA SERVIDOR] No se encontró el casillero."
            );

            CambiarTurno();

            return EventoPartidaServidor.Crear(
                TipoEventoPartida.TurnoCambiado,
                "No se encontró el casillero.",
                resultadoDado,
                ClientIdDelTurno
            );
        }

        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Casillero detectado. " +
            $"ID: {casillero.Id}. Tipo: {casillero.Tipo}"
        );

        casillero.EjecutarEfecto(jugador);

        if (casillero.Tipo == TipoCasillero.EfectoEspecial)
        {
            AjustarPosicionAlTablero(jugador);

            CambiarTurno();

            return EventoPartidaServidor.Crear(
                TipoEventoPartida.DadoLanzado,
                $"{jugador.Nombre} activó un casillero especial.",
                resultadoDado,
                ClientIdDelTurno
            );
        }

        if (casillero.Tipo == TipoCasillero.Pregunta)
        {
            UnityEngine.Debug.Log(
                "[PARTIDA SERVIDOR] ¡CASILLERO DE PREGUNTA DETECTADO!"
            );

            Fase = FasePartidaServidor.EsperandoRespuesta;

            return PrepararPregunta(jugador);
        }

        // MINIJUEGO: por ahora se omite y se pasa al siguiente jugador.
        if (casillero.Tipo == TipoCasillero.Juego)
        {
            UnityEngine.Debug.Log(
                "[PARTIDA SERVIDOR] El jugador cayó en un minijuego. " +
                "Se omite porque todavía no está implementado."
            );

            // No se activa EsperandoMinijuego.
            // Se avanza directamente al siguiente turno.
            CambiarTurno();

            return EventoPartidaServidor.Crear(
                TipoEventoPartida.MinijuegoPendiente,
                $"{jugador.Nombre} cayó en un minijuego. " +
                "¡Próximamente! Continúa el siguiente jugador.",
                resultadoDado,
                ClientIdDelTurno
            );
        }

        // Casillero normal.
        CambiarTurno();

        return EventoPartidaServidor.Crear(
            TipoEventoPartida.DadoLanzado,
            $"{jugador.Nombre} avanzó normalmente.",
            resultadoDado,
            ClientIdDelTurno
        );
    }

    private void AjustarPosicionAlTablero(Jugador jugador)
    {
        int ultimaPosicion = tablero.Count - 1;

        if (jugador.PosicionActualId > ultimaPosicion)
        {
            jugador.EstablecerPosicion(ultimaPosicion);
        }
    }

    private EventoPartidaServidor PrepararPregunta(Jugador jugador)
    {
        UnityEngine.Debug.Log(
            "[PARTIDA SERVIDOR] Preparando pregunta..."
        );

        if (bancoPreguntas == null || bancoPreguntas.Count == 0)
        {
            UnityEngine.Debug.LogError(
                "[PARTIDA SERVIDOR] El banco de preguntas está vacío."
            );

            return EventoPartidaServidor.AccionInvalida(
                "No hay preguntas disponibles.",
                ClientIdDelTurno
            );
        }

        int indicePregunta = random.Next(bancoPreguntas.Count);

        preguntaActiva = bancoPreguntas[indicePregunta];

        if (preguntaActiva == null)
        {
            UnityEngine.Debug.LogError(
                "[PARTIDA SERVIDOR] La pregunta seleccionada es nula."
            );

            return EventoPartidaServidor.AccionInvalida(
                "No se pudo cargar la pregunta.",
                ClientIdDelTurno
            );
        }

        Fase = FasePartidaServidor.EsperandoRespuesta;

        PreguntaVisible preguntaVisible =
            PreguntaVisible.CrearDesde(preguntaActiva);

        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Pregunta preparada correctamente. " +
            $"Cliente: {ClientIdDelTurno}. " +
            $"Índice: {indicePregunta}"
        );

        return EventoPartidaServidor.CrearPregunta(
            $"{jugador.Nombre} debe responder una pregunta.",
            ClientIdDelTurno,
            preguntaVisible
        );
    }

    public EventoPartidaServidor ObtenerEstadoActual()
    {
        if (Fase == FasePartidaServidor.EsperandoRespuesta &&
            preguntaActiva != null)
        {
            return EventoPartidaServidor.CrearPregunta(
                "Hay una pregunta pendiente.",
                ClientIdDelTurno,
                PreguntaVisible.CrearDesde(preguntaActiva)
            );
        }

        if (Fase == FasePartidaServidor.Terminada)
        {
            return EventoPartidaServidor.Crear(
                TipoEventoPartida.Victoria,
                "La partida terminó.",
                0,
                ClientIdDelTurno
            );
        }

        return EventoPartidaServidor.Crear(
            TipoEventoPartida.TurnoCambiado,
            "Estado de partida actualizado.",
            0,
            ClientIdDelTurno
        );
    }

    private void CambiarTurno()
    {
        indiceJugadorActual++;

        if (indiceJugadorActual >= jugadores.Count)
        {
            indiceJugadorActual = 0;
        }

        Fase = FasePartidaServidor.EsperandoDado;

        UnityEngine.Debug.Log(
            $"[PARTIDA SERVIDOR] Turno cambiado. " +
            $"Ahora juega: {ClientIdDelTurno}. Fase: {Fase}"
        );
    }
}
