public enum ModoPartida
{
    Normal,
    Rapido
}

public class ConfiguracionPartida
{
    public ModoPartida Modo { get; private set; }
    public int VueltasTotales { get; private set; }

    private int[] objetivosPorVuelta;

    private ConfiguracionPartida(
        ModoPartida modo,
        int vueltasTotales,
        int[] objetivos)
    {
        Modo = modo;
        VueltasTotales = vueltasTotales;
        objetivosPorVuelta = objetivos;
    }

    public static ConfiguracionPartida Crear(ModoPartida modo)
    {
        if (modo == ModoPartida.Rapido)
        {
            return new ConfiguracionPartida(
                modo,
                1,
                new int[] { 1 }
            );
        }

        return new ConfiguracionPartida(
            modo,
            3,
            new int[] { 3, 4, 5 }
        );
    }

    public int ObtenerObjetivoDeVuelta(int vuelta)
    {
        int indice = vuelta - 1;

        if (indice < 0)
            indice = 0;

        if (indice >= objetivosPorVuelta.Length)
            indice = objetivosPorVuelta.Length - 1;

        return objetivosPorVuelta[indice];
    }

    public bool EsUltimaVuelta(int vuelta)
    {
        return vuelta >= VueltasTotales;
    }
}