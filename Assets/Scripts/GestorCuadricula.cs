using System.Collections.Generic;
using UnityEngine;

public class GestorCuadricula : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private GameObject prefabBaldosa;
    [SerializeField] private float tamanoMaximoTablero = 6.3f;

    [Header("Colores de Casillas")]
    [SerializeField] private Color colorNormal = new Color(0.18f, 0.22f, 0.28f);
    [SerializeField] private Color colorObstaculo = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color colorMeta = new Color(0.2f, 0.85f, 0.3f);

    private int ancho = 6;
    private int alto = 6;
    private float espaciadoActual = 1.05f;
    private int[,] datosCuadricula;
    private List<GameObject> baldosasActivas = new List<GameObject>();

    public void ConstruirNivel(int nuevoAncho, int nuevoAlto, Vector2Int posicionMeta, List<Vector2Int> obstaculos)
    {
        LimpiarCuadricula();

        ancho = nuevoAncho;
        alto = nuevoAlto;
        datosCuadricula = new int[ancho, alto];

        // Ajusta el tamaño de cada celda para que el tablero nunca exceda tamanoMaximoTablero
        int dimensionMayor = Mathf.Max(ancho, alto);
        espaciadoActual = tamanoMaximoTablero / dimensionMayor;
        float escalaBaldosa = espaciadoActual * 0.95f;

        if (obstaculos != null)
        {
            foreach (Vector2Int obs in obstaculos)
            {
                if (EstaDentroDeLimites(obs.x, obs.y))
                {
                    datosCuadricula[obs.x, obs.y] = 1;
                }
            }
        }

        if (EstaDentroDeLimites(posicionMeta.x, posicionMeta.y))
        {
            datosCuadricula[posicionMeta.x, posicionMeta.y] = 2;
        }

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Vector3 pos = CuadriculaAPosicionMundo(x, y);
                GameObject baldosa = Instantiate(prefabBaldosa, pos, Quaternion.identity, transform);
                baldosa.name = $"Baldosa_{x}_{y}";
                baldosa.transform.localScale = new Vector3(escalaBaldosa, escalaBaldosa, 1f);

                SpriteRenderer sr = baldosa.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (datosCuadricula[x, y] == 1) sr.color = colorObstaculo;
                    else if (datosCuadricula[x, y] == 2) sr.color = colorMeta;
                    else sr.color = colorNormal;
                }

                baldosasActivas.Add(baldosa);
            }
        }
    }

    public float ObtenerFactorEscala()
    {
        return espaciadoActual / 1.05f;
    }

    private void LimpiarCuadricula()
    {
        foreach (GameObject baldosa in baldosasActivas)
        {
            if (baldosa != null)
            {
                Destroy(baldosa);
            }
        }
        baldosasActivas.Clear();
    }

    public Vector3 CuadriculaAPosicionMundo(int x, int y)
    {
        float offsetX = (ancho - 1) * espaciadoActual * 0.5f;
        float offsetY = (alto - 1) * espaciadoActual * 0.5f;
        return transform.position + new Vector3(x * espaciadoActual - offsetX, y * espaciadoActual - offsetY, 0f);
    }

    public bool EstaDentroDeLimites(int x, int y)
    {
        return x >= 0 && x < ancho && y >= 0 && y < alto;
    }

    public bool EsTransitable(int x, int y)
    {
        if (!EstaDentroDeLimites(x, y)) return false;
        return datosCuadricula[x, y] != 1;
    }

    public bool EsMeta(int x, int y)
    {
        if (!EstaDentroDeLimites(x, y)) return false;
        return datosCuadricula[x, y] == 2;
    }
}