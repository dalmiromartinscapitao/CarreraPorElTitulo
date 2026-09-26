using System;

public class Jugador
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public int PosicionActualId { get; private set; }
    public string Estado { get; private set; }
    public bool TienePenalizacion { get; private set; }
    public int RondaActual { get; private set; } = 1;
    public int RespuestasCorrectas { get; private set; } = 0;
    public int RespuestasTotales { get; private set; } = 0;
    public bool DebeRepetirPreguntas { get; private set; } = false;
    public bool Gano { get; private set; } = false;

    private ConfiguracionPartida configuracion;

    public Jugador(
        int id,
        string nombre,
        ConfiguracionPartida configuracionPartida,
        int posicionInicialId = 0)
    {
        Id = id;
        Nombre = nombre;
        PosicionActualId = posicionInicialId;
        configuracion = configuracionPartida;

        Estado = "Esperando Turno";
        TienePenalizacion = false;
    }

    public bool EstaPenalizado()
    {
        return TienePenalizacion;
    }

    public void AplicarPenalizacion()
    {
        TienePenalizacion = true;
        Estado = "Penalizado";
    }

    public void CumplirPenalizacion()
    {
        TienePenalizacion = false;
        Estado = "Esperando Turno";
    }

    public void RegistrarTirada(int resultado)
    {
        if (resultado < 1 || resultado > 6)
        {
            throw new ArgumentException(
                "El resultado del dado debe estar entre 1 y 6."
            );
        }

        Estado = $"Lanzó un {resultado}";
    }

    public void Moverse(
        int casillerosAMover,
        int totalCasillerosTablero)
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
    }

    public void EstablecerPosicion(int nuevaPosicion)
    {
        if (nuevaPosicion < 0)
            nuevaPosicion = 0;

        PosicionActualId = nuevaPosicion;

        Estado =
            $"En casillero {PosicionActualId + 1}";
    }

    public void MoverInstantanio(int deltaCasilleros)
    {
        PosicionActualId += deltaCasilleros;

        if (PosicionActualId < 0)
        {
            PosicionActualId = 0;
        }

        Estado =
            $"En casillero {PosicionActualId + 1}";
    }

    public bool ResponderPregunta(
        int opcionSeleccionada,
        int opcionCorrecta)
    {
        bool esCorrecta =
            opcionSeleccionada == opcionCorrecta;

        RespuestasTotales++;

        if (esCorrecta)
        {
            Estado =
                "Respondió Correctamente";

            RespuestasCorrectas++;
        }
        else
        {
            Estado =
                "Respondió Incorrectamente";
        }

        return esCorrecta;
    }

    // =========================================================
    // NUEVO
    // Victoria obtenida mediante el minijuego
    // =========================================================

    public void SumarRespuestaPorMinijuego(){
    {
       
    
    RespuestasCorrectas++;
    RespuestasTotales++;
    
    }

        Estado =
            "Ganó el minijuego y obtuvo una respuesta";

        // Si ya tenía marcada la necesidad de repetir preguntas,
        // verificamos si ahora puede cumplir el objetivo.
        if (RespuestasCorrectas >= ObtenerObjetivoDeRonda())
        {
            DebeRepetirPreguntas = false;
        }
    }

    public int ObtenerObjetivoDeRonda()
    {
        return configuracion.ObtenerObjetivoDeVuelta(
            RondaActual
        );
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
            return false;
        }

        if (configuracion.EsUltimaVuelta(RondaActual))
        {
            DebeRepetirPreguntas = false;
            return true;
        }

        RondaActual++;

        RespuestasCorrectas = 0;
        RespuestasTotales = 0;

        DebeRepetirPreguntas = false;

        PosicionActualId = 0;

        Estado =
            $"Comenzando la vuelta {RondaActual}";

        return true;
    }

    public void MarcarComoGanador()
    {
        Gano = true;
        Estado = "¡GANÓ EL JUEGO!";
    }
}
