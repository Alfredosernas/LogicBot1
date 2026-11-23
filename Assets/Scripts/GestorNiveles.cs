using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DefinicionNivel
{
    public string tituloNivel;
    [TextArea(2, 4)] public string objetivo;
    public int ancho = 8;
    public int alto = 8;
    public Vector2Int inicioRobot;
    public Direccion direccionInicial = Direccion.Este;
    public Vector2Int posicionMeta;
    public List<Vector2Int> obstaculos = new List<Vector2Int>();
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
            gestorCuadricula.ConstruirNivel(nivel.ancho, nivel.alto, nivel.posicionMeta, nivel.obstaculos);
        }

        if (robot != null)
        {
            robot.EstablecerEstadoInicial(nivel.inicioRobot.x, nivel.inicioRobot.y, nivel.direccionInicial);
        }

        if (textoInfoNivel != null)
        {
            textoInfoNivel.text = $"NIVEL {indice + 1}/{niveles.Count}: {nivel.tituloNivel}\n{nivel.objetivo}";
        }

        if (textoRetroalimentacion != null)
        {
            textoRetroalimentacion.text = "Sistema listo. Escribe tu algoritmo y presiona EJECUTAR.";
            textoRetroalimentacion.color = Color.white;
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
            textoRetroalimentacion.text = "¡FELICIDADES!\n\nHas completado todos los niveles de entrenamiento de LogicBot.";
            textoRetroalimentacion.color = Color.green;
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
        // NIVEL 1: Introducción a comandos básicos y bucles
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "PRIMEROS PASOS",
            objetivo = "Lleva al robot hasta la casilla verde usando AVANZAR, GIRAR_IZQ y REPETIR.",
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

        // NIVEL 2: Rodeo de barreras
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "BARRERA CENTRAL",
            objetivo = "Rodea el muro rojo para alcanzar la salida.",
            ancho = 8,
            alto = 8,
            inicioRobot = new Vector2Int(0, 0),
            direccionInicial = Direccion.Este,
            posicionMeta = new Vector2Int(5, 5),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 4),
                new Vector2Int(5, 4)
            }
        });

        // NIVEL 3: Uso del sensor condicional
        niveles.Add(new DefinicionNivel
        {
            tituloNivel = "NAVEGACIÓN CON SENSOR",
            objetivo = "Usa REPETIR junto con SI_OBSTACULO { ... } SINO { ... } para cruzar el pasillo.",
            ancho = 8,
            alto = 8,
            inicioRobot = new Vector2Int(1, 0),
            direccionInicial = Direccion.Norte,
            posicionMeta = new Vector2Int(6, 6),
            obstaculos = new List<Vector2Int>
            {
                new Vector2Int(1, 2),
                new Vector2Int(2, 4),
                new Vector2Int(3, 6),
                new Vector2Int(0, 3),
                new Vector2Int(4, 2),
                new Vector2Int(5, 5)
            }
        });
    }
}