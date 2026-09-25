using System.Collections;
using UnityEngine;

public class DadoVisual : MonoBehaviour
{
    [Header("Rotaciones Finales (X, Y, Z)")]
    public Vector3 cara1;
    public Vector3 cara2;
    public Vector3 cara3 = new Vector3(90, 0, 0); 
    public Vector3 cara4;
    public Vector3 cara5 = new Vector3(0, 0, 0);  
    public Vector3 cara6;

    [Header("Ajustes de Posición y Animación")]
    [Tooltip("X: Derecha/Izquierda, Y: Arriba/Abajo, Z: Adelante/Atrás")]
    public Vector3 offsetDesdeJugador = new Vector3(2f, 1f, 0f); // Por defecto: 2 a la derecha, 1 arriba
    
    public float duracionGiro = 1.5f;
    public float velocidadGiroAleatorio = 1200f;
    public float tiempoLectura = 1.2f; 

    // Usamos un arreglo por si el dado tiene varias partes (ej: el cubo y los puntos separados)
    private Renderer[] renderersVisuales;

    private void Awake()
    {
        // Buscamos todos los renderers en este objeto y en sus hijos
        renderersVisuales = GetComponentsInChildren<Renderer>();
        CambiarVisibilidadDado(false);
    }

    public void Lanzar(int resultado, Transform objetivo, System.Action alTerminar)
    {
        StartCoroutine(RutinaGirarDado(resultado, objetivo, alTerminar));
    }

    private IEnumerator RutinaGirarDado(int resultado, Transform objetivo, System.Action alTerminar)
    {
        CambiarVisibilidadDado(true);

        float tiempoAnimacion = 0f;

        // 1. Fase de giro caótico
        while (tiempoAnimacion < duracionGiro)
        {
            tiempoAnimacion += Time.deltaTime;
            
            // Seguir al jugador aplicando el offset exacto que configures en el Inspector
            if (objetivo != null)
            {
                transform.position = objetivo.position + offsetDesdeJugador;
            }

            transform.Rotate(Vector3.one * (velocidadGiroAleatorio * Time.deltaTime));
            yield return null; 
        }

        // 2. Fase de aterrizaje
        Vector3 rotacionFinal = ObtenerRotacionParaCara(resultado);
        transform.rotation = Quaternion.Euler(rotacionFinal);

        if (objetivo != null)
        {
            transform.position = objetivo.position + offsetDesdeJugador;
        }

        // 3. Fase de lectura
        yield return new WaitForSeconds(tiempoLectura);

        // 4. Terminar
        CambiarVisibilidadDado(false);
        alTerminar?.Invoke();
    }

    private void CambiarVisibilidadDado(bool estado)
    {
        if (renderersVisuales == null) return;
        
        foreach (Renderer render in renderersVisuales)
        {
            render.enabled = estado;
        }
    }

    private Vector3 ObtenerRotacionParaCara(int resultado)
    {
        return resultado switch
        {
            1 => cara1,
            2 => cara2,
            3 => cara3,
            4 => cara4,
            5 => cara5,
            6 => cara6,
            _ => Vector3.zero
        };
    }
}