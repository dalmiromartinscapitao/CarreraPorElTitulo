using UnityEngine;

public class CamaraSigue : MonoBehaviour
{
    [Tooltip("Arrastra aquí el GameObject de tu Ficha Visual")]
    public Transform objetivo; 
    
    [Tooltip("Distancia relativa entre la cámara y la ficha")]
    public Vector3 offset = new Vector3(0, 5f, -5f); 
    
    [Tooltip("Qué tan suave será el movimiento de la cámara")]
    public float velocidadSuavizado = 5f;

    // Usamos LateUpdate para que la cámara se mueva DESPUÉS de que el peón haya terminado su paso
    private void LateUpdate()
    {
        if (objetivo != null)
        {
            // Calculamos a dónde debería ir la cámara
            Vector3 posicionDeseada = objetivo.position + offset;
            
            // Movemos la cámara suavemente hacia esa posición
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, velocidadSuavizado * Time.deltaTime);
        }
    }
}