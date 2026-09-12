using UnityEngine;
using System.Collections;

public class FichaVisual : MonoBehaviour
{
    public float velocidadMovimiento = 5.0f;
    
    // Variable pública que indica en qué casillero físico está el peón
    public int PosicionActualVisual { get; private set; } = 0; 

    // Recibe el ID de destino lógico y el mapa completo de posiciones 3D
    public void MoverACasillero(int idDestino, Vector3[] mapaPosiciones)
    {
        StopAllCoroutines();
        StartCoroutine(MoverCasilleroACasillero(idDestino, mapaPosiciones));
    }

    private IEnumerator MoverCasilleroACasillero(int idDestino, Vector3[] posiciones)
    {
        // Limita el destino al tamaño máximo del arreglo para evitar errores
        idDestino = Mathf.Clamp(idDestino, 0, posiciones.Length - 1);

        // Avanzar casillero por casillero
        while (PosicionActualVisual < idDestino)
        {
            PosicionActualVisual++;
            Vector3 destino = posiciones[PosicionActualVisual];
            
            while (Vector3.Distance(transform.position, destino) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
        }
        
        // Retroceder casillero por casillero (en caso de penalizaciones)
        while (PosicionActualVisual > idDestino)
        {
            PosicionActualVisual--;
            Vector3 destino = posiciones[PosicionActualVisual];
            
            while (Vector3.Distance(transform.position, destino) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
        }
    }
}