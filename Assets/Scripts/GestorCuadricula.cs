using System.Collections.Generic;
using UnityEngine;

public class GestorCuadricula : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private GameObject prefabBaldosa;
    [SerializeField] private float tamanoMaximoTablero = 6.3f;

    [Header("Colores de Casillas")]
    [SerializeField] private Color colorNormal = new Color(0.18f, 0.22f, 0.28f);
    [SerializeField] private Color colorObstaculo = new Color(0.80f, 0.20f, 0.20f);
    [SerializeField] private Color colorMeta = new Color(0.20f, 0.85f, 0.30f);
    [SerializeField] private Color colorMetaBloqueada = new Color(0.11f, 0.32f, 0.17f);
    [SerializeField] private Color colorNodoEnergia = new Color(1.00f, 0.78f, 0.15f);

    private int ancho = 6;
    private int alto = 6;
    private float espaciadoActual = 1.05f;
    private int[,] datosCuadricula;
    private GameObject[,] matrizBaldosas;
    private List<GameObject> baldosasActivas = new List<GameObject>();

    private Vector2Int posicionMetaActual;
    private List<Vector2Int> nodosNivelActual = new List<Vector2Int>();
    private int nodosRestantes = 0;

    public void ConstruirNivel(int nuevoAncho, int nuevoAlto, Vector2Int posicionMeta, List<Vector2Int> obstaculos, List<Vector2Int> nodosEnergia = null)
    {
        LimpiarCuadricula();

        ancho = nuevoAncho;
        alto = nuevoAlto;
        posicionMetaActual = posicionMeta;
        datosCuadricula = new int[ancho, alto];
        matrizBaldosas = new GameObject[ancho, alto];

        nodosNivelActual = nodosEnergia != null ? new List<Vector2Int>(nodosEnergia) : new List<Vector2Int>();

        int dimensionMayor = Mathf.Max(ancho, alto);
        espaciadoActual = tamanoMaximoTablero / dimensionMayor;
        float escalaBaldosa = espaciadoActual * 0.95f;

        // 1. Obstáculos (1)
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

        // 2. Meta (2)
        if (EstaDentroDeLimites(posicionMeta.x, posicionMeta.y))
        {
            datosCuadricula[posicionMeta.x, posicionMeta.y] = 2;
        }

        // 3. Instanciar baldosas base
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Vector3 pos = CuadriculaAPosicionMundo(x, y);
                GameObject baldosa = Instantiate(prefabBaldosa, pos, Quaternion.identity, transform);
                baldosa.name = $"Baldosa_{x}_{y}";
                baldosa.transform.localScale = new Vector3(escalaBaldosa, escalaBaldosa, 1f);

                matrizBaldosas[x, y] = baldosa;
                baldosasActivas.Add(baldosa);
            }
        }

        // 4. Colocar nodos de energía y pintar colores iniciales
        ReiniciarElementosNivel();
    }

    public void ReiniciarElementosNivel()
    {
        if (datosCuadricula == null || matrizBaldosas == null) return;

        // Limpiar nodos previos
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (datosCuadricula[x, y] == 3)
                {
                    datosCuadricula[x, y] = 0;
                }
            }
        }

        // Restaurar todos los nodos de energía del nivel (3)
        nodosRestantes = 0;
        foreach (Vector2Int nodo in nodosNivelActual)
        {
            if (EstaDentroDeLimites(nodo.x, nodo.y) && datosCuadricula[nodo.x, nodo.y] == 0)
            {
                datosCuadricula[nodo.x, nodo.y] = 3;
                nodosRestantes++;
            }
        }

        ActualizarVisualesTablero();
    }

    public bool ComprobarYRecogerNodo(int x, int y)
    {
        if (datosCuadricula == null || !EstaDentroDeLimites(x, y)) return false;

        if (datosCuadricula[x, y] == 3)
        {
            datosCuadricula[x, y] = 0;
            nodosRestantes = Mathf.Max(0, nodosRestantes - 1);
            ActualizarVisualesTablero();
            return true;
        }

        return false;
    }

    public bool MetaDesbloqueada()
    {
        return nodosRestantes <= 0;
    }

    private void ActualizarVisualesTablero()
    {
        if (datosCuadricula == null || matrizBaldosas == null) return;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (matrizBaldosas[x, y] == null) continue;
                SpriteRenderer sr = matrizBaldosas[x, y].GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                int tipo = datosCuadricula[x, y];
                if (tipo == 1) sr.color = colorObstaculo;
                else if (tipo == 2) sr.color = MetaDesbloqueada() ? colorMeta : colorMetaBloqueada;
                else if (tipo == 3) sr.color = colorNodoEnergia;
                else sr.color = colorNormal;
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
        if (datosCuadricula == null || !EstaDentroDeLimites(x, y)) return false;
        return datosCuadricula[x, y] != 1;
    }

    public bool EsMeta(int x, int y)
    {
        if (datosCuadricula == null || !EstaDentroDeLimites(x, y)) return false;
        return datosCuadricula[x, y] == 2;
    }
}