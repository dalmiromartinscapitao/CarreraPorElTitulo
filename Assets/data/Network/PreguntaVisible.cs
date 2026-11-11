using System;

public class PreguntaVisible
{
    public string Texto { get; private set; }
    public string[] Opciones { get; private set; }

    public PreguntaVisible(
        string texto,
        string[] opciones)
    {
        Texto = texto;

        Opciones =
            opciones != null
                ? (string[])opciones.Clone()
                : Array.Empty<string>();
    }

    public static PreguntaVisible CrearDesde(
        PreguntaData pregunta)
    {
        if (pregunta == null)
        {
            return null;
        }

        return new PreguntaVisible(
            pregunta.texto,
            pregunta.opciones
        );
    }
}