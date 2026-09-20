using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBotones : MonoBehaviour
{
    public void SeleccionarModoNormal()
    {
        SesionPartida.SeleccionarModo(ModoPartida.Normal);
    }

    public void SeleccionarModoRapido()
    {
        SesionPartida.SeleccionarModo(ModoPartida.Rapido);
    }

    public void IniciarJuego()
    {
        SceneManager.LoadScene("Juego");
    }

    public void SalirDelJuego()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SalirDeLaAplicacion()
    {
        Application.Quit();
    }
    
}