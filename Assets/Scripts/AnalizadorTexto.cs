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
        List<TipoComando> listaFinalComandos = new List<TipoComando>();

        for (int i = 0; i < lineasDeCodigo.Length; i++)
        {
            string linea = lineasDeCodigo[i].Trim().ToUpper();
            if (string.IsNullOrEmpty(linea)) continue;

            if (linea.StartsWith("REPETIR"))
            {
                string[] partesBucle = linea.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partesBucle.Length < 2 || !int.TryParse(partesBucle[1], out int vecesRepetir))
                {
                    MostrarMensaje($"Error en Línea {i + 1}: Especifica un número válido (ej. REPETIR 3 {{).", Color.red);
                    return;
                }

                List<string> lineasInternas = new List<string>();
                int lineaCierre = -1;

                for (int j = i + 1; j < lineasDeCodigo.Length; j++)
                {
                    string sublinea = lineasDeCodigo[j].Trim().ToUpper();
                    if (sublinea == "}")
                    {
                        lineaCierre = j;
                        break;
                    }
                    lineasInternas.Add(sublinea);
                }

                if (lineaCierre == -1)
                {
                    MostrarMensaje($"Error: Falta cerrar la llave '}}' del bloque REPETIR iniciado en la línea {i + 1}.", Color.red);
                    return;
                }

                List<TipoComando> comandosInternos = new List<TipoComando>();
                foreach (string instruccionInterna in lineasInternas)
                {
                    if (string.IsNullOrEmpty(instruccionInterna)) continue;
                    if (!InterpretarLinea(instruccionInterna, comandosInternos))
                    {
                        MostrarMensaje($"Error de comando dentro del bloque REPETIR: '{instruccionInterna}'.", Color.red);
                        return;
                    }
                }

                for (int r = 0; r < vecesRepetir; r++)
                {
                    listaFinalComandos.AddRange(comandosInternos);
                }

                i = lineaCierre;
                continue;
            }

            if (!InterpretarLinea(linea, listaFinalComandos))
            {
                MostrarMensaje($"Error en Línea {i + 1}: Comando '{lineasDeCodigo[i].Trim()}' no reconocido.", Color.red);
                return;
            }
        }

        MostrarMensaje($"Ejecutando {listaFinalComandos.Count} pasos...", Color.green);

        if (motorEjecucion != null)
        {
            motorEjecucion.IniciarSecuencia(listaFinalComandos);
        }
    }

    private bool InterpretarLinea(string instruccion, List<TipoComando> destino)
    {
        string[] partes = instruccion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string comandoPrincipal = partes[0];

        int repeticiones = 1;
        if (partes.Length > 1 && int.TryParse(partes[1], out int cantidad))
        {
            repeticiones = Mathf.Max(1, cantidad);
        }

        switch (comandoPrincipal)
        {
            case "AVANZAR":
                for (int k = 0; k < repeticiones; k++) destino.Add(TipoComando.Avanzar);
                return true;

            case "GIRAR_IZQ":
            case "GIRAR_IZQUIERDA":
                for (int k = 0; k < repeticiones; k++) destino.Add(TipoComando.GirarIzquierda);
                return true;

            case "GIRAR_DER":
            case "GIRAR_DERECHA":
                for (int k = 0; k < repeticiones; k++) destino.Add(TipoComando.GirarDerecha);
                return true;

            default:
                return false;
        }
    }

    public void BotonReiniciar()
    {
        if (controladorRobot != null)
        {
            controladorRobot.RestaurarPosicionInicial();
        }
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
        else
        {
            Debug.Log(mensaje);
        }
    }
}