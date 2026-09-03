using UnityEngine;
using System.Collections;

public class FichaVisual : MonoBehaviour
{
    [Tooltip("Distancia en el eje X entre cada casillero")]
    public float distanciaPorCasillero = 2.0f; 
    public float velocidadMovimiento = 5.0f;

    // Recibe el ID de la posición lógica del jugador
    public void ActualizarPosicionVisual(int nuevaPosicionId)
    {
        // Calcula la nueva posición en X, manteniendo la Y y Z originales
        float nuevaPosX = nuevaPosicionId * distanciaPorCasillero;
        Vector3 destino = new Vector3(nuevaPosX, transform.position.y, transform.position.z);
        
        StopAllCoroutines();
        StartCoroutine(MoverSuave(destino));
    }

    private IEnumerator MoverSuave(Vector3 destino)
    {
        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
            yield return null;
        }
    }
}