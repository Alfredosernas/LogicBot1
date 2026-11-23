using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnalizadorTexto : MonoBehaviour
{
    [Header("Conexiones")]
    [SerializeField] private MotorEjecucion motorEjecucion;
    [SerializeField] private ControladorRobot controladorRobot;
    [SerializeField] private TMP_InputField campoConsolaEntrada;
    [SerializeField] private TextMeshProUGUI textoRetroalimentacion;
    [SerializeField] private GestorNiveles gestorNiveles;

    public void ProcesarInstrucciones()
    {
        if (campoConsolaEntrada == null || string.IsNullOrWhiteSpace(campoConsolaEntrada.text))
        {
            MostrarMensaje("Error: Escribe un comando antes de ejecutar.", Color.yellow);
            return;
        }

        if (controladorRobot != null)
        {
            controladorRobot.RestaurarPosicionInicial();
        }

        string[] lineasDeCodigo = campoConsolaEntrada.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int indiceLinea = 0;
        List<Instruccion> instruccionesFinales = new List<Instruccion>();

        if (!ParsearBloque(lineasDeCodigo, ref indiceLinea, instruccionesFinales, false, out string mensajeError, out _))
        {
            MostrarMensaje(mensajeError, Color.red);
            return;
        }

        MostrarMensaje("Programa validado con éxito.\nEjecutando algoritmo...", Color.green);

        if (motorEjecucion != null)
        {
            motorEjecucion.IniciarSecuencia(instruccionesFinales);
        }
    }

    private bool ParsearBloque(string[] lineas, ref int indice, List<Instruccion> listaDestino, bool esBloqueInterno, out string error, out bool encontroSino)
    {
        error = null;
        encontroSino = false;

        while (indice < lineas.Length)
        {
            string linea = lineas[indice].Trim();
            indice++;

            if (string.IsNullOrEmpty(linea)) continue;

            string lineaUpper = linea.ToUpper();

            // Cierre de bloque solo: "}"
            if (lineaUpper == "}")
            {
                if (!esBloqueInterno)
                {
                    error = $"Error en línea {indice}: Llave de cierre '}}' inesperada.";
                    return false;
                }
                return true;
            }

            // Cierre de bloque seguido de SINO en la misma línea: "} SINO {"
            if (lineaUpper.StartsWith("}"))
            {
                if (!esBloqueInterno)
                {
                    error = $"Error en línea {indice}: Llave de cierre '}}' inesperada.";
                    return false;
                }

                string resto = lineaUpper.Substring(1).Trim();
                if (resto.StartsWith("SINO"))
                {
                    if (!resto.Contains("{"))
                    {
                        if (indice < lineas.Length && lineas[indice].Trim() == "{")
                        {
                            indice++;
                        }
                        else
                        {
                            error = $"Error en línea {indice}: Falta '{{' tras SINO.";
                            return false;
                        }
                    }
                    encontroSino = true;
                    return true;
                }

                error = $"Error en línea {indice}: Texto no reconocido '{resto}' después de '}}'.";
                return false;
            }

            // Bloque REPETIR
            if (lineaUpper.StartsWith("REPETIR"))
            {
                string[] partes = lineaUpper.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length < 2 || !int.TryParse(partes[1], out int veces))
                {
                    error = $"Error sintáctico en línea {indice}: Formato esperado 'REPETIR [número] {{'.";
                    return false;
                }

                if (!linea.Contains("{"))
                {
                    if (indice < lineas.Length && lineas[indice].Trim() == "{") indice++;
                    else { error = $"Error en línea {indice}: Falta '{{' al abrir REPETIR."; return false; }
                }

                List<Instruccion> cuerpoBucle = new List<Instruccion>();
                if (!ParsearBloque(lineas, ref indice, cuerpoBucle, true, out error, out bool sinoEnBucle)) return false;

                if (sinoEnBucle)
                {
                    error = $"Error en línea {indice}: 'SINO' no puede cerrar un bloque REPETIR.";
                    return false;
                }

                listaDestino.Add(new InstruccionBucle(veces, cuerpoBucle));
                continue;
            }

            // Bloque Condicional SI_OBSTACULO
            if (lineaUpper.StartsWith("SI_OBSTACULO"))
            {
                if (!linea.Contains("{"))
                {
                    if (indice < lineas.Length && lineas[indice].Trim() == "{") indice++;
                    else { error = $"Error en línea {indice}: Falta '{{' tras SI_OBSTACULO."; return false; }
                }

                List<Instruccion> bloqueSi = new List<Instruccion>();
                if (!ParsearBloque(lineas, ref indice, bloqueSi, true, out error, out bool sinoEnMismaLinea)) return false;

                List<Instruccion> bloqueSino = null;

                // Caso 1: "} SINO {" en la misma línea
                if (sinoEnMismaLinea)
                {
                    bloqueSino = new List<Instruccion>();
                    if (!ParsearBloque(lineas, ref indice, bloqueSino, true, out error, out bool sinoDuplicado)) return false;
                    if (sinoDuplicado) { error = $"Error en línea {indice}: Cláusula 'SINO' duplicada."; return false; }
                }
                // Caso 2: "SINO {" en la línea siguiente
                else if (indice < lineas.Length && lineas[indice].Trim().ToUpper().StartsWith("SINO"))
                {
                    string lineaSino = lineas[indice].Trim().ToUpper();
                    indice++;

                    if (!lineaSino.Contains("{"))
                    {
                        if (indice < lineas.Length && lineas[indice].Trim() == "{") indice++;
                        else { error = $"Error en línea {indice}: Falta '{{' tras SINO."; return false; }
                    }

                    bloqueSino = new List<Instruccion>();
                    if (!ParsearBloque(lineas, ref indice, bloqueSino, true, out error, out bool sinoDuplicado)) return false;
                    if (sinoDuplicado) { error = $"Error en línea {indice}: Cláusula 'SINO' duplicada."; return false; }
                }

                listaDestino.Add(new InstruccionCondicional(bloqueSi, bloqueSino));
                continue;
            }

            // Comandos individuales
            if (!InterpretarComandoSimple(linea, listaDestino, out error, indice))
            {
                return false;
            }
        }

        if (esBloqueInterno)
        {
            error = "Error de sintaxis: Falta cerrar una llave '}' al final del bloque.";
            return false;
        }

        return true;
    }

    private bool InterpretarComandoSimple(string linea, List<Instruccion> destino, out string error, int numLinea)
    {
        error = null;
        string[] partes = linea.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return true;

        string comandoPrincipal = partes[0].ToUpper();
        int repeticiones = 1;

        if (partes.Length > 1 && int.TryParse(partes[1], out int cantidad))
        {
            repeticiones = Mathf.Max(1, cantidad);
        }

        switch (comandoPrincipal)
        {
            case "AVANZAR":
                for (int k = 0; k < repeticiones; k++)
                    destino.Add(new InstruccionAccion(TipoComando.Avanzar));
                return true;

            case "GIRAR_IZQ":
            case "GIRAR_IZQUIERDA":
                for (int k = 0; k < repeticiones; k++)
                    destino.Add(new InstruccionAccion(TipoComando.GirarIzquierda));
                return true;

            case "GIRAR_DER":
            case "GIRAR_DERECHA":
                for (int k = 0; k < repeticiones; k++)
                    destino.Add(new InstruccionAccion(TipoComando.GirarDerecha));
                return true;

            default:
                error = $"Error en Línea {numLinea}: Comando '{partes[0]}' no reconocido.";
                return false;
        }
    }

    public void BotonReiniciar()
    {
        if (motorEjecucion != null) motorEjecucion.DetenerSecuencia();
        if (controladorRobot != null) controladorRobot.RestaurarPosicionInicial();
        MostrarMensaje("Robot regresado a la posición inicial.", Color.cyan);
    }

    public void NotificarResultado(bool exito, string mensaje)
    {
        MostrarMensaje(mensaje, exito ? Color.green : Color.red);
    }

    private void MostrarMensaje(string mensaje, Color colorFuente)
    {
        if (textoRetroalimentacion != null)
        {
            textoRetroalimentacion.text = mensaje;
            textoRetroalimentacion.color = colorFuente;
        }
    }
}