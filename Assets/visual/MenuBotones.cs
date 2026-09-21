using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBotones : MonoBehaviour
{
    public void SeleccionarModoNormal(){
        SesionPartida.SeleccionarModo(ModoPartida.Normal);
    }

    public void SeleccionarModoRapido(){
        SesionPartida.SeleccionarModo(ModoPartida.Rapido);
    }

    public void Seleccionar2Jugadores(){
        SesionPartida.SeleccionarCantidadJugadores(2);
    }

    public void Seleccionar3Jugadores(){
        SesionPartida.SeleccionarCantidadJugadores(3);
    }

    public void Seleccionar4Jugadores(){
        SesionPartida.SeleccionarCantidadJugadores(4);
    }

    public void IniciarJuego(){
        SceneManager.LoadScene("Juego");
    }

    public void SalirDelJuego(){
        SceneManager.LoadScene("Menu");
    }

    public void SalirDeLaAplicacion(){
        Application.Quit();
    }
    
}