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
    private Vector3 escalaInicial;
    private bool configuradoPorNivel = false;

    void Awake()
    {
        escalaInicial = transform.localScale;
    }

    void Start()
    {
        if (!configuradoPorNivel)
        {
            EstablecerEstadoInicial(casillaX, casillaY, direccionActual);
        }
    }

    public void EstablecerEstadoInicial(int x, int y, Direccion dir)
    {
        configuradoPorNivel = true;
        casillaInicialX = x;
        casillaInicialY = y;
        direccionInicial = dir;
        rotacionInicial = DireccionARotacion(dir);

        RestaurarPosicionInicial();
    }

    private Quaternion DireccionARotacion(Direccion dir)
    {
        switch (dir)
        {
            case Direccion.Norte: return Quaternion.Euler(0, 0, 90);
            case Direccion.Oeste: return Quaternion.Euler(0, 0, 180);
            case Direccion.Sur: return Quaternion.Euler(0, 0, 270);
            case Direccion.Este:
            default: return Quaternion.Euler(0, 0, 0);
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
            gestorCuadricula.ReiniciarElementosNivel();
            transform.position = gestorCuadricula.CuadriculaAPosicionMundo(casillaX, casillaY);
            transform.localScale = escalaInicial * gestorCuadricula.ObtenerFactorEscala();
            gestorCuadricula.ComprobarYRecogerNodo(casillaX, casillaY);
        }
    }

    public bool HayObstaculoAlFrente()
    {
        int frenteX = casillaX;
        int frenteY = casillaY;

        switch (direccionActual)
        {
            case Direccion.Norte: frenteY++; break;
            case Direccion.Este: frenteX++; break;
            case Direccion.Sur: frenteY--; break;
            case Direccion.Oeste: frenteX--; break;
        }

        if (gestorCuadricula == null) return true;
        return !gestorCuadricula.EsTransitable(frenteX, frenteY);
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
            gestorCuadricula.ComprobarYRecogerNodo(casillaX, casillaY);
            resultado?.Invoke(true);
        }
        else
        {
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