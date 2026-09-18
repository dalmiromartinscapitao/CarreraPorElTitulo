using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void IniciarJuego()
    {
        SceneManager.LoadScene("Juego");
    }

    public void SalirDelJuego()
    {
        SceneManager.LoadScene("Menu");
    }
}