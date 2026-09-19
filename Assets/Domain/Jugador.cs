using UnityEngine;

public class Jugador
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public int PosicionActualId { get; set; }
    public string Estado { get; private set; }
    public bool TienePenalizacion { get; set; }
    public int RondaActual { get; private set; } = 1;
    public int RespuestasCorrectas { get; private set; } = 0;
    public int RespuestasTotales { get; private set; } = 0;
    public bool DebeRepetirPreguntas { get; private set; } = false;
    public bool Gano { get; private set; } = false;
    public Jugador(
        int id,
        string nombre,
        int posicionInicialId = 0
    )
    {
        Id = id;
        Nombre = nombre;
        PosicionActualId = posicionInicialId;

        Estado = "Esperando Turno";
        TienePenalizacion = false;
    }
    public int LanzarDado()
    {
        if (TienePenalizacion)
        {
            Debug.Log(
                $"[Jugador] {Nombre} está penalizado " +
                "y no puede lanzar el dado este turno."
            );

            TienePenalizacion = false;
            Estado = "Esperando Turno";
            return 0;
        }
        int resultado = Random.Range(1, 7);
        Estado = $"Lanzó un {resultado}";
        Debug.Log(
            $"[Dado] {Nombre} tiró el dado y sacó un: {resultado}"
        );
        return resultado;
    }
    public void Moverse(
        int casillerosAMover,
        int totalCasillerosTablero
    )
    {
        if (casillerosAMover <= 0)
            return;
        if (totalCasillerosTablero <= 0)
            return;
        int nuevaPosicion =
            PosicionActualId + casillerosAMover;
        int ultimaPosicion =
            totalCasillerosTablero - 1;
        if (nuevaPosicion >= ultimaPosicion)
        {
            PosicionActualId = ultimaPosicion;
        }
        else
        {
            PosicionActualId = nuevaPosicion;
        }
        Estado =
            $"En casillero {PosicionActualId + 1}";
        Debug.Log(
            $"[Movimiento] {Nombre} avanzó " +
            $"{casillerosAMover} casilleros. " +
            $"Nueva posición ID: {PosicionActualId} " +
            $"(Casillero visual: {PosicionActualId + 1})"
        );
    }
    public void EstablecerPosicion(int nuevaPosicion)
{
    PosicionActualId = nuevaPosicion;
    Debug.Log(
        $"[Posición] {Nombre} ahora está en el casillero " +
        $"{PosicionActualId + 1}"
    );
}
    public void MoverInstantanio(
        int deltaCasilleros
    )
    {
        PosicionActualId += deltaCasilleros;
        if (PosicionActualId < 0)
        {
            PosicionActualId = 0;
        }
        Debug.Log(
            $"[Efecto] {Nombre} fue desplazado " +
            $"al casillero ID: {PosicionActualId} " +
            $"(Visual: {PosicionActualId + 1})"
        );
    }
    public bool ResponderPregunta(
        int opcionSeleccionada,
        int opcionCorrecta
    )
    {
        bool esCorrecta =
            opcionSeleccionada == opcionCorrecta;
        RespuestasTotales++;
        if (esCorrecta)
        {
            Estado = "Respondió Correctamente";
            RespuestasCorrectas++;
            Debug.Log(
                $"[Pregunta] ¡{Nombre} respondió correctamente! " +
                $"Correctas: {RespuestasCorrectas} | " +
                $"Objetivo: {ObtenerObjetivoDeRonda()}"
            );
        }
        else
        {
            Estado = "Respondió Incorrectamente";
            Debug.Log(
                $"[Pregunta] {Nombre} se equivocó. " +
                $"Correctas: {RespuestasCorrectas} | " +
                $"Objetivo: {ObtenerObjetivoDeRonda()}"
            );
        }
        return esCorrecta;
    }
    public int ObtenerObjetivoDeRonda()
    {
        switch (RondaActual)
        {
            case 1:
                return 3;

            case 2:
                return 4;

            case 3:
                return 5;

            default:
                return 5;
        }
    }
    public bool TieneRespuestasNecesarias()
    {
        return
            RespuestasCorrectas >=
            ObtenerObjetivoDeRonda();
    }
    public int ObtenerRespuestasFaltantes()
    {
        int faltantes =
            ObtenerObjetivoDeRonda() -
            RespuestasCorrectas;


        if (faltantes < 0)
            faltantes = 0;


        return faltantes;
    }
    public bool IntentarCompletarVuelta()
    {
        if (!TieneRespuestasNecesarias())
        {
            DebeRepetirPreguntas = true;
            Debug.Log(
                $"[Ronda] {Nombre} NO puede completar " +
                $"la vuelta {RondaActual}."
            );
            Debug.Log(
                $"[Ronda] Tiene {RespuestasCorrectas} " +
                $"correctas y necesita " +
                $"{ObtenerObjetivoDeRonda()}."
            );
            Debug.Log(
                $"[Ronda] Le faltan " +
                $"{ObtenerRespuestasFaltantes()} " +
                $"respuestas correctas."
            );
            return false;
        }
        Debug.Log(
            $"[Ronda] {Nombre} tiene " +
            $"{RespuestasCorrectas} correctas " +
            $"y necesita solamente " +
            $"{ObtenerObjetivoDeRonda()}."
        );
        if (RondaActual == 3)
        {
            DebeRepetirPreguntas = false;
            Debug.Log(
                $"[Ronda] {Nombre} completó " +
                "la tercera vuelta."
            );
            return true;
        }
        RondaActual++;
        RespuestasCorrectas = 0;
        RespuestasTotales = 0;
        DebeRepetirPreguntas = false;
        PosicionActualId = 0;
        Estado =
            $"Comenzando la vuelta {RondaActual}";
        Debug.Log(
            $"[Ronda] ¡{Nombre} pasó a la " +
            $"Vuelta {RondaActual}!"
        );
        Debug.Log(
            $"[Ronda] {Nombre} vuelve al " +
            "casillero 1."
        );
        return true;
    }
    public void MarcarComoGanador()
    {
        Gano = true;
        Estado = "¡GANÓ EL JUEGO!";
        Debug.Log(
            $"[VICTORIA] ¡{Nombre} completó " +
            "las 3 vueltas y ganó el juego!"
        );
    }
}