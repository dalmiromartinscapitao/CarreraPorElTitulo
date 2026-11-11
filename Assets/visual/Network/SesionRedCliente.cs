public static class SesionRedCliente
{
    public static string IdSalaActual
    {
        get;
        private set;
    }

    public static void EstablecerSala(
        string idSala)
    {
        IdSalaActual = idSala;
    }
}