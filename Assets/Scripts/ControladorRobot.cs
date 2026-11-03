using System;
using System.Collections;
using UnityEngine;

public class ControladorRobot : MonoBehaviour
{
    [Header("Posición en Cuadrícula")]
    public int casillaX = 0;
    public int casillaY = 0;
    public Direccion direccionActual = Direccion.Este;
    public float velocidadMovimiento = 5f;

    [SerializeField] private GestorCuadricula gestorCuadricula;

    private int casillaInicialX;
    private int casillaInicialY;
    private Direccion direccionInicial;
    private Quaternion rotacionInicial;

    void Start()
    {
        casillaInicialX = casillaX;
        casillaInicialY = casillaY;
        direccionInicial = direccionActual;
        rotacionInicial = transform.rotation;

        if (gestorCuadricula != null)
        {
            transform.position = gestorCuadricula.CuadriculaAPosicionMundo(casillaX, casillaY);
        }
    }

    public void RestaurarPosicionInicial()
    {
        StopAllCoroutines();
        casillaX = casillaInicialX;
        casillaY = casillaInicialY;
        direccionActual = direccionInicial;
        transform.rotation = rotacionInicial;

        if (gestorCuadricula != null)
        {
            transform.position = gestorCuadricula.CuadriculaAPosicionMundo(casillaX, casillaY);
        }
    }

    public IEnumerator EjecutarAvanzar(Action<bool> resultado)
    {
        int destinoX = casillaX;
        int destinoY = casillaY;

        switch (direccionActual)
        {
            case Direccion.Norte: destinoY++; break;
            case Direccion.Este: destinoX++; break;
            case Direccion.Sur: destinoY--; break;
            case Direccion.Oeste: destinoX--; break;
        }

        if (gestorCuadricula != null && gestorCuadricula.EsTransitable(destinoX, destinoY))
        {
            casillaX = destinoX;
            casillaY = destinoY;
            Vector3 posicionDestino = gestorCuadricula.CuadriculaAPosicionMundo(casillaX, casillaY);

            while (Vector3.Distance(transform.position, posicionDestino) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, posicionDestino, velocidadMovimiento * Time.deltaTime);
                yield return null;
            }
            transform.position = posicionDestino;
            resultado?.Invoke(true);
        }
        else
        {
            Debug.LogWarning($"[Colisión] Bloqueado en ({destinoX}, {destinoY}).");
            yield return new WaitForSeconds(0.2f);
            resultado?.Invoke(false);
        }
    }

    public IEnumerator EjecutarGirarIzquierda()
    {
        direccionActual = (Direccion)(((int)direccionActual + 3) % 4);
        transform.Rotate(0, 0, 90);
        yield return new WaitForSeconds(0.15f);
    }

    public IEnumerator EjecutarGirarDerecha()
    {
        direccionActual = (Direccion)(((int)direccionActual + 1) % 4);
        transform.Rotate(0, 0, -90);
        yield return new WaitForSeconds(0.15f);
    }
}