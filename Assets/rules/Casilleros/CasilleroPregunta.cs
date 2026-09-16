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
        
        // Llama al Singleton de la UI para mostrar el recuadro
        UIPreguntas.Instancia.MostrarPregunta(jugador);
    }
}