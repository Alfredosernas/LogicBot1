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
            // Si aún no conectas el panel derecho, muestra la guía aquí al iniciar el nivel
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

        // NIVEL 2: Bucles REPETIR
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "ESCALERA DE BUCLES",
            objetivo = "Sube en escalera hasta la meta sin pasarte del límite de líneas.",
            guiaConcepto =
                "[ LECCIÓN 2: BUCLES (REPETIR) ]\n" +
                "Cuando notas un patrón que se repite, usa un bucle para ahorrar líneas:\n\n" +
                "REPETIR 4 {\n" +
                "    AVANZAR\n" +
                "    GIRAR_IZQ\n" +
                "}\n\n" +
                "Todo lo que esté dentro de { ... } se ejecutará el número de veces indicado.",
            maxLineas = 6,
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
                new Vector2Int(5, 4)
            }
        });

        // NIVEL 3: Estados y objetivos intermedios
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "NODO DE ENERGÍA",
            objetivo = "Activa el nodo amarillo para desbloquear la salida verde.",
            guiaConcepto =
                "[ LECCIÓN 3: ESTADOS Y RUTAS ]\n" +
                "La salida permanecerá bloqueada (verde oscuro) hasta que recojas todos los nodos amarillos.\n\n" +
                "• Consejo de optimización:\n" +
                "  Combina movimientos múltiples como AVANZAR 3 con giros para mantener tu código corto.",
            maxLineas = 8,
            ancho = 7,
            alto = 7,
            inicioRobot = new Vector2Int(0, 3),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(6, 3),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(3, 2),
                new Vector2Int(3, 3),
                new Vector2Int(3, 4)
            },
            nodosEnergia = new List<Vector2Int>
            {
                new Vector2Int(3, 6)
            }
        });

        // NIVEL 4: Condicionales SI_OBSTACULO / SINO
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "SENSOR INTELIGENTE",
            objetivo = "Esquiva la diagonal usando el sensor de proximidad.",
            guiaConcepto =
                "[ LECCIÓN 4: CONDICIONALES ]\n" +
                "Permiten que el robot tome decisiones según lo que detecte frente a él:\n\n" +
                "SI_OBSTACULO {\n" +
                "    GIRAR_IZQ\n" +
                "} SINO {\n" +
                "    AVANZAR\n" +
                "}\n\n" +
                "Combínalo dentro de un REPETIR para avanzar automáticamente.",
            maxLineas = 10,
            ancho = 8,
            alto = 8,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(7, 4),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(2, 0),
                new Vector2Int(3, 1),
                new Vector2Int(4, 2),
                new Vector2Int(5, 3)
            }
        });

        // NIVEL 5: Integración total
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "PROTOCOLO FINAL",
            objetivo = "Activa ambos nodos amarillos para abrir la compuerta de salida.",
            guiaConcepto =
                "[ DESAFÍO INTEGRADOR ]\n" +
                "Aplica todo lo aprendido:\n\n" +
                "1. Traza una ruta que pase por los dos nodos amarillos.\n" +
                "2. Usa REPETIR en los tramos largos para respetar el máximo de 9 líneas.\n" +
                "3. Dirígete a la meta una vez iluminada.",
            maxLineas = 9,
            ancho = 8,
            alto = 8,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Norte,
            posicionMeta = new Vector2Int(4, 2),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(2, 4),
                new Vector2Int(3, 4),
                new Vector2Int(4, 4),
                new Vector2Int(5, 4),
                new Vector2Int(6, 4)
            },
            nodosEnergia = new List<Vector2Int>
            {
                new Vector2Int(0, 6),
                new Vector2Int(7, 6)
            }
        });
    }
}