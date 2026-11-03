using UnityEngine;

public class GestorCuadricula : MonoBehaviour
{
    [Header("Dimensiones")]
    public int ancho = 8;
    public int alto = 8;
    public float tamanoCelda = 1.0f;

    [Header("Visualización")]
    public Sprite spriteCelda; // Asigna aquí un sprite cuadrado

    // 0 = Espacio Libre, 1 = Obstáculo / Pared, 2 = Meta
    private int[,] datosCuadricula;

    void Awake()
    {
        InicializarCuadricula();
        DibujarCuadricula();
    }

    void InicializarCuadricula()
    {
        datosCuadricula = new int[ancho, alto];

        // Definición de obstáculos de prueba
        datosCuadricula[2, 2] = 1;
        datosCuadricula[2, 3] = 1;
        datosCuadricula[3, 3] = 1;

        // Meta (casilla objetivo)
        datosCuadricula[5, 5] = 2;
    }

    void DibujarCuadricula()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                // Crear un objeto visual para cada celda
                GameObject celdaVisual = new GameObject($"Celda_{x}_{y}");
                celdaVisual.transform.position = CuadriculaAPosicionMundo(x, y);
                celdaVisual.transform.parent = this.transform;

                // Dejar un pequeño margen entre celdas para distinguir la cuadrícula
                celdaVisual.transform.localScale = Vector3.one * (tamanoCelda * 0.92f);

                SpriteRenderer renderer = celdaVisual.AddComponent<SpriteRenderer>();
                renderer.sprite = spriteCelda;
                renderer.sortingOrder = -1; // Mantener el fondo detrás del robot

                // Asignar colores según el tipo de casilla
                int tipo = datosCuadricula[x, y];
                if (tipo == 1)
                {
                    renderer.color = new Color(0.8f, 0.2f, 0.2f); // Rojo: Obstáculo
                }
                else if (tipo == 2)
                {
                    renderer.color = new Color(0.2f, 0.9f, 0.3f); // Verde: Meta
                }
                else
                {
                    renderer.color = new Color(0.18f, 0.22f, 0.3f); // Azul oscuro: Suelo transitable
                }
            }
        }
    }

    public bool EsTransitable(int x, int y)
    {
        if (x < 0 || x >= ancho || y < 0 || y >= alto)
            return false;

        return datosCuadricula[x, y] != 1;
    }

    public Vector3 CuadriculaAPosicionMundo(int x, int y)
    {
        return new Vector3(x * tamanoCelda, y * tamanoCelda, 0);
    }

    public bool EsMeta(int x, int y)
    {
        if (x < 0 || x >= ancho || y < 0 || y >= alto)
            return false;

        return datosCuadricula[x, y] == 2;
    }


}