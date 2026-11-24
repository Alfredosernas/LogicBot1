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

    private readonly Color colorExito = new Color(0.20f, 1.00f, 0.40f);
    private readonly Color colorError = new Color(1.00f, 0.30f, 0.30f);
    private readonly Color colorAviso = new Color(1.00f, 0.72f, 0.15f);
    private readonly Color colorInfo = new Color(0.35f, 0.85f, 1.00f);

    public void ProcesarInstrucciones()
    {
        if (gestorNiveles != null) gestorNiveles.OcultarBotonSiguiente();

        if (campoConsolaEntrada == null || string.IsNullOrWhiteSpace(campoConsolaEntrada.text))
        {
            MostrarMensaje("> [AVISO] Consola vacía. Escribe un comando antes de ejecutar.", colorAviso);
            return;
        }

        string[] lineasCrudas = campoConsolaEntrada.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        List<string> lineasLimpias = new List<string>();
        foreach (string l in lineasCrudas)
        {
            if (!string.IsNullOrWhiteSpace(l))
            {
                lineasLimpias.Add(l.Trim());
            }
        }

        if (lineasLimpias.Count == 0)
        {
            MostrarMensaje("> [AVISO] Consola vacía. Escribe un comando antes de ejecutar.", colorAviso);
            return;
        }

        // Comprobar presupuesto de líneas del nivel
        int limiteLineas = gestorNiveles != null ? gestorNiveles.ObtenerLimiteLineas() : 0;
        if (limiteLineas > 0 && lineasLimpias.Count > limiteLineas)
        {
            MostrarMensaje($"> [ERR] LÍMITE EXCEDIDO ({lineasLimpias.Count}/{limiteLineas} líneas).\nOptimiza tu código usando REPETIR.", colorError);
            return;
        }

        if (controladorRobot != null)
        {
            controladorRobot.RestaurarPosicionInicial();
        }

        string[] lineasDeCodigo = lineasLimpias.ToArray();
        int indiceLinea = 0;
        List<Instruccion> instruccionesFinales = new List<Instruccion>();

        if (!ParsearBloque(lineasDeCodigo, ref indiceLinea, instruccionesFinales, false, out string mensajeError, out _))
        {
            MostrarMensaje($"> [ERR] {mensajeError}", colorError);
            return;
        }

        MostrarMensaje($"> [SYS] Compilación OK ({lineasLimpias.Count} líneas).\n> [RUN] Ejecutando secuencia...", colorExito);

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

            if (lineaUpper == "}")
            {
                if (!esBloqueInterno)
                {
                    error = $"Línea {indice}: Llave de cierre '}}' inesperada.";
                    return false;
                }
                return true;
            }

            if (lineaUpper.StartsWith("}"))
            {
                if (!esBloqueInterno)
                {
                    error = $"Línea {indice}: Llave de cierre '}}' inesperada.";
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
                            error = $"Línea {indice}: Falta '{{' tras SINO.";
                            return false;
                        }
                    }
                    encontroSino = true;
                    return true;
                }

                error = $"Línea {indice}: Texto no reconocido '{resto}' después de '}}'.";
                return false;
            }

            if (lineaUpper.StartsWith("REPETIR"))
            {
                string[] partes = lineaUpper.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length < 2 || !int.TryParse(partes[1], out int veces))
                {
                    error = $"Línea {indice}: Formato esperado 'REPETIR [número] {{'.";
                    return false;
                }

                if (!linea.Contains("{"))
                {
                    if (indice < lineas.Length && lineas[indice].Trim() == "{") indice++;
                    else { error = $"Línea {indice}: Falta '{{' al abrir REPETIR."; return false; }
                }

                List<Instruccion> cuerpoBucle = new List<Instruccion>();
                if (!ParsearBloque(lineas, ref indice, cuerpoBucle, true, out error, out bool sinoEnBucle)) return false;

                if (sinoEnBucle)
                {
                    error = $"Línea {indice}: 'SINO' no puede cerrar un bloque REPETIR.";
                    return false;
                }

                listaDestino.Add(new InstruccionBucle(veces, cuerpoBucle));
                continue;
            }

            if (lineaUpper.StartsWith("SI_OBSTACULO"))
            {
                if (!linea.Contains("{"))
                {
                    if (indice < lineas.Length && lineas[indice].Trim() == "{") indice++;
                    else { error = $"Línea {indice}: Falta '{{' tras SI_OBSTACULO."; return false; }
                }

                List<Instruccion> bloqueSi = new List<Instruccion>();
                if (!ParsearBloque(lineas, ref indice, bloqueSi, true, out error, out bool sinoEnMismaLinea)) return false;

                List<Instruccion> bloqueSino = null;

                if (sinoEnMismaLinea)
                {
                    bloqueSino = new List<Instruccion>();
                    if (!ParsearBloque(lineas, ref indice, bloqueSino, true, out error, out bool sinoDuplicado)) return false;
                    if (sinoDuplicado) { error = $"Línea {indice}: Cláusula 'SINO' duplicada."; return false; }
                }
                else if (indice < lineas.Length && lineas[indice].Trim().ToUpper().StartsWith("SINO"))
                {
                    string lineaSino = lineas[indice].Trim().ToUpper();
                    indice++;

                    if (!lineaSino.Contains("{"))
                    {
                        if (indice < lineas.Length && lineas[indice].Trim() == "{") indice++;
                        else { error = $"Línea {indice}: Falta '{{' tras SINO."; return false; }
                    }

                    bloqueSino = new List<Instruccion>();
                    if (!ParsearBloque(lineas, ref indice, bloqueSino, true, out error, out bool sinoDuplicado)) return false;
                    if (sinoDuplicado) { error = $"Línea {indice}: Cláusula 'SINO' duplicada."; return false; }
                }

                listaDestino.Add(new InstruccionCondicional(bloqueSi, bloqueSino));
                continue;
            }

            if (!InterpretarComandoSimple(linea, listaDestino, out error, indice))
            {
                return false;
            }
        }

        if (esBloqueInterno)
        {
            error = "Sintaxis incompleta: Falta cerrar una llave '}' al final del bloque.";
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
                error = $"Línea {numLinea}: Comando '{partes[0]}' no reconocido.";
                return false;
        }
    }

    public void BotonReiniciar()
    {
        if (motorEjecucion != null) motorEjecucion.DetenerSecuencia();
        if (controladorRobot != null) controladorRobot.RestaurarPosicionInicial();
        if (gestorNiveles != null) gestorNiveles.OcultarBotonSiguiente();
        MostrarMensaje("> [SYS] Posición inicial y nodos restaurados.", colorInfo);
    }

    public void NotificarResultado(bool exito, string mensaje)
    {
        string prefijo = exito ? "> [OK] " : "> [ERR] ";
        MostrarMensaje(prefijo + mensaje, exito ? colorExito : colorError);
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