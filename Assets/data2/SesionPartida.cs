public static class SesionPartida
{
    public static ModoPartida ModoSeleccionado
    {
        get;
        private set;
    } = ModoPartida.Normal;

    public static void SeleccionarModo(ModoPartida modo)
    {
        ModoSeleccionado = modo;
    }
}