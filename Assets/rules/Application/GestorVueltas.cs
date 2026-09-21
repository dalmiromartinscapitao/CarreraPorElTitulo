public enum ResultadoVuelta
{
    NecesitaPreguntas,
    AvanzoDeVuelta,
    Gano
}

public class GestorVueltas
{
    private readonly ConfiguracionPartida configuracion;

    public GestorVueltas(
        ConfiguracionPartida configuracionPartida)
    {
        configuracion = configuracionPartida;
    }

    public ResultadoVuelta ResolverLlegadaMeta(
        Jugador jugador)
    {
        if (!jugador.TieneRespuestasNecesarias())
        {
            jugador.IntentarCompletarVuelta();

            return ResultadoVuelta.NecesitaPreguntas;
        }

        int rondaAntes =
            jugador.RondaActual;

        bool completo =
            jugador.IntentarCompletarVuelta();

        if (!completo)
        {
            return ResultadoVuelta.NecesitaPreguntas;
        }

        if (configuracion.EsUltimaVuelta(rondaAntes))
        {
            return ResultadoVuelta.Gano;
        }

        return ResultadoVuelta.AvanzoDeVuelta;
    }
}