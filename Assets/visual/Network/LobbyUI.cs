using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField campoNombreSala;
    [SerializeField] private TMP_Dropdown listaMaxJugadores;
    [SerializeField] private Button botonCrearSala;
    [SerializeField] private TextMeshProUGUI textoResumenSalas;
    [SerializeField] private TextMeshProUGUI textoResultadoLobby;
    [SerializeField] private TMP_InputField campoIdSala;
    [SerializeField] private Button botonUnirseSala;

    private NetworkLobbyServer lobbyServidor;

    private void Start()
    {
        lobbyServidor =
            FindObjectOfType<NetworkLobbyServer>();

        if (lobbyServidor == null)
        {
            MostrarResultado(
                "Error: no existe LobbyServidor."
            );

            return;
        }

        botonCrearSala.onClick.AddListener(
            SolicitarCrearSala
        );

        botonUnirseSala.onClick.AddListener(
        SolicitarUnirseSala
        );

        lobbyServidor.ResumenActualizado +=
            MostrarResumen;

        lobbyServidor.ResultadoRecibido +=
            MostrarResultado;

        MostrarResumen(
            lobbyServidor.ResumenActual
        );
    }

    private void OnDestroy()
    {
        if (lobbyServidor == null)
        {
            return;
        }

        lobbyServidor.ResumenActualizado -=
            MostrarResumen;

        lobbyServidor.ResultadoRecibido -=
            MostrarResultado;
    }

    private void SolicitarCrearSala()
    {
        if (
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsClient
        )
        {
            MostrarResultado(
                "Primero creá un host o uníte como cliente."
            );

            return;
        }

        string nombreSala =
            campoNombreSala.text.Trim();

        if (string.IsNullOrWhiteSpace(nombreSala))
        {
            MostrarResultado(
                "Ingresá un nombre para la sala."
            );

            return;
        }

        string textoMaxJugadores =
            listaMaxJugadores
                .options[
                    listaMaxJugadores.value
                ]
                .text;

        if (
            !int.TryParse(
                textoMaxJugadores,
                out int maxJugadores
            )
        )
        {
            MostrarResultado(
                "Cantidad de jugadores inválida."
            );

            return;
        }

        lobbyServidor.SolicitarCrearSalaServerRpc(
            new FixedString32Bytes(nombreSala),
            maxJugadores
        );
    }

    private void SolicitarUnirseSala(){
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsClient){
        MostrarResultado("Primero creá un host o uníte como cliente.");

        return;
        }

        string idSala = campoIdSala.text.Trim();

        if (string.IsNullOrWhiteSpace(idSala)){
            MostrarResultado("Ingresá un ID de sala.");

            return;
        }

        lobbyServidor.SolicitarUnirseSalaServerRpc(new FixedString32Bytes(idSala.ToUpper()));
    }

    private void MostrarResumen(
        string resumen)
    {
        textoResumenSalas.text = resumen;
    }

    private void MostrarResultado(
        string mensaje)
    {
        textoResultadoLobby.text = mensaje;
    }
}