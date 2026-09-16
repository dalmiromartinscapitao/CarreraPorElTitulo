using UnityEngine;

public class Jugador 
{
    // Propiedades principales
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public int PosicionActualId { get; set; } // Permitimos modificarlo desde el GameManager si es necesario
    public string Estado { get; private set; }
    public bool TienePenalizacion { get; set; }
    
    // Control de rendimiento y rondas
    public int RondaActual { get; private set; } = 1;
    public int RespuestasCorrectas { get; private set; } = 0;
    public int RespuestasTotales { get; private set; } = 0;

    // Constructor
    public Jugador(int id, string nombre, int posicionInicialId = 0)
    {
        Id = id;
        Nombre = nombre;
        PosicionActualId = posicionInicialId;
        Estado = "Esperando Turno";
        TienePenalizacion = false;
    }

    // Lanza un dado de 6 caras
    public int LanzarDado()
    {
        if (TienePenalizacion)
        {
            Debug.Log($"[Jugador] {Nombre} está penalizado y no puede lanzar el dado este turno.");
            TienePenalizacion = false; 
            Estado = "Esperando Turno";
            return 0;
        }

        int resultado = Random.Range(1, 7); // Genera un entero entre 1 y 6
        Estado = $"Lanzó un {resultado}";
        Debug.Log($"[Dado] {Nombre} tiró el dado y sacó un: {resultado}");
        
        return resultado;
    }

    // Movimiento estrictamente hacia adelante usando módulo para dar la vuelta en el tablero de 28 casilleros (0 al 27)
    public void Moverse(int casillerosAMover, int totalCasillerosTablero)
    {
        if (casillerosAMover <= 0) return;

        int nuevaPosicion = PosicionActualId + casillerosAMover;
        
        // Aplica módulo para dar la vuelta cíclicamente hacia adelante
        PosicionActualId = nuevaPosicion % totalCasillerosTablero;

        Estado = $"En movimiento a casillero {PosicionActualId}";
        Debug.Log($"[Movimiento] {Nombre} avanzó {casillerosAMover} casilleros. Nueva posición ID: {PosicionActualId} (Tablero de {totalCasillerosTablero} casilleros)");
    }

    public void MoverInstantanio(int deltaCasilleros)
    {
        PosicionActualId += deltaCasilleros;
        if (PosicionActualId < 0) PosicionActualId = 0; 
        
        Debug.Log($"[Efecto] {Nombre} fue desplazado a la casilla ID: {PosicionActualId}");
    }

    // Registra la respuesta a una pregunta
    public bool ResponderPregunta(int opcionSeleccionada, int opcionCorrecta)
    {
        bool esCorrecta = (opcionSeleccionada == opcionCorrecta);
        RespuestasTotales++; 

        if (esCorrecta)
        {
            Estado = "Respondió Correctamente";
            RespuestasCorrectas++; 
            Debug.Log($"[Pregunta] ¡{Nombre} respondió correctamente! Correctas: {RespuestasCorrectas}/{RespuestasTotales}");
        }
        else
        {
            Estado = "Respondió Incorrectamente";
            Debug.Log($"[Pregunta] {Nombre} se equivocó. Correctas: {RespuestasCorrectas}/{RespuestasTotales}");
        }

        return esCorrecta;
    }

    // Evalúa si supera estrictamente el 70% para pasar de ronda
    public bool IntentarAvanzarDeRonda()
    {
        if (RespuestasTotales == 0)
        {
            Debug.Log($"[Ronda] {Nombre} no ha respondido ninguna pregunta todavía, por lo que no se puede evaluar la ronda.");
            return false;
        }

        float porcentajeAciertos = ((float)RespuestasCorrectas / RespuestasTotales) * 100f;
        float porcentajeRedondeado = Mathf.Round(porcentajeAciertos * 100f) / 100f;

        // Exige estrictamente MÁS del 70% (> 70)
        if (porcentajeRedondeado > 70f)
        {
            RondaActual++;
            Debug.Log($"[Ronda - ÉXITO] ¡{Nombre} superó el porcentaje con un {porcentajeRedondeado}%! Avanza a la Ronda {RondaActual}.");
            
            // Reiniciamos los contadores para la nueva ronda
            RespuestasTotales = 0;
            RespuestasCorrectas = 0;
            return true;
        }
        else
        {
            Debug.Log($"[Ronda - FALLO] {Nombre} obtuvo un {porcentajeRedondeado}%. Necesita estrictamente más del 70% para avanzar.");
            return false;
        }
    }
}