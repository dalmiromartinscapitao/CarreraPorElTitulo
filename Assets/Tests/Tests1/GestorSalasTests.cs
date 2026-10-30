using NUnit.Framework;

public class GestorSalasTests
{
    [Test]
    public void CrearSalaValidaAgregaAlCreadorComoJugador()
    {
        GestorSalas gestor =
            new GestorSalas();

        Sala sala = gestor.CrearSala(
            10,
            "Carrera Universitaria",
            4
        );

        Assert.IsNotNull(sala);
        Assert.AreEqual("SALA-001", sala.Id);
        Assert.AreEqual(1, sala.Jugadores.Count);
        Assert.AreEqual(10, sala.Jugadores[0]);
    }

    [Test]
    public void SalaEnJuegoPermiteUnirseSoloComoEspectador()
    {
        Sala sala = new Sala(
            "SALA-001",
            "Sala Test",
            10,
            2
        );

        sala.IntentarUnir(11);

        bool inicioCorrecto =
            sala.IntentarIniciar(10);

        ResultadoUnionSala resultado =
            sala.IntentarUnir(12);

        Assert.IsTrue(inicioCorrecto);
        Assert.AreEqual(
            ResultadoUnionSala.Espectador,
            resultado
        );
        Assert.AreEqual(1, sala.Espectadores.Count);
    }

    [Test]
    public void CrearSalaRechazaCantidadInvalidaDeJugadores()
    {
        GestorSalas gestor =
            new GestorSalas();

        Sala sala = gestor.CrearSala(
            10,
            "Sala Inválida",
            5
        );

        Assert.IsNull(sala);
    }
}