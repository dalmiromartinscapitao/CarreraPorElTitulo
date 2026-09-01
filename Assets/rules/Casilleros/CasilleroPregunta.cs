using System.Collections.Generic;
using UnityEngine;

public class CasilleroPregunta : CasilleroBase
{
    public CasilleroPregunta(int id, List<int> siguientesIds) : base(id, siguientesIds)
    {
        Tipo = TipoCasillero.Pregunta;
    }

    public override void EjecutarEfecto(Jugador jugador)
    {
        Debug.Log($"[Pregunta] {jugador.Nombre} cayó en un casillero de Pregunta (ID: {Id}).");
        
        // Simulación: le enviamos la opción 1 y la correcta es la 1
        bool acerto = jugador.ResponderPregunta(opcionSeleccionada: 1, opcionCorrecta: 1);

        if (acerto)
        {
            Debug.Log($"¡{jugador.Nombre} respondió bien!");
        }
        else
        {
            Debug.Log($"{jugador.Nombre} falló la pregunta.");
        }
    }
}