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
        lobbyServidor = NetworkLobbyServer.Instance != null
            ? NetworkLobbyServer.Instance
            : FindObjectOfType<NetworkLobbyServer>();

        if (lobbyServidor == null)
        {
            Debug.LogError("[JuegoRedUI] No se encontró NetworkLobbyServer.");

            if (textoResultado != null)
                textoResultado.text = "Error: sin servidor.";

            return;
        }

        if (botonTirarDado != null)
            botonTirarDado.interactable = false;

        if (panelPreguntaRed != null)
            panelPreguntaRed.SetActive(false);

        if (botonTirarDado != null)
            botonTirarDado.onClick.AddListener(SolicitarTirarDado);

        lobbyServidor.EstadoPartidaRecibido += ActualizarEstadoPartida;
        lobbyServidor.EstadoVisualActualizado += ActualizarVisual3D;
        lobbyServidor.ResultadoRecibido += MostrarResultado;
        lobbyServidor.PreguntaRecibida += MostrarPregunta;

        ActualizarTextoSesion();
        lobbyServidor.SolicitarEstadoActualServerRpc();
    }

    private void OnDestroy()
    {
        if (lobbyServidor == null)
            return;

        lobbyServidor.EstadoPartidaRecibido -= ActualizarEstadoPartida;
        lobbyServidor.EstadoVisualActualizado -= ActualizarVisual3D;
        lobbyServidor.ResultadoRecibido -= MostrarResultado;
        lobbyServidor.PreguntaRecibida -= MostrarPregunta;
    }

    private void ActualizarVisual3D(
        int indiceJugadorTurno,
        int[] posicionesCasilleros,
        int valorDado)
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.SincronizarEstadoDesdeRed(
                indiceJugadorTurno,
                posicionesCasilleros,
                valorDado
            );
        }
    }

    private void ActualizarTextoSesion()
    {
        if (textoSesion == null)
            return;

        ulong clientId = NetworkManager.Singleton.LocalClientId;

        textoSesion.text =
            $"ID: {clientId} | Sala: {SesionRedCliente.IdSalaActual}";
    }

    private void SolicitarTirarDado()
    {
        if (lobbyServidor != null)
        {
            Debug.Log("[DIAGNOSTICO] Se recibió una solicitud para tirar el dado.");
            lobbyServidor.SolicitarTirarDadoServerRpc();
        }
    }

    private void ActualizarEstadoPartida(
        string estado,
        ulong clientIdDelTurno)
    {
        if (textoEstadoPartida != null)
            textoEstadoPartida.text = estado;

        if (botonTirarDado != null && NetworkManager.Singleton != null)
        {
            bool esMiTurno =
                NetworkManager.Singleton.LocalClientId == clientIdDelTurno;

            botonTirarDado.interactable = esMiTurno;
        }
    }

    private void MostrarResultado(string mensaje)
    {
        if (textoResultado != null)
            textoResultado.text = mensaje;

        Debug.Log($"[RED] {mensaje}");
    }

    private void MostrarPregunta(string texto, string[] opciones)
    {
        Debug.Log("[DIAGNOSTICO] Se ejecutó MostrarPregunta en la interfaz.");
        Debug.Log($"[JuegoRedUI] MOSTRANDO PREGUNTA: {texto}");

        if (panelPreguntaRed == null)
        {
            Debug.LogError("[JuegoRedUI] ERROR: PanelPreguntaRed no está asignado en el Inspector.");
            return;
        }

        // Bloquear el dado mientras se responde.
        if (botonTirarDado != null)
            botonTirarDado.interactable = false;

        // Activar el panel de preguntas directamente.
        panelPreguntaRed.SetActive(true);

        // Mostrar el texto de la pregunta.
        if (textoPreguntaRed != null)
        {
            textoPreguntaRed.gameObject.SetActive(true);
            textoPreguntaRed.text = texto;
        }
        else
        {
            Debug.LogError("[JuegoRedUI] No está asignado TextoPreguntaRed.");
        }

        if (botonesOpcionesRed == null || botonesOpcionesRed.Length == 0)
        {
            Debug.LogError("[JuegoRedUI] No hay botones de opciones asignados.");
            return;
        }

        // Configurar los botones de respuesta.
        for (int i = 0; i < botonesOpcionesRed.Length; i++)
        {
            Button boton = botonesOpcionesRed[i];

            if (boton == null)
                continue;

            int indiceOpcion = i;

            boton.gameObject.SetActive(true);

            TextMeshProUGUI textoBoton =
                boton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (textoBoton != null)
            {
                textoBoton.gameObject.SetActive(true);

                if (opciones != null && i < opciones.Length)
                    textoBoton.text = opciones[i];
                else
                    textoBoton.text = "";
            }

            boton.onClick.RemoveAllListeners();

            if (opciones != null && i < opciones.Length)
            {
                boton.interactable = true;
                boton.onClick.AddListener(() => ResponderPregunta(indiceOpcion));
            }
            else
            {
                boton.interactable = false;
            }
        }

        // Asegurar que el CanvasGroup permita mostrar e interactuar si existe.
        CanvasGroup cg = panelPreguntaRed.GetComponent<CanvasGroup>();

        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        Debug.Log("[JuegoRedUI] Pregunta mostrada y configurada correctamente.");
    }

    private void ResponderPregunta(int opcionSeleccionada)
    {
        Debug.Log($"[JuegoRedUI] Botón de respuesta presionado. Opción elegida: {opcionSeleccionada}");

        if (panelPreguntaRed != null)
            panelPreguntaRed.SetActive(false);

        if (lobbyServidor != null)
            lobbyServidor.SolicitarResponderPreguntaServerRpc(opcionSeleccionada);
    }
}