using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente que genera un fondo de gradiente radial suave con ruido sutil para una imagen de UI en Unity.
/// </summary>
[RequireComponent(typeof(Image))]
public class GradientBackground : MonoBehaviour
{
    /// <summary>
    /// Color en el centro del gradiente.
    /// </summary>
    [Header("Configuración de Colores")]
    public Color colorCentro = new Color(0.15f, 0.2f, 0.35f); // Azul suave

    /// <summary>
    /// Color en el borde del gradiente.
    /// </summary>
    public Color colorBorde = new Color(0.08f, 0.12f, 0.25f); // Azul oscuro

    /// <summary>
    /// Tamaño de la textura generada para el gradiente.
    /// </summary>
    [Header("Estilo Suave")]
    [Range(512, 2048)]
    public int tamañoTextura = 1024;

    /// <summary>
    /// Controla la suavidad de la transición del gradiente (exponente de la curva).
    /// </summary>
    [Range(0, 2)]
    public float intensidadGradiente = 1.2f; // Controla la suavidad de la transición

    /// <summary>
    /// Intensidad del ruido Perlin aplicado para evitar bandas de color.
    /// </summary>
    [Range(0, 0.05f)]
    public float intensidadRuido = 0.015f; // Ruido muy sutil para evitar bandas de color

    /// <summary>
    /// Referencia al componente Image donde se aplicará el gradiente.
    /// </summary>
    private Image image;

    /// <summary>
    /// Inicializa la referencia al componente Image.
    /// </summary>
    void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// Genera el fondo de gradiente suave al iniciar el componente.
    /// </summary>
    void Start()
    {
        GenerarFondoSuave();
    }

    /// <summary>
    /// Genera una textura de gradiente radial suave con ruido sutil y la aplica como sprite al componente Image.
    /// </summary>
    public void GenerarFondoSuave()
    {
        int ancho = tamañoTextura;
        int alto = tamañoTextura;
        Texture2D textura = new Texture2D(ancho, alto);

        float centroX = ancho / 2f;
        float centroY = alto / 2f;
        float maxDistancia = Mathf.Sqrt(centroX * centroX + centroY * centroY);

        // Crear gradiente radial con ruido muy sutil
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                // Calcular distancia desde el centro
                float distanciaX = centroX - x;
                float distanciaY = centroY - y;
                float distancia = Mathf.Sqrt(distanciaX * distanciaX + distanciaY * distanciaY);

                // Ajustar la curva de la transición
                float distanciaNormalizada = Mathf.Clamp01(distancia / maxDistancia);
                distanciaNormalizada = Mathf.Pow(distanciaNormalizada, intensidadGradiente);

                // Aplicar función de suavizado para evitar bandas
                distanciaNormalizada = Mathf.SmoothStep(0, 1, distanciaNormalizada);

                // Añadir un ruido Perlin muy sutil para textura natural
                float ruido = (Mathf.PerlinNoise(x * 0.01f, y * 0.01f) - 0.5f) * intensidadRuido;

                // Combinar gradiente con ruido (el ruido es casi imperceptible)
                float valorFinal = Mathf.Clamp01(distanciaNormalizada + ruido);

                // Interpolar entre colores
                Color colorPixel = Color.Lerp(colorCentro, colorBorde, valorFinal);
                textura.SetPixel(x, y, colorPixel);
            }
        }

        textura.Apply();

        // Aplicar a la imagen
        Sprite sprite = Sprite.Create(textura, new Rect(0, 0, ancho, alto), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
    }
}