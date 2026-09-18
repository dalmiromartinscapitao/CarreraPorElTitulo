using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void IniciarJuego()
    {
        SceneManager.LoadScene("Juego");
    }

    public void SalirDelJuego() //Volver al menu principal
    {
        SceneManager.LoadScene("Menu");
    }

    public void SalirDeLaAplicacion() //Cierra el juego por completo
    {
        Application.Quit();
    }
}