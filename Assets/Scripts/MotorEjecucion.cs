using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorEjecucion : MonoBehaviour
{
    [SerializeField] private ControladorRobot robot;
    [SerializeField] private GestorCuadricula gestorCuadricula;
    [SerializeField] private AnalizadorTexto analizadorTexto;
    [SerializeField] private GestorNiveles gestorNiveles;

    public bool AbortarPorChoque { get; set; } = false;

    public void IniciarSecuencia(List<Instruccion> instruccionesParaEjecutar)
    {
        StopAllCoroutines();
        AbortarPorChoque = false;
        StartCoroutine(EjecutarArbolInstrucciones(instruccionesParaEjecutar));
    }

    public void DetenerSecuencia()
    {
        StopAllCoroutines();
        AbortarPorChoque = false;
    }

    private IEnumerator EjecutarArbolInstrucciones(List<Instruccion> instrucciones)
    {
        foreach (Instruccion inst in instrucciones)
        {
            if (AbortarPorChoque) yield break;
            yield return StartCoroutine(inst.Ejecutar(this, robot));
        }

        if (!AbortarPorChoque)
        {
            ComprobarObjetivo();
        }
    }

    public void NotificarColision()
    {
        if (analizadorTexto != null)
        {
            analizadorTexto.NotificarResultado(false, "ALERTA DE COLISIÓN\n\nEl robot chocó contra una pared o límite del mapa.");
        }
        if (gestorNiveles != null)
        {
            gestorNiveles.OcultarBotonSiguiente();
        }
    }

    private void ComprobarObjetivo()
    {
        if (gestorCuadricula != null && gestorCuadricula.EsMeta(robot.casillaX, robot.casillaY))
        {
            if (gestorCuadricula.MetaDesbloqueada())
            {
                if (analizadorTexto != null)
                {
                    analizadorTexto.NotificarResultado(true, "¡NIVEL COMPLETADO!\n\nHas alcanzado la meta exitosamente.");
                }
                if (gestorNiveles != null)
                {
                    gestorNiveles.NivelCompletado();
                }
            }
            else
            {
                if (analizadorTexto != null)
                {
                    analizadorTexto.NotificarResultado(false, "META BLOQUEADA\n\nDebes recoger todos los nodos de energía amarillos antes de entrar en la meta.");
                }
                if (gestorNiveles != null)
                {
                    gestorNiveles.OcultarBotonSiguiente();
                }
            }
        }
        else
        {
            if (analizadorTexto != null)
            {
                analizadorTexto.NotificarResultado(false, "RECORRIDO FINALIZADO\n\nEl robot se detuvo, pero no está en la casilla verde.");
            }
            if (gestorNiveles != null)
            {
                gestorNiveles.OcultarBotonSiguiente();
            }
        }
    }
}

public abstract class Instruccion
{
    public abstract IEnumerator Ejecutar(MotorEjecucion motor, ControladorRobot robot);
}

public class InstruccionAccion : Instruccion
{
    private TipoComando comando;

    public InstruccionAccion(TipoComando comando)
    {
        this.comando = comando;
    }

    public override IEnumerator Ejecutar(MotorEjecucion motor, ControladorRobot robot)
    {
        if (motor.AbortarPorChoque) yield break;

        switch (comando)
        {
            case TipoComando.Avanzar:
                bool pasoValido = true;
                yield return motor.StartCoroutine(robot.EjecutarAvanzar(exito => pasoValido = exito));
                if (!pasoValido)
                {
                    motor.AbortarPorChoque = true;
                    motor.NotificarColision();
                }
                break;

            case TipoComando.GirarIzquierda:
                yield return motor.StartCoroutine(robot.EjecutarGirarIzquierda());
                break;

            case TipoComando.GirarDerecha:
                yield return motor.StartCoroutine(robot.EjecutarGirarDerecha());
                break;
        }

        yield return new WaitForSeconds(0.1f);
    }
}

public class InstruccionBucle : Instruccion
{
    private int repeticiones;
    private List<Instruccion> cuerpo;

    public InstruccionBucle(int repeticiones, List<Instruccion> cuerpo)
    {
        this.repeticiones = repeticiones;
        this.cuerpo = cuerpo;
    }

    public override IEnumerator Ejecutar(MotorEjecucion motor, ControladorRobot robot)
    {
        for (int i = 0; i < repeticiones; i++)
        {
            if (motor.AbortarPorChoque) yield break;

            foreach (Instruccion inst in cuerpo)
            {
                if (motor.AbortarPorChoque) yield break;
                yield return motor.StartCoroutine(inst.Ejecutar(motor, robot));
            }
        }
    }
}

public class InstruccionCondicional : Instruccion
{
    private List<Instruccion> bloqueSi;
    private List<Instruccion> bloqueSino;

    public InstruccionCondicional(List<Instruccion> bloqueSi, List<Instruccion> bloqueSino = null)
    {
        this.bloqueSi = bloqueSi;
        this.bloqueSino = bloqueSino;
    }

    public override IEnumerator Ejecutar(MotorEjecucion motor, ControladorRobot robot)
    {
        if (motor.AbortarPorChoque) yield break;

        bool hayPared = robot.HayObstaculoAlFrente();
        List<Instruccion> bloqueAEjecutar = hayPared ? bloqueSi : bloqueSino;

        if (bloqueAEjecutar != null)
        {
            foreach (Instruccion inst in bloqueAEjecutar)
            {
                if (motor.AbortarPorChoque) yield break;
                yield return motor.StartCoroutine(inst.Ejecutar(motor, robot));
            }
        }
    }
}