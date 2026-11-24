using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DefinicionNivel
{
    public string tituloNivel;
    [TextArea(2, 4)] public string objetivo;
    [TextArea(4, 10)] public string guiaConcepto;
    public int maxLineas = 0; // 0 = sin límite
    public List<string> comandosRequeridos = new List<string>();
    public int ancho = 8;
    public int alto = 8;
    public Vector2Int inicioRobot;
    public Direccion direccionInicial = Direccion.Este;
    public Vector2Int posicionMeta;
    public List<Vector2Int> obstaculos = new List<Vector2Int>();
    public List<Vector2Int> nodosEnergia = new List<Vector2Int>();
}

public class GestorNiveles : MonoBehaviour
{
    [Header("Referencias del Juego")]
    [SerializeField] private GestorCuadricula gestorCuadricula;
    [SerializeField] private ControladorRobot robot;
    [SerializeField] private MotorEjecucion motorEjecucion;

    [Header("Interfaz de Nivel")]
    [SerializeField] private TextMeshProUGUI textoInfoNivel;
    [SerializeField] private TextMeshProUGUI textoRetroalimentacion;
    [SerializeField] private TextMeshProUGUI textoManualSintaxis;
    [SerializeField] private TMP_InputField campoConsolaEntrada;
    [SerializeField] private GameObject botonSiguienteNivel;

    [Header("Lista de Niveles")]
    [SerializeField] private List<DefinicionNivel> niveles = new List<DefinicionNivel>();

    private int indiceNivelActual = 0;
    private bool nivelSuperado = false;

    void Awake()
    {
        if (niveles.Count == 0)
        {
            CrearNivelesPorDefecto();
        }
    }

    void Start()
    {
        CargarNivel(0);
    }

    public int ObtenerLimiteLineas()
    {
        if (indiceNivelActual >= 0 && indiceNivelActual < niveles.Count)
        {
            return niveles[indiceNivelActual].maxLineas;
        }
        return 0;
    }

    public List<string> ObtenerComandosRequeridos()
    {
        if (indiceNivelActual >= 0 && indiceNivelActual < niveles.Count && niveles[indiceNivelActual].comandosRequeridos != null)
        {
            return niveles[indiceNivelActual].comandosRequeridos;
        }
        return new List<string>();
    }

    public void CargarNivel(int indice)
    {
        if (indice < 0 || indice >= niveles.Count) return;

        indiceNivelActual = indice;
        nivelSuperado = false;
        DefinicionNivel nivel = niveles[indice];

        if (motorEjecucion != null) motorEjecucion.DetenerSecuencia();
        OcultarBotonSiguiente();

        if (gestorCuadricula != null)
        {
            gestorCuadricula.ConstruirNivel(nivel.ancho, nivel.alto, nivel.posicionMeta, nivel.obstaculos, nivel.nodosEnergia);
        }

        if (robot != null)
        {
            robot.EstablecerEstadoInicial(nivel.inicioRobot.x, nivel.inicioRobot.y, nivel.direccionInicial);
        }

        if (textoInfoNivel != null)
        {
            string etiquetaLimite = nivel.maxLineas > 0 ? $" [Máx: {nivel.maxLineas} líneas]" : "";
            textoInfoNivel.text = $"NIVEL {indice + 1}/{niveles.Count}: {nivel.tituloNivel}{etiquetaLimite}\n{nivel.objetivo}";
        }

        if (textoManualSintaxis != null)
        {
            textoManualSintaxis.text = nivel.guiaConcepto;
        }

        if (textoRetroalimentacion != null)
        {
            if (textoManualSintaxis == null && !string.IsNullOrEmpty(nivel.guiaConcepto))
            {
                textoRetroalimentacion.text = nivel.guiaConcepto;
                textoRetroalimentacion.color = new Color(0.75f, 0.90f, 1.00f);
            }
            else
            {
                textoRetroalimentacion.text = "> [SYS] Sistema listo. Escribe tu algoritmo y presiona EJECUTAR.";
                textoRetroalimentacion.color = Color.white;
            }
        }
    }

