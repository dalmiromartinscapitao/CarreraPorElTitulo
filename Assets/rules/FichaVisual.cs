using UnityEngine;
using System.Collections;

public class FichaVisual : MonoBehaviour
{
    public float velocidadMovimiento = 5.0f;
    public int PosicionActualVisual { get; private set; } = 0; 
    
    [Header("Desplazamiento (Evitar choques)")]
    public Vector3 offsetFicha; // Sumará esta distancia al waypoint original

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
            
            if (PosicionActualVisual >= posiciones.Length)
            {
                PosicionActualVisual = 0;
            }

            // Calculamos el destino sumando el punto de la casilla + el offset de esta ficha
            Vector3 destino = posiciones[PosicionActualVisual] + offsetFicha;
            
            while (Vector3.Distance(transform.position, destino) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
        }
    }
}