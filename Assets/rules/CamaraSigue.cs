using UnityEngine;

public class CamaraSigue : MonoBehaviour
{
    [Tooltip("Arrastra aquí el GameObject de tu Ficha Visual")]
    public Transform objetivo; 
    
    [Tooltip("Distancia relativa entre la cámara y la ficha")]
    public Vector3 offset = new Vector3(0, 5f, -5f); 
    
    [Tooltip("Qué tan suave será el movimiento de la cámara")]
    public float velocidadSuavizado = 5f;

    
    private void LateUpdate()
    {
        if (objetivo != null)
        {
       
            Vector3 posicionDeseada = objetivo.position + offset;
            
          
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, velocidadSuavizado * Time.deltaTime);
        }
    }
}