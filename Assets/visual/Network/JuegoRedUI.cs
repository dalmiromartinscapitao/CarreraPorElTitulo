using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JuegoRedUI : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private TextMeshProUGUI textoSesion;
    [SerializeField] private TextMeshProUGUI textoEstadoPartida;
    [SerializeField] private TextMeshProUGUI textoResultado;
    [SerializeField] private Button botonTirarDado;

    [Header("Pregunta")]
    [SerializeField] private GameObject panelPreguntaRed;
    [SerializeField] private TextMeshProUGUI textoPreguntaRed;
    [SerializeField] private Button[] botonesOpcionesRed;

    private NetworkLobbyServer lobbyServidor;

    private void Start()
    {
        lobbyServidor =
            FindObjectOfType<NetworkLobbyServer>();

        if (lobbyServidor == null)
        {
            textoResultado.text =
                "Error: no existe servidor de lobby.";

            return;
        }

        botonTirarDado.interactable = false;

        panelPreguntaRed.SetActive(false);

        botonTirarDado.onClick.AddListener(
            SolicitarTirarDado
        );

        lobbyServidor.EstadoPartidaRecibido +=
            ActualizarEstadoPartida;

        lobbyServidor.ResultadoRecibido +=
            MostrarResultado;

        lobbyServidor.PreguntaRecibida +=
            MostrarPregunta;

        ActualizarTextoSesion();

        lobbyServidor.SolicitarEstadoActualServerRpc();
    }

    private void OnDestroy()
    {
        if (lobbyServidor == null)
        {
            return;
        }

        lobbyServidor.EstadoPartidaRecibido -=
            ActualizarEstadoPartida;

        lobbyServidor.ResultadoRecibido -=
            MostrarResultado;

        lobbyServidor.PreguntaRecibida -=
            MostrarPregunta;
    }

    private void ActualizarTextoSesion()
    {
        ulong clientId =
            NetworkManager.Singleton.LocalClientId;

        textoSesion.text =
            $"Cliente ID: {clientId}\n" +
            $"Sala: {SesionRedCliente.IdSalaActual}";
    }

    private void SolicitarTirarDado()
    {
        lobbyServidor
            .SolicitarTirarDadoServerRpc();
    }

    private void ActualizarEstadoPartida(
        string estado,
        ulong clientIdDelTurno)
    {
        textoEstadoPartida.text = estado;

        bool esMiTurno =
            NetworkManager.Singleton.LocalClientId ==
            clientIdDelTurno;

        botonTirarDado.interactable = esMiTurno;
    }

    private void MostrarResultado(
        string mensaje)
    {
        textoResultado.text = mensaje;
    }

    private void MostrarPregunta(
        string texto,
        string[] opciones)
    {
        botonTirarDado.interactable = false;

        textoPreguntaRed.text = texto;

        for (
            int i = 0;
            i < botonesOpcionesRed.Length;
            i++
        )
        {
            int indiceOpcion = i;

            botonesOpcionesRed[i]
                .GetComponentInChildren<TextMeshProUGUI>()
                .text = opciones[i];

            botonesOpcionesRed[i]
                .onClick
                .RemoveAllListeners();

            botonesOpcionesRed[i]
                .onClick
                .AddListener(
                    () => ResponderPregunta(
                        indiceOpcion
                    )
                );
        }

        panelPreguntaRed.SetActive(true);
    }

    private void ResponderPregunta(
        int opcionSeleccionada)
    {
        panelPreguntaRed.SetActive(false);

        lobbyServidor
            .SolicitarResponderPreguntaServerRpc(
                opcionSeleccionada
            );
    }
}