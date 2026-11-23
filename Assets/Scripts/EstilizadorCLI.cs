using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EstilizadorCLI : MonoBehaviour
{
    [Header("Elementos de la Interfaz")]
    [SerializeField] private TMP_InputField consolaEntrada;
    [SerializeField] private TextMeshProUGUI textoRetroalimentacion;
    [SerializeField] private TextMeshProUGUI textoInfoNivel;
    [SerializeField] private Button botonEjecutar;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonSiguienteNivel;

    [Header("Fuente Monospace (Opcional)")]
    [SerializeField] private TMP_FontAsset fuenteMonospace;

    [Header("Paleta Terminal CLI")]
    [SerializeField] private Color colorFondoEscena = new Color(0.08f, 0.10f, 0.14f);     // Azul pizarra muy oscuro
    [SerializeField] private Color colorFondoConsola = new Color(0.04f, 0.06f, 0.08f);    // Negro terminal
    [SerializeField] private Color colorVerdeTerminal = new Color(0.20f, 1.00f, 0.40f);   // Verde fósforo
    [SerializeField] private Color colorCianInfo = new Color(0.35f, 0.85f, 1.00f);        // Cian técnico
    [SerializeField] private Color colorAmbar = new Color(1.00f, 0.72f, 0.15f);           // Ámbar alerta

    void Awake()
    {
        AplicarEstiloTerminal();
    }

    public void AplicarEstiloTerminal()
    {
        // 1. Fondo de la cámara principal
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = colorFondoEscena;
        }

        // 2. Consola de entrada de código
        if (consolaEntrada != null)
        {
            Image fondoInput = consolaEntrada.GetComponent<Image>();
            if (fondoInput != null) fondoInput.color = colorFondoConsola;

            if (consolaEntrada.textComponent != null)
            {
                consolaEntrada.textComponent.color = colorVerdeTerminal;
                consolaEntrada.textComponent.fontSize = 20;
                AplicarFuente(consolaEntrada.textComponent);
            }

            if (consolaEntrada.placeholder is TMP_Text placeholderTMP)
            {
                placeholderTMP.text = "> Escribe tu algoritmo aqui...";
                placeholderTMP.color = new Color(colorVerdeTerminal.r, colorVerdeTerminal.g, colorVerdeTerminal.b, 0.35f);
                placeholderTMP.fontSize = 20;
                AplicarFuente(placeholderTMP);
            }

            // Cursor grueso estilo terminal
            consolaEntrada.customCaretColor = true;
            consolaEntrada.caretColor = colorVerdeTerminal;
            consolaEntrada.caretWidth = 3;
            consolaEntrada.selectionColor = new Color(colorVerdeTerminal.r, colorVerdeTerminal.g, colorVerdeTerminal.b, 0.25f);
        }

        // 3. Texto de retroalimentación inferior
        if (textoRetroalimentacion != null)
        {
            textoRetroalimentacion.fontSize = 18;
            AplicarFuente(textoRetroalimentacion);
        }

        // 4. Encabezado del nivel
        if (textoInfoNivel != null)
        {
            textoInfoNivel.color = colorCianInfo;
            textoInfoNivel.fontSize = 20;
            AplicarFuente(textoInfoNivel);
        }

        // 5. Botones estilo comandos de consola
        EstilizarBoton(botonEjecutar, "[ EJECUTAR ]", new Color(0.08f, 0.24f, 0.14f), colorVerdeTerminal);
        EstilizarBoton(botonReiniciar, "[ REINICIAR ]", new Color(0.22f, 0.17f, 0.07f), colorAmbar);
        EstilizarBoton(botonSiguienteNivel, "[ SIGUIENTE >> ]", new Color(0.10f, 0.45f, 0.22f), Color.white);
    }

    private void EstilizarBoton(Button boton, string etiqueta, Color colorFondo, Color colorTexto)
    {
        if (boton == null) return;

        Image img = boton.GetComponent<Image>();
        if (img != null) img.color = colorFondo;

        TMP_Text txt = boton.GetComponentInChildren<TMP_Text>(true);
        if (txt != null)
        {
            txt.text = etiqueta;
            txt.color = colorTexto;
            txt.fontStyle = FontStyles.Bold;
            txt.fontSize = 14; 
            AplicarFuente(txt);
        }
    }

    private void AplicarFuente(TMP_Text componenteTexto)
    {
        if (fuenteMonospace != null && componenteTexto != null)
        {
            componenteTexto.font = fuenteMonospace;
        }
    }
}