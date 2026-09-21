[System.Serializable]
public class PreguntaData
{
    public string texto;
    public string[] opciones;
    public int indiceCorrecta;
}

[System.Serializable]
public class BancoPreguntas
{
    public PreguntaData[] preguntas;
}