using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Genera y aplica un fondo minimalista con un patrón de puntos sutiles sobre un componente Image de Unity.
/// Permite configurar colores, espaciado, tamaño y opacidad de los puntos, así como variaciones suaves en el color.
/// </summary>
[RequireComponent(typeof(Image))]
public class DotsPattern : MonoBehaviour
{
    /// <summary>
    /// Color base del fondo.
    /// </summary>
    [Header("Configuración de Fondo")]
    public Color colorFondo = new Color(0.08f, 0.12f, 0.25f);

    /// <summary>
    /// Color de acento utilizado para el gradiente y los puntos.
    /// </summary>
    public Color colorAcento = new Color(0.12f, 0.18f, 0.35f);

    /// <summary>
    /// Espaciado entre los puntos del patrón (en píxeles).
    /// </summary>
    [Header("Configuración Sutil")]
    [Range(300, 1000)]
    public int espaciadoPatron = 800; // Espaciado muy amplio para mínima distracción

    /// <summary>
    /// Radio de los puntos del patrón (en píxeles).
    /// </summary>
    [Range(1, 5)]
    public int tamañoPunto = 1; // Puntos muy pequeños

    /// <summary>
    /// Opacidad de los puntos del patrón.
    /// </summary>
    [Range(0.01f, 0.1f)]
    public float opacidadPunto = 0.03f; // Muy baja opacidad

    /// <summary>
    /// Variación máxima de color usando ruido Perlin para el fondo.
    /// </summary>
    [Range(0, 0.3f)]
    public float variacionSuave = 0.1f; // Pequeña variación en el color

    /// <summary>
    /// Referencia al componente Image donde se aplicará la textura generada.
    /// </summary>
    private Image image;

    /// <summary>
    /// Textura generada para el fondo.
    /// </summary>
    private Texture2D texturaFondo;

    /// <summary>
    /// Inicializa la referencia al componente Image.
    /// </summary>
    void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// Genera la textura minimalista al iniciar el componente.
    /// </summary>
    void Start()
    {
        GenerarTexturaMinimalista();
    }

    /// <summary>
    /// Genera una textura minimalista con gradiente, variación de color y puntos sutiles, y la aplica al componente Image.
    /// </summary>
    void GenerarTexturaMinimalista()
    {
        int tamañoTextura = 1024;
        texturaFondo = new Texture2D(tamañoTextura, tamañoTextura);

        // Inicializar con color de fondo
        for (int y = 0; y < tamañoTextura; y++)
        {
            for (int x = 0; x < tamañoTextura; x++)
            {
                // Crear un gradiente muy sutil
                float distancia = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(tamañoTextura / 2f, tamañoTextura / 2f)
                ) / (tamañoTextura / 1.5f);

                distancia = Mathf.Clamp01(distancia);
                Color colorPixel = Color.Lerp(colorFondo, colorAcento, distancia);

                // Añadir mínima variación utilizando ruido Perlin
                float ruido = Mathf.PerlinNoise(x * 0.005f, y * 0.005f) * variacionSuave;
                colorPixel = new Color(
                    colorPixel.r + ruido,
                    colorPixel.g + ruido,
                    colorPixel.b + ruido,
                    1
                );

                texturaFondo.SetPixel(x, y, colorPixel);
            }
        }

        // Añadir puntos muy sutiles y distanciados
        for (int y = 0; y < tamañoTextura; y += espaciadoPatron)
        {
            for (int x = 0; x < tamañoTextura; x += espaciadoPatron)
            {
                // Pequeña variación aleatoria en la posición
                int posX = x + Random.Range(-20, 20);
                int posY = y + Random.Range(-20, 20);

                if (posX >= 0 && posX < tamañoTextura && posY >= 0 && posY < tamañoTextura)
                {
                    DibujarPuntoSuave(posX, posY, tamañoPunto, colorAcento, opacidadPunto, tamañoTextura);
                }
            }
        }

        texturaFondo.Apply();

        // Aplicar a la imagen
        Sprite sprite = Sprite.Create(texturaFondo, new Rect(0, 0, tamañoTextura, tamañoTextura), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        image.type = Image.Type.Simple; // No repetir la textura
    }

    /// <summary>
    /// Dibuja un punto suave (con bordes difuminados) en la textura en la posición indicada.
    /// </summary>
    /// <param name="centroX">Coordenada X del centro del punto.</param>
    /// <param name="centroY">Coordenada Y del centro del punto.</param>
    /// <param name="radio">Radio del punto.</param>
    /// <param name="color">Color base del punto.</param>
    /// <param name="opacidad">Opacidad máxima del punto.</param>
    /// <param name="tamañoTextura">Tamaño de la textura (ancho y alto).</param>
    void DibujarPuntoSuave(int centroX, int centroY, int radio, Color color, float opacidad, int tamañoTextura)
    {
        Color colorPunto = new Color(color.r, color.g, color.b, opacidad);

        for (int y = -radio; y <= radio; y++)
        {
            for (int x = -radio; x <= radio; x++)
            {
                float distancia = Mathf.Sqrt(x * x + y * y) / radio;

                if (distancia <= 1)
                {
                    int px = centroX + x;
                    int py = centroY + y;

                    if (px >= 0 && px < tamañoTextura && py >= 0 && py < tamañoTextura)
                    {
                        // Desvanecer bordes
                        float alpha = opacidad * (1.0f - distancia * distancia);
                        Color colorActual = texturaFondo.GetPixel(px, py);
                        Color colorNuevo = Color.Lerp(colorActual, colorPunto, alpha);
                        texturaFondo.SetPixel(px, py, colorNuevo);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Regenera el patrón de fondo. Puede ser llamado desde el editor.
    /// </summary>
    public void RegenerarPatron()
    {
        if (gameObject.activeInHierarchy)
        {
            GenerarTexturaMinimalista();
        }
    }
}