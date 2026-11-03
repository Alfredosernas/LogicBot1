using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorEjecucion : MonoBehaviour
{
    [SerializeField] private ControladorRobot robot;
    [SerializeField] private GestorCuadricula gestorCuadricula;
    [SerializeField] private AnalizadorTexto analizadorTexto;

    private Queue<TipoComando> colaComandos = new Queue<TipoComando>();
    private bool abortarPorChoque = false;

    public void AgregarComando(TipoComando comando)
    {
        colaComandos.Enqueue(comando);
    }

    public void IniciarSecuencia(List<TipoComando> comandosParaEjecutar)
    {
        StopAllCoroutines();
        abortarPorChoque = false;
        colaComandos = new Queue<TipoComando>(comandosParaEjecutar);
        StartCoroutine(ProcesarCola());
    }

    private IEnumerator ProcesarCola()
    {
        while (colaComandos.Count > 0 && !abortarPorChoque)
        {
            TipoComando comandoActual = colaComandos.Dequeue();

            switch (comandoActual)
            {
                case TipoComando.Avanzar:
                    bool pasoValido = true;
                    yield return StartCoroutine(robot.EjecutarAvanzar(exito => pasoValido = exito));
                    if (!pasoValido)
                    {
                        abortarPorChoque = true;
                        if (analizadorTexto != null)
                        {
                            analizadorTexto.NotificarResultado(false, "ALERTA DE COLISIÓN\n\nEl robot chocó contra una pared o límite del mapa.");
                        }
                    }
                    break;

                case TipoComando.GirarIzquierda:
                    yield return StartCoroutine(robot.EjecutarGirarIzquierda());
                    break;

                case TipoComando.GirarDerecha:
                    yield return StartCoroutine(robot.EjecutarGirarDerecha());
                    break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (!abortarPorChoque)
        {
            ComprobarObjetivo();
        }
    }

    private void ComprobarObjetivo()
    {
        if (gestorCuadricula != null && gestorCuadricula.EsMeta(robot.casillaX, robot.casillaY))
        {
            if (analizadorTexto != null)
            {
                analizadorTexto.NotificarResultado(true, "¡NIVEL COMPLETADO!\n\nHas alcanzado la meta exitosamente.");
            }
        }
        else
        {
            if (analizadorTexto != null)
            {
                analizadorTexto.NotificarResultado(false, "RECORRIDO FINALIZADO\n\nEl robot se detuvo, pero no está en la casilla verde.");
            }
        }
    }
}