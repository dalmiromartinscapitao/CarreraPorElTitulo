using UnityEngine;

public class Jugador 
{
    // Propiedades principales
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public int PosicionActualId { get; private set; }
    public string Estado { get; private set; }
    public bool TienePenalizacion { get; set; }

    // Constructor
    public Jugador(int id, string nombre, int posicionInicialId = 0)
    {
        Id = id;
        Nombre = nombre;
        PosicionActualId = posicionInicialId;
        Estado = "Esperando Turno";
        TienePenalizacion = false;
    }

    // Lanza un dado de 6 caras e imprime el resultado
    public int LanzarDado()
    {
        if (TienePenalizacion)
        {
            Debug.Log($"[Jugador] {Nombre} está penalizado y no puede lanzar el dado este turno.");
            TienePenalizacion = false; // Se consume la penalización
            Estado = "Esperando Turno";
            return 0;
        }

        int resultado = Random.Range(1, 7); // Genera un entero entre 1 y 6
        Estado = $"Lanzó un {resultado}";
        Debug.Log($"[Dado] {Nombre} tiró el dado y sacó un: {resultado}");
        
        return resultado;
    }

   public void Moverse(int casillerosAMover, int limiteTablero)
    {
        if (casillerosAMover <= 0) return;

        PosicionActualId += casillerosAMover;
        
        // Evitar que la posición supere la meta
        if(PosicionActualId > limiteTablero)
        {
            PosicionActualId = limiteTablero;
        }

        Estado = $"En movimiento a casillero {PosicionActualId}";
        Debug.Log($"[Movimiento] {Nombre} avanza {casillerosAMover} casilleros. Nueva posición ID: {PosicionActualId}");
    }

  
    public void MoverInstantanio(int deltaCasilleros)
    {
        PosicionActualId += deltaCasilleros;
        if (PosicionActualId < 0) PosicionActualId = 0; // Evita posiciones negativas
        
        Debug.Log($"[Efecto] {Nombre} fue desplazado a la casilla ID: {PosicionActualId}");
    }

    // Evalúa la respuesta del jugador ante un casillero de tipo Pregunta
    public bool ResponderPregunta(int opcionSeleccionada, int opcionCorrecta)
    {
        bool esCorrecta = (opcionSeleccionada == opcionCorrecta);

        if (esCorrecta)
        {
            Estado = "Respondió Correctamente";
            Debug.Log($"[Pregunta] ¡{Nombre} respondió correctamente!");
        }
        else
        {
            Estado = "Respondió Incorrectamente";
            Debug.Log($"[Pregunta] {Nombre} se equivocó en la respuesta.");
        }

        return esCorrecta;
    }
}