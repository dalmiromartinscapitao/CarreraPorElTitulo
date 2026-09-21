public static class SesionPartida
{
    public static ModoPartida ModoSeleccionado{
        get;
        private set;
    } = ModoPartida.Normal;

    public static int CantidadJugadores{
        get;
        private set;
    } = 4;

    public static void SeleccionarModo(ModoPartida modo){
        ModoSeleccionado = modo;
    }

    public static void SeleccionarCantidadJugadores(int cantidad){
        if (cantidad < 2){
            cantidad = 2;
        }

        if (cantidad > 4){
            cantidad = 4;
        }

        CantidadJugadores = cantidad;
    }
}