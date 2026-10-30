using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkBootstrap : MonoBehaviour
{
    [Header("Interfaz")]
    [SerializeField] private Button botonHost;
    [SerializeField] private Button botonCliente;
    [SerializeField] private TMP_InputField campoIp;
    [SerializeField] private TextMeshProUGUI textoEstado;

    [Header("Red")]
    [SerializeField] private ushort puerto = 7777;

    private NetworkManager networkManager;
    private UnityTransport transporte;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        transporte = GetComponent<UnityTransport>();

        if (networkManager == null || transporte == null)
        {
            CambiarEstado(
                "Error: falta NetworkManager o UnityTransport."
            );

            return;
        }

        botonHost.onClick.AddListener(CrearHost);
        botonCliente.onClick.AddListener(UnirseComoCliente);
    }

    private void OnEnable()
    {
        if (networkManager == null)
        {
            return;
        }

        networkManager.OnClientConnectedCallback +=
            AlConectarCliente;

        networkManager.OnClientDisconnectCallback +=
            AlDesconectarCliente;
    }

    private void OnDisable()
    {
        if (networkManager == null)
        {
            return;
        }

        networkManager.OnClientConnectedCallback -=
            AlConectarCliente;

        networkManager.OnClientDisconnectCallback -=
            AlDesconectarCliente;
    }

    public void CrearHost()
    {
        if (networkManager.IsListening)
        {
            CambiarEstado(
                "Ya existe una conexión activa."
            );

            return;
        }

        transporte.SetConnectionData(
            "127.0.0.1",
            puerto,
            "0.0.0.0"
        );

        bool inicioCorrecto =
            networkManager.StartHost();

        CambiarEstado(
            inicioCorrecto
                ? $"Host creado. Cliente ID: " +
                $"{networkManager.LocalClientId}. " +
                $"Puerto: {puerto}"
                : "No se pudo crear el host."
        );
    }

    public void UnirseComoCliente()
    {
        if (networkManager.IsListening)
        {
            CambiarEstado(
                "Ya existe una conexión activa."
            );

            return;
        }

        string ipHost = campoIp.text.Trim();

        if (string.IsNullOrWhiteSpace(ipHost))
        {
            CambiarEstado(
                "Ingresá la IP del host."
            );

            return;
        }

        transporte.SetConnectionData(
            ipHost,
            puerto
        );

        bool inicioCorrecto =
            networkManager.StartClient();

        CambiarEstado(
            inicioCorrecto
                ? $"Conectando a {ipHost}:{puerto}..."
                : "No se pudo iniciar el cliente."
        );
    }

    private void AlConectarCliente(
        ulong clientId)
    {
        if (
            clientId !=
            networkManager.LocalClientId
        )
        {
            return;
        }

        CambiarEstado(
            $"Conectado. Cliente ID: {clientId}"
        );
    }

    private void AlDesconectarCliente(
        ulong clientId)
    {
        if (
            clientId !=
            networkManager.LocalClientId
        )
        {
            return;
        }

        CambiarEstado(
            "Conexión finalizada."
        );
    }

    private void CambiarEstado(
        string mensaje)
    {
        Debug.Log(
            $"[RED] {mensaje}"
        );

        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }
}