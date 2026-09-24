using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class MenuManager : MonoBehaviour
{
    // Asegúrate de poner el nombre exacto de tu escena de juego aquí
    public string nombreEscenaJuego = "EscenaJuego"; 

    public void JugarCon2Jugadores()
    {
        SesionPartida.SeleccionarCantidadJugadores(2);
        EmpezarPartida();
    }

    public void JugarCon3Jugadores()
    {
        SesionPartida.SeleccionarCantidadJugadores(3);
        EmpezarPartida();
    }

    public void JugarCon4Jugadores()
    {
        SesionPartida.SeleccionarCantidadJugadores(4);
        EmpezarPartida();
    }

    private void EmpezarPartida()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}