using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkLobbyServer : NetworkBehaviour
{
    public static NetworkLobbyServer Instance { get; private set; }

    public event Action<string, string[]> PreguntaRecibida;
    public event Action<string, ulong> EstadoPartidaRecibido;
    public event Action<string> ResumenActualizado;
    public event Action<string> ResultadoRecibido;
    public event Action<int, int[], int> EstadoVisualActualizado;

    private readonly NetworkVariable<FixedString512Bytes> resumenSalas =
        new NetworkVariable<FixedString512Bytes>(
            new FixedString512Bytes("No hay salas creadas."),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private GestorSalas gestorSalas;

    [Header("Configuración de Gameplay")]
    [SerializeField] private TextAsset archivoPreguntas;
    [SerializeField] private int cantidadCasilleros = 28;

    private BancoPreguntas bancoPreguntas;

    private readonly Dictionary<string, PartidaServidor> partidasPorSala =
        new Dictionary<string, PartidaServidor>();

    private readonly Dictionary<ulong, string> salaPorCliente =
        new Dictionary<ulong, string>();

    public string ResumenActual => resumenSalas.Value.ToString();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;
        resumenSalas.OnValueChanged += AlCambiarResumenSalas;

        if (IsServer)
        {
            gestorSalas = new GestorSalas();
            CargarPreguntasDelServidor();
            ActualizarResumenEnServidor();
        }

        MostrarResumen(resumenSalas.Value.ToString());
    }

    public override void OnNetworkDespawn()
    {
        resumenSalas.OnValueChanged -= AlCambiarResumenSalas;
    }

    private void CargarPreguntasDelServidor()
    {
        if (archivoPreguntas == null)
        {
            Debug.LogError("[NetworkLobbyServer] Falta Preguntas.json.");
            return;
        }

        bancoPreguntas = JsonUtility.FromJson<BancoPreguntas>(archivoPreguntas.text);

        if (bancoPreguntas == null ||
            bancoPreguntas.preguntas == null ||
            bancoPreguntas.preguntas.Length == 0)
        {
            Debug.LogError("[NetworkLobbyServer] El banco de preguntas es inválido.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarCrearSalaServerRpc(
        FixedString32Bytes nombreSala,
        int maxJugadores,
        ServerRpcParams parametrosRpc = default
    )
    {
        ulong clientId = parametrosRpc.Receive.SenderClientId;

        if (salaPorCliente.ContainsKey(clientId))
        {
            EnviarResultadoACliente(clientId, "Ya pertenecés a una sala.");
            return;
        }

        Sala nuevaSala = gestorSalas.CrearSala(clientId, nombreSala.ToString(), maxJugadores);

        if (nuevaSala == null)
        {
            EnviarResultadoACliente(clientId, "Configuración de sala inválida.");
            return;
        }

        salaPorCliente.Add(clientId, nuevaSala.Id);
        ActualizarResumenEnServidor();
        EnviarResultadoACliente(clientId, $"Sala creada: {nuevaSala.Id}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarUnirseSalaServerRpc(
        FixedString32Bytes idSala,
        ServerRpcParams parametrosRpc = default
    )
    {
        ulong clientId = parametrosRpc.Receive.SenderClientId;

        if (salaPorCliente.ContainsKey(clientId))
        {
            EnviarResultadoACliente(clientId, "Ya pertenecés a una sala.");
            return;
        }

        Sala sala = gestorSalas.ObtenerSala(idSala.ToString());

        if (sala == null)
        {
            EnviarResultadoACliente(clientId, "La sala indicada no existe.");
            return;
        }

        ResultadoUnionSala resultado = sala.IntentarUnir(clientId);

        if (resultado == ResultadoUnionSala.Jugador || resultado == ResultadoUnionSala.Espectador)
        {
            salaPorCliente.Add(clientId, sala.Id);
            ActualizarResumenEnServidor();
            EnviarResultadoACliente(clientId, $"Te uniste a {sala.Id}.");
            return;
        }

        EnviarResultadoACliente(clientId, "No se pudo unir a la sala.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarIniciarPartidaServerRpc(ServerRpcParams parametrosRpc = default)
    {
        ulong clientId = parametrosRpc.Receive.SenderClientId;

        if (!salaPorCliente.TryGetValue(clientId, out string idSala)) return;

        Sala sala = gestorSalas.ObtenerSala(idSala);
        if (sala == null) return;

        try
        {
            PartidaServidor partida = new PartidaServidor(
                ConfiguracionPartida.Crear(ModoPartida.Normal),
                sala.Jugadores,
                bancoPreguntas.preguntas,
                cantidadCasilleros
            );

            if (sala.IntentarIniciar(clientId))
            {
                partidasPorSala.Add(sala.Id, partida);
                EnviarEstadoPartidaASala(
                    sala,
                    partida,
                    EventoPartidaServidor.Crear(TipoEventoPartida.TurnoCambiado, "La partida comenzó.", 0, partida.ClientIdDelTurno)
                );
                ActualizarResumenEnServidor();
                CargarJuegoParaSala(sala);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Server] Error al iniciar partida: {ex.Message}");
        }
    }

    private void CargarJuegoParaSala(Sala sala)
    {
        List<ulong> destinatarios = new List<ulong>(sala.Jugadores);
        destinatarios.AddRange(sala.Espectadores);

        ClientRpcParams parametrosCliente = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = destinatarios.ToArray() }
        };

        PrepararJuegoClientRpc(new FixedString32Bytes(sala.Id), parametrosCliente);

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("JuegoRed", LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void PrepararJuegoClientRpc(FixedString32Bytes idSala, ClientRpcParams parametrosRpc = default)
    {
        SesionRedCliente.EstablecerSala(idSala.ToString());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarEstadoActualServerRpc(ServerRpcParams parametrosRpc = default)
    {
        ulong clientId = parametrosRpc.Receive.SenderClientId;
        if (!salaPorCliente.TryGetValue(clientId, out string idSala)) return;

        Sala sala = gestorSalas.ObtenerSala(idSala);
        if (sala == null || sala.Estado != EstadoSala.EnJuego) return;

        if (partidasPorSala.TryGetValue(idSala, out PartidaServidor partida))
        {
            EnviarEstadoPartidaASala(sala, partida, partida.ObtenerEstadoActual());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarTirarDadoServerRpc(ServerRpcParams parametrosRpc = default)
    {
        Debug.Log("[DIAGNOSTICO] Se recibió una solicitud para tirar el dado.");

        ulong clientId = parametrosRpc.Receive.SenderClientId;
        if (!salaPorCliente.TryGetValue(clientId, out string idSala)) return;

        Sala sala = gestorSalas.ObtenerSala(idSala);
        if (sala == null || sala.Estado != EstadoSala.EnJuego) return;

        if (partidasPorSala.TryGetValue(idSala, out PartidaServidor partida))
        {
            Debug.Log($"[SERVIDOR] Solicitud de tirar dado recibida del Cliente {clientId}. Turno actual: {partida.ClientIdDelTurno}");
            EventoPartidaServidor evento = partida.SolicitarTirarDado(clientId);
            
            if (evento.EsValido)
            {
                EnviarEstadoPartidaASala(sala, partida, evento);
            }
            else
            {
                Debug.LogWarning($"[SERVIDOR] Acción inválida de tirada: {evento.Mensaje}");
                EnviarResultadoACliente(clientId, evento.Mensaje);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarResponderPreguntaServerRpc(int opcionSeleccionada, ServerRpcParams parametrosRpc = default)
    {
        ulong clientId = parametrosRpc.Receive.SenderClientId;
        if (!salaPorCliente.TryGetValue(clientId, out string idSala)) return;

        if (partidasPorSala.TryGetValue(idSala, out PartidaServidor partida))
        {
            Sala sala = gestorSalas.ObtenerSala(idSala);
            Debug.Log($"[SERVIDOR] Respuesta de pregunta recibida del Cliente {clientId}. Opción: {opcionSeleccionada}");
            
            EventoPartidaServidor evento = partida.SolicitarRespuesta(clientId, opcionSeleccionada);
            if (evento.EsValido)
            {
                EnviarEstadoPartidaASala(sala, partida, evento);
            }
            else
            {
                EnviarResultadoACliente(clientId, evento.Mensaje);
            }
        }
    }

    private void ActualizarResumenEnServidor()
    {
        if (!IsServer) return;
        resumenSalas.Value = new FixedString512Bytes(ConstruirResumenSalas());
    }

    private string ConstruirResumenSalas()
    {
        if (gestorSalas == null || gestorSalas.Salas.Count == 0) return "No hay salas creadas.";
        StringBuilder resumen = new StringBuilder();
        foreach (Sala sala in gestorSalas.Salas)
        {
            resumen.AppendLine($"{sala.Id} | {sala.Nombre} | {sala.Jugadores.Count}/{sala.MaxJugadores} | {sala.Estado}");
        }
        return resumen.ToString();
    }

    private void EnviarResultadoACliente(ulong clientId, string mensaje)
    {
        ClientRpcParams parametrosCliente = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };
        RecibirResultadoClientRpc(new FixedString128Bytes(mensaje ?? ""), parametrosCliente);
    }

    private void EnviarEstadoPartidaASala(
    Sala sala,
    PartidaServidor partida,
    EventoPartidaServidor evento)
{
  Debug.Log("[DIAGNOSTICO] Entró en EnviarEstadoPartidaASala.");

Debug.Log($"[DIAGNOSTICO] Tipo de evento: {evento.Tipo}");
Debug.Log($"[DIAGNOSTICO] Fase de partida: {partida.Fase}");
Debug.Log($"[DIAGNOSTICO] Jugador del turno: {partida.ClientIdDelTurno}");
Debug.Log($"[DIAGNOSTICO] Pregunta del evento: {evento.Pregunta != null}");

    List<ulong> destinatarios = new List<ulong>(sala.Jugadores);
    destinatarios.AddRange(sala.Espectadores);

    ClientRpcParams parametrosCliente = new ClientRpcParams
    {
        Send = new ClientRpcSendParams
        {
            TargetClientIds = destinatarios.ToArray()
        }
    };

    string estado =
        $"{evento.Mensaje}\nTurno: Cliente {partida.ClientIdDelTurno}";

    RecibirEstadoPartidaClientRpc(
        new FixedString512Bytes(estado),
        partida.ClientIdDelTurno,
        parametrosCliente
    );

    int[] posiciones = new int[partida.Jugadores.Count];

    int indiceTurno = 0;

    for (int i = 0; i < partida.Jugadores.Count; i++)
    {
        posiciones[i] = partida.Jugadores[i].PosicionActualId;

        if (i < sala.Jugadores.Count &&
            sala.Jugadores[i] == partida.ClientIdDelTurno)
        {
            indiceTurno = i;
        }
    }

    RecibirEstadoVisualClientRpc(
        indiceTurno,
        posiciones,
        evento.ResultadoDado,
        parametrosCliente
    );

    // DETECCIÓN Y ENVÍO DE PREGUNTAS

    PreguntaVisible pregunta = evento.Pregunta;

    if (pregunta == null &&
        partida.Fase == FasePartidaServidor.EsperandoRespuesta)
    {
        Debug.LogWarning(
            "[SERVIDOR] El evento no contiene pregunta. " +
            "Intentando recuperar la pregunta activa."
        );

        pregunta = partida.ObtenerPreguntaPendiente();
    }

    if (partida.Fase == FasePartidaServidor.EsperandoRespuesta &&
        pregunta != null)
    {
        Debug.Log(
            $"[SERVIDOR] ¡PREGUNTA CONFIRMADA! " +
            $"Enviando al cliente {partida.ClientIdDelTurno}"
        );

        EnviarPreguntaACliente(
            partida.ClientIdDelTurno,
            pregunta
        );
    }
    else if (partida.Fase == FasePartidaServidor.EsperandoRespuesta)
    {
        Debug.LogError(
            "[SERVIDOR] ERROR: La partida espera respuesta, " +
            "pero no se encontró ninguna pregunta."
        );
    }
}

    private void EnviarPreguntaACliente(ulong clientId, PreguntaVisible pregunta)
    {
        Debug.Log("[DIAGNOSTICO] Se está intentando enviar la pregunta al cliente.");

        if (pregunta == null)
        {
            Debug.LogWarning("[SERVIDOR] Intento de enviar una pregunta nula.");
            return;
        }

        string texto = pregunta.Texto ?? "¿Pregunta?";
        string op0 = (pregunta.Opciones != null && pregunta.Opciones.Length > 0) ? pregunta.Opciones[0] : "Opción A";
        string op1 = (pregunta.Opciones != null && pregunta.Opciones.Length > 1) ? pregunta.Opciones[1] : "Opción B";
        string op2 = (pregunta.Opciones != null && pregunta.Opciones.Length > 2) ? pregunta.Opciones[2] : "Opción C";
        string op3 = (pregunta.Opciones != null && pregunta.Opciones.Length > 3) ? pregunta.Opciones[3] : "Opción D";

        ClientRpcParams parametrosCliente = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };

        RecibirPreguntaClientRpc(
            new FixedString128Bytes(texto),
            new FixedString128Bytes(op0),
            new FixedString128Bytes(op1),
            new FixedString128Bytes(op2),
            new FixedString128Bytes(op3),
            parametrosCliente
        );
    }

    [ClientRpc]
    private void RecibirEstadoVisualClientRpc(int indiceJugadorTurno, int[] posicionesCasilleros, int valorDado, ClientRpcParams parametrosRpc = default)
    {
        EstadoVisualActualizado?.Invoke(indiceJugadorTurno, posicionesCasilleros, valorDado);
    }

    [ClientRpc]
    private void RecibirEstadoPartidaClientRpc(FixedString512Bytes estado, ulong clientIdDelTurno, ClientRpcParams parametrosRpc = default)
    {
        EstadoPartidaRecibido?.Invoke(estado.ToString(), clientIdDelTurno);
    }

    [ClientRpc]
    private void RecibirPreguntaClientRpc(
        FixedString128Bytes texto,
        FixedString128Bytes opcion0,
        FixedString128Bytes opcion1,
        FixedString128Bytes opcion2,
        FixedString128Bytes opcion3,
        ClientRpcParams parametrosRpc = default
    )
    {
        Debug.Log($"[CLIENTE {NetworkManager.Singleton.LocalClientId}] ¡Pregunta recibida por RPC! Texto: {texto}");
        string[] opciones = new string[] { opcion0.ToString(), opcion1.ToString(), opcion2.ToString(), opcion3.ToString() };
        PreguntaRecibida?.Invoke(texto.ToString(), opciones);
    }

    [ClientRpc]
    private void RecibirResultadoClientRpc(FixedString128Bytes mensaje, ClientRpcParams parametrosRpc = default)
    {
        ResultadoRecibido?.Invoke(mensaje.ToString());
    }

    private void AlCambiarResumenSalas(FixedString512Bytes anterior, FixedString512Bytes actual)
    {
        MostrarResumen(actual.ToString());
    }

    private void MostrarResumen(string resumen)
    {
        ResumenActualizado?.Invoke(resumen);
    }
}