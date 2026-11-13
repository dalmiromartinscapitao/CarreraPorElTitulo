
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

        Debug.Log("[JuegoRedUI] ================================");
        Debug.Log($"[JuegoRedUI] MOSTRANDO PREGUNTA: {texto}");
        Debug.Log($"[JuegoRedUI] Opciones recibidas: {opciones?.Length ?? 0}");

        if (panelPreguntaRed == null)
        {
            Debug.LogError(
                "[JuegoRedUI] ERROR: PanelPreguntaRed no está asignado."
            );
            return;
        }

        // Bloquear el dado mientras se responde.
        if (botonTirarDado != null)
            botonTirarDado.interactable = false;

        // Activar el panel principal.
        panelPreguntaRed.SetActive(true);

        // Activar el contenedor de preguntas.
        Transform panelPreguntas =
            panelPreguntaRed.transform.Find("PanelDePreguntas");

        if (panelPreguntas != null)
        {
            panelPreguntas.gameObject.SetActive(true);

            Debug.Log(
                "[DIAGNOSTICO] PanelDePreguntas activado."
            );

            // Activar y comprobar todos sus hijos.
            foreach (Transform hijo in panelPreguntas)
            {
                hijo.gameObject.SetActive(true);

                Debug.Log(
                    $"[DIAGNOSTICO] Hijo: {hijo.name} | " +
                    $"Activo: {hijo.gameObject.activeSelf}"
                );
            }
        }
        else
        {
            Debug.LogError(
                "[DIAGNOSTICO] No se encontró PanelDePreguntas. " +
                "Revisá que sea hijo directo de PanelPreguntaRed."
            );
        }

        // Mostrar el texto de la pregunta.
        if (textoPreguntaRed != null)
        {
            textoPreguntaRed.gameObject.SetActive(true);
            textoPreguntaRed.text = texto;
        }
        else
        {
            Debug.LogError(
                "[JuegoRedUI] No está asignado TextoPreguntaRed."
            );
        }

        if (botonesOpcionesRed == null ||
            botonesOpcionesRed.Length == 0)
        {
            Debug.LogError(
                "[JuegoRedUI] No hay botones de opciones asignados."
            );
            return;
        }

        // Configurar los botones de respuesta.
        for (int i = 0; i < botonesOpcionesRed.Length; i++)
        {
            Button boton = botonesOpcionesRed[i];

            if (boton == null)
                continue;

            int indiceOpcion = i;

            // Activar el botón.
            boton.gameObject.SetActive(true);

            // Obtener el texto del botón.
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

            // Limpiar listeners anteriores.
            boton.onClick.RemoveAllListeners();

            // Asignar la respuesta correspondiente.
            if (opciones != null && i < opciones.Length)
            {
                boton.interactable = true;

                boton.onClick.AddListener(
                    () => ResponderPregunta(indiceOpcion)
                );
            }
            else
            {
                boton.interactable = false;
            }
        }

        // Asegurar que el CanvasGroup permita mostrar e interactuar.
        CanvasGroup cg =
            panelPreguntaRed.GetComponent<CanvasGroup>();

        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // También comprobar si PanelDePreguntas tiene CanvasGroup.
        if (panelPreguntas != null)
        {
            CanvasGroup cgHijo =
                panelPreguntas.GetComponent<CanvasGroup>();

            if (cgHijo != null)
            {
                cgHijo.alpha = 1f;
                cgHijo.interactable = true;
                cgHijo.blocksRaycasts = true;
            }
        }

        Debug.Log(
            $"[JuegoRedUI] PanelPreguntaRed activo: " +
            $"{panelPreguntaRed.activeSelf}"
        );

        Debug.Log(
            $"[JuegoRedUI] PanelDePreguntas activo: " +
            $"{(panelPreguntas != null && panelPreguntas.gameObject.activeSelf)}"
        );

        Debug.Log(
            "[JuegoRedUI] Pregunta mostrada correctamente."
        );
    }

    private void ResponderPregunta(int opcionSeleccionada)
    {
        Debug.Log(
            $"[JuegoRedUI] Botón de respuesta presionado. " +
            $"Opción elegida: {opcionSeleccionada}"
        );

        if (panelPreguntaRed != null)
            panelPreguntaRed.SetActive(false);

        if (lobbyServidor != null)
        {
            lobbyServidor.SolicitarResponderPreguntaServerRpc(
                opcionSeleccionada
            );
        }
    }
}
