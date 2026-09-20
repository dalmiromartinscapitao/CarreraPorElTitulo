using NUnit.Framework;

public class JugadorTests
{
    [Test]
    public void JugadorPuedeCompletarLaTerceraVuelta()
    {
        // ARRANGE
        ConfiguracionPartida configuracion = ConfiguracionPartida.Crear(ModoPartida.Normal);

        Jugador jugador = new Jugador(1,"Jugador Rojo",configuracion);

        // ACT
        // Llevamos al jugador a la tercera vuelta.
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);

        bool primeraVuelta =
            jugador.IntentarCompletarVuelta();

        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);

        bool segundaVuelta =
            jugador.IntentarCompletarVuelta();

        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);
        jugador.ResponderPregunta(1, 1);

        bool terceraVuelta =
            jugador.IntentarCompletarVuelta();

        // ASSERT
        Assert.IsTrue(
            primeraVuelta,
            "El jugador debería poder completar la primera vuelta."
        );

        Assert.IsTrue(
            segundaVuelta,
            "El jugador debería poder completar la segunda vuelta."
        );

        Assert.IsTrue(
            terceraVuelta,
            "El jugador debería poder completar la tercera vuelta."
        );

        Assert.AreEqual(
            3,
            jugador.RondaActual,
            "El jugador debería encontrarse en la tercera vuelta."
        );
    }


    [Test]
    public void JugadorNoPuedeCompletarVueltaSinRespuestasNecesarias()
    {
        // ARRANGE
        ConfiguracionPartida configuracion = ConfiguracionPartida.Crear(ModoPartida.Normal);

        Jugador jugador = new Jugador(1,"Jugador Rojo",configuracion);

        // Lo llevamos directamente a la tercera vuelta
        // simulando que ya completó las dos anteriores.
        for (int i = 0; i < 3; i++)
        {
            jugador.ResponderPregunta(1, 1);
        }

        jugador.IntentarCompletarVuelta();

        for (int i = 0; i < 4; i++)
        {
            jugador.ResponderPregunta(1, 1);
        }

        jugador.IntentarCompletarVuelta();

        // Ahora está en la tercera vuelta,
        // pero solamente tiene 4 respuestas correctas.
        bool resultado =
            jugador.IntentarCompletarVuelta();

        Assert.IsFalse(
            resultado,
            "El jugador no debería poder completar la tercera vuelta con menos de 5 respuestas correctas."
        );

        Assert.IsTrue(
            jugador.DebeRepetirPreguntas,
            "El jugador debería quedar obligado a repetir preguntas."
        );
    }
}