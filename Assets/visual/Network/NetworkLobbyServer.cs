using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;



public class NetworkLobbyServer : NetworkBehaviour
{

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarTirarDadoServerRpc(
    ServerRpcParams parametrosRpc = default
)
{
    ulong clientId =
        parametrosRpc.Receive.SenderClientId;

    if (
        !salaPorCliente.TryGetValue(
            clientId,
            out string idSala
        )
    )
    {
        EnviarResultadoACliente(
            clientId,
            "No pertenecés a una sala."
        );

        return;
    }

    Sala sala =
        gestorSalas.ObtenerSala(idSala);

    if (
        sala == null ||
        sala.Estado != EstadoSala.EnJuego
    )
    {
        EnviarResultadoACliente(
            clientId,
            "La partida no está en curso."
        );

        return;
    }

    if (
        !partidasPorSala.TryGetValue(
            idSala,
            out PartidaServidor partida
        )
    )
    {
        EnviarResultadoACliente(
            clientId,
            "No existe estado de partida."
        );

        return;
    }

    EventoPartidaServidor evento =
        partida.SolicitarTirarDado(clientId);

    if (!evento.EsValido)
    {
        EnviarResultadoACliente(
            clientId,
            evento.Mensaje
        );

        return;
    }

    EnviarEstadoPartidaASala(
        sala,
        partida,
        evento
    );
}

    public event Action<string, ulong> EstadoPartidaRecibido;

