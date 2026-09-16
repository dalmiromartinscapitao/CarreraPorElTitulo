using UnityEngine;
using System.Collections;

public class FichaVisual : MonoBehaviour
{
    public float velocidadMovimiento = 5.0f;
    public int PosicionActualVisual { get; private set; } = 0; 

    // NUEVO: Recibe la cantidad de pasos (el resultado del dado) en lugar del ID destino
    public void MoverAdelante(int pasos, Vector3[] posiciones)
    {
        StopAllCoroutines();
        StartCoroutine(CorrutinaMoverAdelante(pasos, posiciones));
    }

    private IEnumerator CorrutinaMoverAdelante(int pasos, Vector3[] posiciones)
    {
        for (int i = 0; i < pasos; i++)
        {
            PosicionActualVisual++;
            
            // Si la ficha visual llega al final del mapa, reinicia a 0 para dar la vuelta hacia adelante
            if (PosicionActualVisual >= posiciones.Length)
            {
                PosicionActualVisual = 0;
            }

            Vector3 destino = posiciones[PosicionActualVisual];
            
            while (Vector3.Distance(transform.position, destino) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
        }
    }
}