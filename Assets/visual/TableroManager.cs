using System.Collections.Generic;
using UnityEngine;

public class TableroManager : MonoBehaviour
{
    public List<CasilleroBase> ListaCasilleros{
        get;
        private set;
    } 
    = new List<CasilleroBase>();


    public void InicializarTablero(int cantidadCasilleros){
        ListaCasilleros.Clear();

        if (cantidadCasilleros <= 0)
            return;


        for (int i = 0;
            i < cantidadCasilleros;i++){
            int siguiente = i + 1;

            List<int> siguientesIds;

            if (i == cantidadCasilleros - 1){
                siguientesIds = new List<int>();
            }
            else{
                siguientesIds = new List<int>{
                    siguiente
                };
            }

            if (i == 0 || i == cantidadCasilleros - 1){
                ListaCasilleros.Add(new CasilleroNormal(i,siguientesIds));
            }
            else if (i % 3 == 0){
                ListaCasilleros.Add(new CasilleroPregunta(i,siguientesIds));
            }
            else if (i % 5 == 0){
                ListaCasilleros.Add(new CasilleroEspecial(i,siguientesIds));
            }
            else{
                ListaCasilleros.Add(new CasilleroNormal(i,siguientesIds));
            }
        }
    }


    public CasilleroBase ObtenerCasillero(int idCasillero){
        return ListaCasilleros.Find(c => c.Id == idCasillero);
    }


    public void EvaluarCasillero(int idCasillero,Jugador jugador){
        CasilleroBase casilleroActual = ObtenerCasillero(idCasillero);

        if (casilleroActual != null){
            casilleroActual.EjecutarEfecto(jugador);
        }
    }
}