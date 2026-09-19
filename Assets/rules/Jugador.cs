using UnityEngine;

public class Jugador
{
    // =========================================================
    // PROPIEDADES PRINCIPALES
    // =========================================================

    public int Id { get; private set; }
    public string Nombre { get; private set; }

    // 0 = casillero 1 visualmente
    // 27 = casillero 28 visualmente
    public int PosicionActualId { get; set; }

    public string Estado { get; private set; }

    public bool TienePenalizacion { get; set; }


    // =========================================================
    // CONTROL DE RONDAS
    // =========================================================

    // 1, 2 o 3
    public int RondaActual { get; private set; } = 1;

    // Respuestas correctas de la vuelta actual
    public int RespuestasCorrectas { get; private set; } = 0;

    // Total de preguntas respondidas en la vuelta actual
    public int RespuestasTotales { get; private set; } = 0;


    // =========================================================
    // ESTADO DEL JUGADOR
    // =========================================================

    // Indica si el jugador está obligado a seguir
    // respondiendo preguntas en el casillero 28.
    public bool DebeRepetirPreguntas { get; private set; } = false;

    // Indica si ganó el juego.
    public bool Gano { get; private set; } = false;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

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


    // =========================================================
    // LANZAR DADO
    // =========================================================

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


    // =========================================================
    // MOVIMIENTO NORMAL
    // =========================================================

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


        // IMPORTANTE:
        // Ya NO damos la vuelta automáticamente.
        //
        // Si supera el casillero 28,
        // se queda en el casillero 28.
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


    // =========================================================
    // MOVIMIENTO ESPECIAL
    // =========================================================

    public void MoverInstantanio(
        int deltaCasilleros
    )
    {
        PosicionActualId += deltaCasilleros;


        // Nunca puede quedar antes del casillero 1.
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


    // =========================================================
    // RESPONDER PREGUNTA
    // =========================================================

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


    // =========================================================
    // OBJETIVO DE CADA VUELTA
    // =========================================================

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


    // =========================================================
    // ¿TIENE LAS RESPUESTAS NECESARIAS?
    // =========================================================

    public bool TieneRespuestasNecesarias()
    {
        return
            RespuestasCorrectas >=
            ObtenerObjetivoDeRonda();
    }


    // =========================================================
    // RESPUESTAS QUE FALTAN
    // =========================================================

    public int ObtenerRespuestasFaltantes()
    {
        int faltantes =
            ObtenerObjetivoDeRonda() -
            RespuestasCorrectas;


        if (faltantes < 0)
            faltantes = 0;


        return faltantes;
    }


    // =========================================================
    // INTENTAR COMPLETAR LA VUELTA
    // =========================================================

    public bool IntentarCompletarVuelta()
    {
        // -----------------------------------------------------
        // NO TIENE LAS RESPUESTAS NECESARIAS
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // TIENE LAS RESPUESTAS NECESARIAS
        // -----------------------------------------------------

        Debug.Log(
            $"[Ronda] {Nombre} tiene " +
            $"{RespuestasCorrectas} correctas " +
            $"y necesita solamente " +
            $"{ObtenerObjetivoDeRonda()}."
        );


        // -----------------------------------------------------
        // TERCERA VUELTA
        // -----------------------------------------------------
        //
        // Como ya está en la tercera vuelta y tiene
        // >= 5 respuestas correctas, completó el juego.
        // GameManager se encargará de declararlo ganador.

        if (RondaActual == 3)
        {
            DebeRepetirPreguntas = false;

            Debug.Log(
                $"[Ronda] {Nombre} completó " +
                "la tercera vuelta."
            );

            return true;
        }


        // -----------------------------------------------------
        // PASAR A LA SIGUIENTE VUELTA
        // -----------------------------------------------------

        RondaActual++;


        // Reiniciar preguntas para la nueva vuelta.
        RespuestasCorrectas = 0;
        RespuestasTotales = 0;


        // Ya no está bloqueado.
        DebeRepetirPreguntas = false;


        // Volver al casillero 1.
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


    // =========================================================
    // MARCAR COMO GANADOR
    // =========================================================

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