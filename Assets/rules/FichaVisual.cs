using UnityEngine;
using System.Collections;

public class FichaVisual : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5.0f;


    // Índice real del waypoint donde está la ficha.
    // 0 = casillero 1
    // 27 = casillero 28
    public int PosicionActualVisual
    {
        get;
        private set;
    } = 0;


    [Header("Desplazamiento (Evitar choques)")]
    public Vector3 offsetFicha;


    // =========================================================
    // MOVER A UNA POSICIÓN ESPECÍFICA
    // =========================================================

    public void MoverAIndice(
        int indiceDestino,
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
                posiciones
            )
        );
    }


    // =========================================================
    // CORRUTINA DE MOVIMIENTO
    // =========================================================

    private IEnumerator CorrutinaMoverAIndice(
        int indiceDestino,
        Vector3[] posiciones
    )
    {
        // Si ya está en el destino, no hacemos nada.
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


            yield break;
        }


        // Determinar si avanzamos o retrocedemos.
        int direccion =
            PosicionActualVisual <
            indiceDestino
            ? 1
            : -1;


        // -----------------------------------------------------
        // MOVER CASILLERO POR CASILLERO
        // -----------------------------------------------------

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
    }


    // =========================================================
    // SINCRONIZAR INSTANTÁNEAMENTE
    // =========================================================

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


    // =========================================================
    // MÉTODO ANTIGUO
    // =========================================================
    //
    // Lo dejamos para mantener compatibilidad con cualquier
    // otro script que todavía lo esté usando.
    //
    // Ahora NO hace wrap de 28 → 1.

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
            posiciones
        );
    }
}