    public void NivelCompletado()
    {
        nivelSuperado = true;

        if (indiceNivelActual + 1 < niveles.Count)
        {
            if (botonSiguienteNivel != null)
            {
                botonSiguienteNivel.SetActive(true);
            }
        }
        else if (textoRetroalimentacion != null)
        {
            textoRetroalimentacion.text = "> [OK] ¡FELICIDADES!\nHas completado los 5 niveles de entrenamiento de LogicBot.";
            textoRetroalimentacion.color = new Color(0.20f, 1.00f, 0.40f);
        }
    }

    public void OcultarBotonSiguiente()
    {
        nivelSuperado = false;
        if (botonSiguienteNivel != null)
        {
            botonSiguienteNivel.SetActive(false);
        }
    }

    public void CargarSiguienteNivel()
    {
        if (!nivelSuperado) return;

        if (campoConsolaEntrada != null)
        {
            campoConsolaEntrada.text = "";
        }

        if (indiceNivelActual + 1 < niveles.Count)
        {
            CargarNivel(indiceNivelActual + 1);
        }
    }

    private void CrearNivelesPorDefecto()
    {
        // NIVEL 1: Secuencias y movimiento básico
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "PRIMEROS PASOS",
            objetivo = "Lleva al robot hasta la casilla verde.",
            guiaConcepto =
                "[ LECCIÓN 1: SECUENCIAS ]\n" +
                "Un algoritmo ejecuta instrucciones en orden, una tras otra:\n\n" +
                "• AVANZAR\n" +
                "  Mueve 1 casilla hacia donde apunta el visor azul.\n" +
                "  (También puedes indicar pasos: AVANZAR 3)\n\n" +
                "• GIRAR_IZQ / GIRAR_DER\n" +
                "  Rota 90° a la izquierda o derecha sin avanzar.",
            maxLineas = 8,
            ancho = 6,
            alto = 6,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(4, 3),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(2, 0),
                new Vector2Int(2, 1)
            }
        });

        // NIVEL 2: Bucles REPETIR (Pasillo en escalera cerrado)
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "ESCALERA DE BUCLES",
            objetivo = "Sube el pasillo en escalera usando REPETIR (obligatorio).",
            guiaConcepto =
                "[ LECCIÓN 2: BUCLES (REPETIR) ]\n" +
                "Este nivel exige usar un bucle para repetir el patrón del escalón:\n\n" +
                "REPETIR 5 {\n" +
                "    GIRAR_IZQ\n" +
                "    AVANZAR\n" +
                "    GIRAR_DER\n" +
                "    AVANZAR\n" +
                "}",
            maxLineas = 6,
            comandosRequeridos = new List<string> { "REPETIR" },
            ancho = 7,
            alto = 7,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(5, 5),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(2, 1),
                new Vector2Int(3, 2),
                new Vector2Int(4, 3),
                new Vector2Int(5, 4),
                new Vector2Int(0, 2),
                new Vector2Int(1, 3),
                new Vector2Int(2, 4),
                new Vector2Int(3, 5),
                new Vector2Int(4, 6)
            }
        });

        // NIVEL 3: Rutas en zigzag y nodos múltiples
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "LABERINTO DE ENERGÍA",
            objetivo = "Recoge los dos nodos amarillos en zigzag antes de entrar a la meta.",
            guiaConcepto =
                "[ LECCIÓN 3: OPTIMIZACIÓN DE RUTA ]\n" +
                "La salida verde permanece bloqueada hasta recoger todos los nodos amarillos.\n\n" +
                "• Usa AVANZAR N (ej. AVANZAR 6) para recorrer pasillos largos en una sola línea y respetar el límite de 10 líneas.",
            maxLineas = 10,
            ancho = 8,
            alto = 8,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(7, 7),
            obstaculos = new List<Vector2Int>
            {
                // Muro izquierdo (obliga a subir primero)
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(2, 3),
                new Vector2Int(2, 4),
                // Muro derecho (obliga a bajar por el centro antes de subir a la meta)
                new Vector2Int(5, 3),
                new Vector2Int(5, 4),
                new Vector2Int(5, 5),
                new Vector2Int(5, 6),
                new Vector2Int(5, 7)
            },
            nodosEnergia = new List<Vector2Int>
            {
                new Vector2Int(1, 6),
                new Vector2Int(6, 1)
            }
        });

        // NIVEL 4: Condicionales SI_OBSTACULO / SINO (Corredor cerrado)
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "SENSOR INTELIGENTE",
            objetivo = "Usa SI_OBSTACULO (obligatorio) dentro de REPETIR para cruzar el corredor.",
            guiaConcepto =
                "[ LECCIÓN 4: CONDICIONALES ]\n" +
                "Permiten decidir qué hacer según lo que detecte el sensor frontal:\n\n" +
                "REPETIR 10 {\n" +
                "    SI_OBSTACULO {\n" +
                "        GIRAR_IZQ\n" +
                "        AVANZAR\n" +
                "        GIRAR_DER\n" +
                "    } SINO {\n" +
                "        AVANZAR\n" +
                "    }\n" +
                "}",
            maxLineas = 10,
            comandosRequeridos = new List<string> { "SI_OBSTACULO" },
            ancho = 8,
            alto = 8,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(6, 4),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(4, 1),
                new Vector2Int(4, 2),
                new Vector2Int(5, 2),
                new Vector2Int(5, 3),
                new Vector2Int(6, 3),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                new Vector2Int(2, 3),
                new Vector2Int(2, 4),
                new Vector2Int(3, 4),
                new Vector2Int(3, 5)
            }
        });

        // NIVEL 5: Reto final en espiral 10x10
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "NÚCLEO EN ESPIRAL",
            objetivo = "Navega la espiral hasta el centro recolectando los 5 nodos. Requiere REPETIR y SI_OBSTACULO.",
            guiaConcepto =
                "[ DESAFÍO FINAL: ESPIRAL ]\n" +
                "En este laberinto de 10x10 debes combinar REPETIR y SI_OBSTACULO en máximo 8 líneas:\n\n" +
                "1. Si hay obstáculo al frente, gira a la izquierda (GIRAR_IZQ); si no, avanza.\n" +
                "2. Calibra las repeticiones (entre 65 y 70 ciclos) hasta detenerte justo sobre la meta central.",
            maxLineas = 8,
            comandosRequeridos = new List<string> { "REPETIR", "SI_OBSTACULO" },
            ancho = 10,
            alto = 10,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(5, 5),
            obstaculos = new List<Vector2Int>
            {
                // Anillo exterior de la espiral
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1), new Vector2Int(4, 1),
                new Vector2Int(5, 1), new Vector2Int(6, 1), new Vector2Int(7, 1), new Vector2Int(8, 1),
                new Vector2Int(8, 2), new Vector2Int(8, 3), new Vector2Int(8, 4), new Vector2Int(8, 5),
                new Vector2Int(8, 6), new Vector2Int(8, 7), new Vector2Int(8, 8),
                new Vector2Int(7, 8), new Vector2Int(6, 8), new Vector2Int(5, 8), new Vector2Int(4, 8),
                new Vector2Int(3, 8), new Vector2Int(2, 8), new Vector2Int(1, 8),
                new Vector2Int(1, 7), new Vector2Int(1, 6), new Vector2Int(1, 5), new Vector2Int(1, 4), new Vector2Int(1, 3),
                // Anillo interior de la espiral
                new Vector2Int(2, 3), new Vector2Int(3, 3), new Vector2Int(4, 3), new Vector2Int(5, 3), new Vector2Int(6, 3),
                new Vector2Int(6, 4), new Vector2Int(6, 5), new Vector2Int(6, 6),
                new Vector2Int(5, 6), new Vector2Int(4, 6), new Vector2Int(3, 6),
                new Vector2Int(3, 5), new Vector2Int(4, 5)
            },
            nodosEnergia = new List<Vector2Int>
            {
                new Vector2Int(9, 0),
                new Vector2Int(9, 9),
                new Vector2Int(0, 9),
                new Vector2Int(7, 2),
                new Vector2Int(2, 7)
            }
        });
    }
}