using UnityEngine;
using System.Collections;

public class FichaVisual : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5.0f;

    public int PosicionActualVisual
    {
        get;
        private set;
    } = 0;

    [Header("Desplazamiento (Evitar choques)")]
    public Vector3 offsetFicha;

    public void MoverAIndice(
        int indiceDestino,
        Vector3[] posiciones,
        System.Action alTerminarMovimiento = null
    )
    {
        if (
            posiciones == null ||
            posiciones.Length == 0
        )
        {
            alTerminarMovimiento?.Invoke();
            return;
        }

        indiceDestino =
            Mathf.Clamp(
                indiceDestino,
                0,
                posiciones.Length - 1
            );

        StopAllCoroutines();

        StartCoroutine(
            CorrutinaMoverAIndice(
                indiceDestino,
                posiciones,
                alTerminarMovimiento
            )
        );
    }

    private IEnumerator CorrutinaMoverAIndice(
        int indiceDestino,
        Vector3[] posiciones,
        System.Action alTerminarMovimiento
    )
    {
        if (
            PosicionActualVisual ==
            indiceDestino
        )
        {
            Vector3 destinoFinal =
                posiciones[indiceDestino] +
                offsetFicha;

            while (
                Vector3.Distance(
                    transform.position,
                    destinoFinal
                ) > 0.01f
            )
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        destinoFinal,
                        velocidadMovimiento *
                        Time.deltaTime
                    );

                yield return null;
            }

            transform.position =
                destinoFinal;

            alTerminarMovimiento?.Invoke();
            yield break;
        }

        int direccion =
            PosicionActualVisual <
            indiceDestino
            ? 1
            : -1;

        while (
            PosicionActualVisual !=
            indiceDestino
        )
        {
            PosicionActualVisual +=
                direccion;

            Vector3 destino =
                posiciones[
                    PosicionActualVisual
                ] +
                offsetFicha;

            while (
                Vector3.Distance(
                    transform.position,
                    destino
                ) > 0.01f
            )
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        destino,
                        velocidadMovimiento *
                        Time.deltaTime
                    );

                yield return null;
            }

            transform.position =
                destino;
        }

        alTerminarMovimiento?.Invoke();
    }

    public void SincronizarPosicion(
        int indice,
        Vector3[] posiciones
    )
    {
        if (
            posiciones == null ||
            posiciones.Length == 0
        )
        {
            return;
        }

        indice =
            Mathf.Clamp(
                indice,
                0,
                posiciones.Length - 1
            );

        StopAllCoroutines();

        PosicionActualVisual =
            indice;

        transform.position =
            posiciones[indice] +
            offsetFicha;
    }

    public void MoverAdelante(
        int pasos,
        Vector3[] posiciones
    )
    {
        if (
            posiciones == null ||
            posiciones.Length == 0
        )
        {
            return;
        }

        int destino =
            PosicionActualVisual + pasos;

        destino =
            Mathf.Clamp(
                destino,
                0,
                posiciones.Length - 1
            );

        MoverAIndice(
            destino,
            posiciones,
            null
        );
    }
}