using UnityEngine;
using TMPro;

public class VictoriaManager : MonoBehaviour
{
    public static string NombreGanador;

    public TMP_Text textoGanador;

    private void Start()
    {
        if (textoGanador != null)
        {
            textoGanador.text = "¡" + NombreGanador + " ganó!";
        }
    }
}