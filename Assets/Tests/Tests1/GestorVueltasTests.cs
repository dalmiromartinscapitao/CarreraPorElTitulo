using NUnit.Framework;

public class GestorVueltasTests
{
    [Test]
    public void JugadorConRespuestasNecesariasAvanzaDeVuelta()
    {
        ConfiguracionPartida configuracion =
            ConfiguracionPartida.Crear(
                ModoPartida.Normal
            );

        Jugador jugador =
            new Jugador(
                1,
                "Jugador Test",
                configuracion
            );

        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);

        GestorVueltas gestor =
            new GestorVueltas(configuracion);

        ResultadoVuelta resultado =
            gestor.ResolverLlegadaMeta(jugador);

        Assert.AreEqual(
            ResultadoVuelta.AvanzoDeVuelta,
            resultado
        );

        Assert.AreEqual(
            2,
            jugador.RondaActual
        );
    }

    [Test]
    public void JugadorSinRespuestasNecesariasDebeRepetirPreguntas()
    {
        ConfiguracionPartida configuracion =
            ConfiguracionPartida.Crear(
                ModoPartida.Normal
            );

        Jugador jugador =
            new Jugador(
                1,
                "Jugador Test",
                configuracion
            );

        jugador.ResponderPregunta(1, 1);

        GestorVueltas gestor =
            new GestorVueltas(configuracion);

        ResultadoVuelta resultado =
            gestor.ResolverLlegadaMeta(jugador);

        Assert.AreEqual(
            ResultadoVuelta.NecesitaPreguntas,
            resultado
        );

        Assert.IsTrue(
            jugador.DebeRepetirPreguntas
        );
    }

    [Test]
    public void ModoRapidoPermiteGanarEnUnaVuelta()
    {
        ConfiguracionPartida configuracion =
            ConfiguracionPartida.Crear(
                ModoPartida.Rapido
            );

        Jugador jugador =
            new Jugador(
                1,
                "Jugador Test",
                configuracion
            );

        jugador.ResponderPregunta(1, 1);

        GestorVueltas gestor =
            new GestorVueltas(configuracion);

        ResultadoVuelta resultado =
            gestor.ResolverLlegadaMeta(jugador);

        Assert.AreEqual(
            ResultadoVuelta.Gano,
            resultado
        );
    }
}