    private readonly NetworkVariable<FixedString512Bytes>
        resumenSalas =
            new NetworkVariable<FixedString512Bytes>(
                new FixedString512Bytes(
                    "No hay salas creadas."
                ),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

    private GestorSalas gestorSalas;

    [Header("Configuración de Gameplay")]
    [SerializeField] private TextAsset archivoPreguntas;
    [SerializeField] private int cantidadCasilleros = 28;

    private BancoPreguntas bancoPreguntas;

    private readonly Dictionary<string, PartidaServidor>
    partidasPorSala =
        new Dictionary<string, PartidaServidor>();

    private readonly Dictionary<ulong, string>
        salaPorCliente =
            new Dictionary<ulong, string>();

    public event Action<string> ResumenActualizado;
    public event Action<string> ResultadoRecibido;

    public string ResumenActual =>
        resumenSalas.Value.ToString();

    public override void OnNetworkSpawn()
    {
        resumenSalas.OnValueChanged +=
            AlCambiarResumenSalas;

        if (IsServer){
            gestorSalas = new GestorSalas();

            CargarPreguntasDelServidor();

            ActualizarResumenEnServidor();
        }

        MostrarResumen(
            resumenSalas.Value.ToString()
        );
    }

    private void CargarPreguntasDelServidor(){
        if (archivoPreguntas == null){
            Debug.LogError("[NetworkLobbyServer] Falta Preguntas.json.");

        return;
        }

        bancoPreguntas = JsonUtility.FromJson<BancoPreguntas>(archivoPreguntas.text);

        if (bancoPreguntas == null ||
            bancoPreguntas.preguntas == null ||
            bancoPreguntas.preguntas.Length == 0){

                Debug.LogError("[NetworkLobbyServer] El banco de preguntas es inválido.");
        }
    }

    public override void OnNetworkDespawn()
    {
        resumenSalas.OnValueChanged -=
            AlCambiarResumenSalas;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarCrearSalaServerRpc(
        FixedString32Bytes nombreSala,
        int maxJugadores,
        ServerRpcParams parametrosRpc = default
    )
    {
        ulong clientId =
            parametrosRpc.Receive.SenderClientId;

        if (salaPorCliente.ContainsKey(clientId))
        {
            EnviarResultadoACliente(
                clientId,
                "Ya pertenecés a una sala."
            );

            return;
        }

        Sala nuevaSala =
            gestorSalas.CrearSala(
                clientId,
                nombreSala.ToString(),
                maxJugadores
            );

        if (nuevaSala == null)
        {
            EnviarResultadoACliente(
                clientId,
                "Configuración de sala inválida."
            );

            return;
        }

        salaPorCliente.Add(
            clientId,
            nuevaSala.Id
        );

        ActualizarResumenEnServidor();

        EnviarResultadoACliente(
            clientId,
            $"Sala creada: {nuevaSala.Id}"
        );
    }


    [ServerRpc(RequireOwnership = false)]
    public void SolicitarUnirseSalaServerRpc(FixedString32Bytes idSala,ServerRpcParams parametrosRpc = default){
        ulong clientId = parametrosRpc.Receive.SenderClientId;

        if (salaPorCliente.ContainsKey(clientId)){
            EnviarResultadoACliente(clientId,"Ya pertenecés a una sala.");
            return;
        }

        Sala sala = gestorSalas.ObtenerSala(idSala.ToString());

        if (sala == null){
            EnviarResultadoACliente(clientId,"La sala indicada no existe.");
            return;
        }

        ResultadoUnionSala resultado = sala.IntentarUnir(clientId);

        if (resultado == ResultadoUnionSala.Jugador){
            salaPorCliente.Add(clientId,sala.Id);

            ActualizarResumenEnServidor();

            EnviarResultadoACliente(clientId,$"Te uniste a {sala.Id} como jugador.");

            return;
        }

        if (resultado == ResultadoUnionSala.Espectador){
            salaPorCliente.Add(clientId,sala.Id);

            ActualizarResumenEnServidor();

            EnviarResultadoACliente(clientId,$"Te uniste a {sala.Id} como espectador.");

            return;
        }

        if (resultado == ResultadoUnionSala.SalaLlena){
            EnviarResultadoACliente(clientId,"La sala está llena.");

            return;
        }

        if (resultado == ResultadoUnionSala.SalaTerminada){
            EnviarResultadoACliente(clientId,"La sala ya terminó.");

            return;
        }

        EnviarResultadoACliente(clientId,"Ya estabas dentro de esa sala.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SolicitarIniciarPartidaServerRpc(ServerRpcParams parametrosRpc = default){
        ulong clientId = parametrosRpc.Receive.SenderClientId;

        if (!salaPorCliente.TryGetValue(clientId,out string idSala)){
            EnviarResultadoACliente(clientId,"No pertenecés a ninguna sala.");

        return;
        }

        Sala sala = gestorSalas.ObtenerSala(idSala);

        if (sala == null){
            EnviarResultadoACliente(clientId,"La sala no existe.");

        return;
        }

        if (bancoPreguntas == null || bancoPreguntas.preguntas == null || bancoPreguntas.preguntas.Length == 0){
            EnviarResultadoACliente(clientId,"No hay preguntas configuradas en el servidor.");

            return;
        }

        PartidaServidor partida;

        try{
            partida = new PartidaServidor(ConfiguracionPartida.Crear(ModoPartida.Normal),
            sala.Jugadores,
            bancoPreguntas.preguntas,
            cantidadCasilleros);
        }

        catch (Exception excepcion){
            EnviarResultadoACliente(clientId,$"No se pudo crear la partida: " + $"{excepcion.Message}");

        return;
        }

        bool inicioCorrecto = sala.IntentarIniciar(clientId);

        if (!inicioCorrecto){
            EnviarResultadoACliente(clientId,"Solo el creador puede iniciar " + "una sala con al menos dos jugadores.");

        return;
        }

        partidasPorSala.Add(sala.Id,partida);

        EnviarEstadoPartidaASala(sala,partida,EventoPartidaServidor.Crear(TipoEventoPartida.TurnoCambiado,
            "La partida comenzó.",
            0,
            partida.ClientIdDelTurno)
        );

        ActualizarResumenEnServidor();

        EnviarResultadoACliente(clientId,$"La partida de {sala.Id} comenzó.");
    }

    private void ActualizarResumenEnServidor()
    {
        if (!IsServer)
        {
            return;
        }

        resumenSalas.Value =
            new FixedString512Bytes(
                ConstruirResumenSalas()
            );
    }

    private string ConstruirResumenSalas()
    {
        if (
            gestorSalas == null ||
            gestorSalas.Salas.Count == 0
        )
        {
            return "No hay salas creadas.";
        }

        StringBuilder resumen =
            new StringBuilder();

        foreach (Sala sala in gestorSalas.Salas){
            resumen.AppendLine(
            $"{sala.Id} | {sala.Nombre} | " +
            $"{sala.Jugadores.Count}/" +
            $"{sala.MaxJugadores} jugadores | " +
            $"{sala.Estado} | " +
            $"{sala.Espectadores.Count} espectadores");
        }

        return resumen.ToString();
    }

    private void EnviarResultadoACliente(
        ulong clientId,
        string mensaje)
    {
        ClientRpcParams parametrosCliente =
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds =
                        new ulong[] { clientId }
                }
            };

        RecibirResultadoClientRpc(
            new FixedString128Bytes(mensaje),
            parametrosCliente
        );
    }

    private void EnviarEstadoPartidaASala(Sala sala,PartidaServidor partida,EventoPartidaServidor evento){
        
        List<ulong> destinatarios = new List<ulong>();

        destinatarios.AddRange(sala.Jugadores);
        destinatarios.AddRange(sala.Espectadores);

        ClientRpcParams parametrosCliente = new ClientRpcParams{
            Send = new ClientRpcSendParams
            {
                TargetClientIds = destinatarios.ToArray()
            }
        };

        string estado = ConstruirEstadoPartida(partida,evento);

        RecibirEstadoPartidaClientRpc(new FixedString512Bytes(estado),partida.ClientIdDelTurno,parametrosCliente);
    }

    private string ConstruirEstadoPartida(PartidaServidor partida,EventoPartidaServidor evento){
        StringBuilder estado = new StringBuilder();

        estado.AppendLine(evento.Mensaje);

        estado.AppendLine($"Fase: {partida.Fase}");

        estado.AppendLine($"Turno: Cliente {partida.ClientIdDelTurno}");

        for (int i = 0;i < partida.Jugadores.Count;i++){
        
            Jugador jugador = partida.Jugadores[i];

            estado.AppendLine(
                $"{jugador.Nombre} | " +
                $"Casillero: " +
                $"{jugador.PosicionActualId + 1} | " +
                $"Vuelta: {jugador.RondaActual} | " +
                $"Correctas: " +
                $"{jugador.RespuestasCorrectas}");
        }

        return estado.ToString();
    }

    [ClientRpc]
    private void RecibirEstadoPartidaClientRpc(FixedString512Bytes estado,ulong clientIdDelTurno,ClientRpcParams parametrosRpc = default){
    
        EstadoPartidaRecibido?.Invoke(estado.ToString(),clientIdDelTurno);
    }

    [ClientRpc]
    private void RecibirResultadoClientRpc(
        FixedString128Bytes mensaje,
        ClientRpcParams parametrosRpc = default
    )
    {
        ResultadoRecibido?.Invoke(
            mensaje.ToString()
        );
    }

    private void AlCambiarResumenSalas(
        FixedString512Bytes anterior,
        FixedString512Bytes actual)
    {
        MostrarResumen(actual.ToString());
    }

    private void MostrarResumen(
        string resumen)
    {
        ResumenActualizado?.Invoke(resumen);
    }
}