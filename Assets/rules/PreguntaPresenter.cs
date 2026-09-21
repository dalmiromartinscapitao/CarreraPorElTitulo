using System;

public interface IPreguntaView
{
    event Action<int> OpcionSeleccionada;

    void MostrarPreguntaEnPantalla(PreguntaData pregunta);
    void OcultarPregunta();
}

public class PreguntaPresenter
{
    private readonly IPreguntaView vista;
    private readonly BancoPreguntas banco;
    private readonly Random aleatorio = new Random();

    private Jugador jugadorActual;
    private PreguntaData preguntaActual;

    public event Action<Jugador, bool> RespuestaProcesada;

    public PreguntaPresenter(
        IPreguntaView vistaPregunta,
        BancoPreguntas bancoPreguntas)
    {
        vista = vistaPregunta;
        banco = bancoPreguntas;

        vista.OpcionSeleccionada += ProcesarRespuesta;
    }

    public void MostrarPregunta(Jugador jugador)
    {
        if (banco == null || banco.preguntas == null ||
            banco.preguntas.Length == 0)
        {
            return;
        }

        jugadorActual = jugador;

        int indice = aleatorio.Next(banco.preguntas.Length);
        preguntaActual = banco.preguntas[indice];

        vista.MostrarPreguntaEnPantalla(preguntaActual);
    }

    private void ProcesarRespuesta(int opcionSeleccionada)
    {
        if (jugadorActual == null || preguntaActual == null)
        {
            return;
        }

        bool esCorrecta = jugadorActual.ResponderPregunta(
            opcionSeleccionada,
            preguntaActual.indiceCorrecta
        );

        Jugador jugadorQueRespondio = jugadorActual;

        jugadorActual = null;
        preguntaActual = null;

        vista.OcultarPregunta();

        RespuestaProcesada?.Invoke(
            jugadorQueRespondio,
            esCorrecta
        );
    }
}