using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuBotones : MonoBehaviour{

    public TMP_Text texto2Jugadores;
    public TMP_Text texto3Jugadores;
    public TMP_Text texto4Jugadores;

    public TMP_Text textoNormal;
    public TMP_Text textoRapido;

    public void SeleccionarModoNormal(){
        SesionPartida.SeleccionarModo(ModoPartida.Normal);

        textoNormal.text = "NORMAL SELECCIONADO";
        textoRapido.text = "RÁPIDO";
    }

    public void SeleccionarModoRapido(){
        SesionPartida.SeleccionarModo(ModoPartida.Rapido);

        textoNormal.text = "NORMAL";
        textoRapido.text = "RÁPIDO SELECCIONADO";
    }

    public void Seleccionar2Jugadores(){
        SesionPartida.SeleccionarCantidadJugadores(2);

        texto2Jugadores.text = "2 JUGADORES SELECCIONADOS";
        texto3Jugadores.text = "3 JUGADORES";
        texto4Jugadores.text = "4 JUGADORES";
    }

    public void Seleccionar3Jugadores(){
        SesionPartida.SeleccionarCantidadJugadores(3);

        texto2Jugadores.text = "2 JUGADORES";
        texto3Jugadores.text = "3 JUGADORES SELECCIONADOS";
        texto4Jugadores.text = "4 JUGADORES";
    }

    public void Seleccionar4Jugadores(){
        SesionPartida.SeleccionarCantidadJugadores(4);

        texto2Jugadores.text = "2 JUGADORES";
        texto3Jugadores.text = "3 JUGADORES";
        texto4Jugadores.text = "4 JUGADORES SELECCIONADOS";
    }

    public void IniciarJuego(){
        SceneManager.LoadScene("Juego");
    }

    public void SalirDelJuego(){
        SceneManager.LoadScene("Menu");
    }

    public void AbrirConfiguracion(){
    SceneManager.LoadScene("Configuracion");
    }

    public void AbrirReglas(){
    SceneManager.LoadScene("Reglas");
    }

    public void SalirDeLaAplicacion(){
        Application.Quit();
    }
    
}