using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para Coroutine y IEnumerator

/// <summary>
/// Controlador para manejar la visualización de un panel de imagen con efectos de fade-in y fade-out.
/// </summary>
public class PanelImagenController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelContenedor; // Contenedor del panel de imagen.
    [SerializeField] private Image imagenMostrada; // Imagen que se mostrará en el panel.
    [SerializeField] private Button botonCerrar; // Botón para cerrar el panel.

    [Header("Opciones de Imagen")]
    [SerializeField] private Vector2 tamañoImagen = new Vector2(800, 800); // Tamaño de la imagen mostrada.
    [SerializeField] private float margenBotonCerrar = 20f; // Margen del botón de cerrar respecto al borde del panel.

    [Header("Opciones del Panel")]
    [SerializeField] private Color colorFondo = new Color(0, 0, 0, 0.8f); // Color de fondo del panel.
    [SerializeField] private Vector2 tamañoPanel = new Vector2(1000, 1000); // Tamaño del panel.

    [Header("Fade")]
    /// <summary>
    /// Duración del efecto de fade-in y fade-out.
    /// </summary>
    [SerializeField] private float duracionFade = 0.5f;

    /// <summary>
    /// Componente para manejar la opacidad y la interactividad del panel.
    /// </summary>
    private CanvasGroup canvasGroup;
    /// <summary>
    /// Corrutina activa para el efecto de fade.
    /// </summary>
    private Coroutine fadeCoroutine;
    /// <summary>
    /// Imagen de fondo del panel.
    /// </summary>
    private Image panelBackgroundImage;
    /// <summary>
    /// RectTransform del panel.
    /// </summary>
    private RectTransform panelRectTransform;
    /// <summary>
    /// RectTransform de la imagen mostrada.
    /// </summary>
    private RectTransform imagenRectTransform;
    /// <summary>
    /// Referencia al controlador de la cámara.
    /// </summary>
    private CameraView cameraView;
    /// <summary>
    /// Referencia al controlador de movimiento automático.
    /// </summary>
    private AutomaticMovementController autoCamera;

    /// <summary>
    /// Inicializa las referencias internas al despertar el objeto.
    /// </summary>
    void Awake()
    {
        cameraView = GameObject.Find("MainCamera").GetComponent<CameraView>();

        // Buscar el controlador de movimiento automático
        autoCamera = FindObjectOfType<AutomaticMovementController>();

        if (panelContenedor != null)
        {
            panelBackgroundImage = panelContenedor.GetComponent<Image>();
            panelRectTransform = panelContenedor.GetComponent<RectTransform>();
            canvasGroup = panelContenedor.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = panelContenedor.AddComponent<CanvasGroup>();
            }
        }

        if (imagenMostrada != null)
        {
            imagenRectTransform = imagenMostrada.rectTransform;
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarPanel);
        }
    }

    /// <summary>
    /// Configura las propiedades iniciales del panel y sus elementos al inicio.
    /// </summary>
    void Start()
    {
        if (panelContenedor != null)
        {
            panelContenedor.SetActive(false);
            if (panelBackgroundImage != null) panelBackgroundImage.color = colorFondo;
            if (panelRectTransform != null) panelRectTransform.sizeDelta = tamañoPanel;
        }

        if (imagenRectTransform != null)
        {
            imagenRectTransform.sizeDelta = tamañoImagen;
        }

        AjustarPosicionBotonCerrar();
    }

    /// <summary>
    /// Muestra el panel con una imagen específica y aplica un efecto de fade-in.
    /// </summary>
    /// <param name="imagen">Sprite de la imagen que se mostrará en el panel.</param>
    public void MostrarPanel(Sprite imagen)
    {
        if (panelContenedor == null || imagenMostrada == null || imagen == null) return;

        // Notificar al controlador de cámara automática que se ha abierto un punto de interés
        if (autoCamera != null)
        {
            autoCamera.OnPointOfInterestOpened();
        }

        imagenMostrada.sprite = imagen;
        if (imagenRectTransform != null) imagenRectTransform.sizeDelta = tamañoImagen;

        if (!panelContenedor.activeSelf)
        {
            panelContenedor.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        AjustarPosicionBotonCerrar();
        StartFade(canvasGroup != null ? canvasGroup.alpha : 0, 1);
        cameraView.RotateCameraToLookAt(gameObject.transform.position);
    }

    /// <summary>
    /// Cierra el panel aplicando un efecto de fade-out.
    /// </summary>
    public void CerrarPanel()
    {
        if (panelContenedor == null || !panelContenedor.activeSelf) return;

        // Notificar al controlador de cámara automática que se ha cerrado un punto de interés
        if (autoCamera != null)
        {
            autoCamera.OnPointOfInterestClosed();
        }

        StartFade(canvasGroup != null ? canvasGroup.alpha : 1, 0);
    }

    /// <summary>
    /// Apaga la imagen y notifica al controlador de cámara automática si corresponde.
    /// </summary>
    public void ApagarImagen()
    {
        if (panelContenedor.activeSelf && canvasGroup != null && canvasGroup.alpha > 0.1f)
        {
            panelContenedor.SetActive(false);
        }
        // Notificar al controlador de cámara automática que se ha cerrado un punto de interés
        if (autoCamera != null)
        {
            autoCamera.OnPointOfInterestClosed();
        }
    }

    /// <summary>
    /// Inicia el efecto de fade entre dos valores de opacidad.
    /// </summary>
    /// <param name="from">Valor inicial de opacidad.</param>
    /// <param name="to">Valor final de opacidad.</param>
    private void StartFade(float from, float to)
    {
        if (canvasGroup == null)
        {
            if (panelContenedor != null) panelContenedor.SetActive(to > 0);
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadePanel(from, to));
    }

    /// <summary>
    /// Corrutina que realiza el efecto de fade-in o fade-out.
    /// </summary>
    /// <param name="startAlpha">Valor inicial de opacidad.</param>
    /// <param name="endAlpha">Valor final de opacidad.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator FadePanel(float startAlpha, float endAlpha)
    {
        float t = 0f;
        float currentAlpha = canvasGroup.alpha;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duracionFade);
            canvasGroup.alpha = Mathf.Lerp(currentAlpha, endAlpha, normalized);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (endAlpha >= 1f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            panelContenedor.SetActive(false);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        fadeCoroutine = null;
    }

    /// <summary>
    /// Ajusta la posición del botón de cerrar en la esquina superior derecha del panel.
    /// </summary>
    private void AjustarPosicionBotonCerrar()
    {
        if (botonCerrar == null || panelContenedor == null) return;

        RectTransform botonRect = botonCerrar.GetComponent<RectTransform>();
        if (botonRect == null) return;

        botonRect.anchorMin = new Vector2(1, 1);
        botonRect.anchorMax = new Vector2(1, 1);
        botonRect.pivot = new Vector2(1, 1);
        botonRect.anchoredPosition = new Vector2(-margenBotonCerrar, -margenBotonCerrar);
    }

    /// <summary>
    /// Para asegurar que la cámara automática se desactiva incluso si se destruye este objeto.
    /// </summary>
    void OnDestroy()
    {
        if (autoCamera != null && panelContenedor != null && panelContenedor.activeInHierarchy)
        {
            autoCamera.OnPointOfInterestClosed();
        }
    }
}