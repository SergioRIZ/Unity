using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


/// <summary>
/// Controlador para manejar la visualización y animación de un panel de texto en la interfaz de usuario.
/// </summary>
public class PanelTextoController : MonoBehaviour
{
    // --- Referencias Necesarias ---
    /// <summary>
    /// Objeto que representa el fondo del panel (para color, etc.).
    /// </summary>
    [SerializeField] private GameObject panelFondoTextoObjeto;

    /// <summary>
    /// El componente TextMeshProUGUI que mostrará el texto.
    /// </summary>
    [SerializeField] private TextMeshProUGUI textoMostrado;

    /// <summary>
    /// El botón para cerrar el panel.
    /// </summary>
    [SerializeField] private Button botonCerrar;

    // --- Propiedades de Apariencia del Panel ---
    /// <summary>
    /// Color del fondo del panel.
    /// </summary>
    [SerializeField] private Color colorFondo = new Color(0, 0, 0, 0.8f);

    /// <summary>
    /// Margen del botón de cerrar respecto al borde del panel.
    /// </summary>
    [SerializeField] private float margenBotonCerrar = 20f;

    // --- Animación ---
    /// <summary>
    /// Duración de la animación de fade-in y fade-out en segundos.
    /// </summary>
    [Header("Animación")]
    [SerializeField] private float duracionFade = 0.5f;

    // --- Variables Internas ---
    /// <summary>
    /// Componente CanvasGroup utilizado para manejar la visibilidad y la interactividad del panel.
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// Referencia a la corrutina activa de fade.
    /// </summary>
    private Coroutine fadeCoroutine;

    /// <summary>
    /// Referencia al controlador de la cámara principal.
    /// </summary>
    private CameraView cameraView;

    /// <summary>
    /// Referencia al controlador de movimiento automático.
    /// </summary>
    private AutomaticMovementController autoCamera;

    /// <summary>
    /// Inicializa las referencias y configura el estado inicial del panel.
    /// </summary>
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        cameraView = GameObject.Find("MainCamera").GetComponent<CameraView>();
        autoCamera = FindObjectOfType<AutomaticMovementController>();
        if (botonCerrar != null)
        {
            // Asegúrate de que el botón llame al método de cerrar
            botonCerrar.onClick.AddListener(CerrarPanel);
        }

        // Forzar desactivación al inicio
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        // Inicialmente desactiva todo el objeto del panel
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Muestra el panel con una animación de fade-in.
    /// </summary>
    public void MostrarPanel()
    {

        if (canvasGroup == null)
        {
            gameObject.SetActive(true);
            OnEnable();
            return;
        }

        if (gameObject.activeSelf && canvasGroup.alpha >= 1f) return;
        // Notificar al controlador de cámara automática que se ha abierto un punto de interés
        if (autoCamera != null)
        {
            autoCamera.OnPointOfInterestOpened();
        }
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        StartFade(canvasGroup.alpha, 1);
        cameraView.RotateCameraToLookAt(gameObject.transform.position);
    }

    /// <summary>
    /// Cierra el panel con una animación de fade-out.
    /// </summary>
    public void CerrarPanel()
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }
        // Notificar al controlador de cámara automática que se ha cerrado un punto de interés
        if (autoCamera != null)
        {
            autoCamera.OnPointOfInterestClosed();
        }

        if (!gameObject.activeSelf || canvasGroup.alpha <= 0f) return;

        StartFade(canvasGroup.alpha, 0);
    }

    /// <summary>
    /// Configura la apariencia del panel y ajusta su diseño al activarse.
    /// </summary>
    void OnEnable()
    {
        AjustarPosicionBotonCerrar();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ForceRebuildLayouts());
        }
    }

    /// <summary>
    /// Resetea el estado del panel al desactivarse.
    /// </summary>
    void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Corrutina para forzar la reconstrucción de los layouts de UI.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator ForceRebuildLayouts()
    {
        yield return null;

        if (textoMostrado != null && textoMostrado.rectTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(textoMostrado.rectTransform);

            if (textoMostrado.rectTransform.parent != null)
            {
                var parentRect = textoMostrado.rectTransform.parent.GetComponent<RectTransform>();
                if (parentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                }
            }
        }
    }

    /// <summary>
    /// Configura el color del fondo del panel.
    /// </summary>
    private void ConfigurarAparienciaPanel()
    {
        if (panelFondoTextoObjeto != null)
        {
            Image panelImage = panelFondoTextoObjeto.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = colorFondo;
            }
        }
    }

    /// <summary>
    /// Ajusta la posición del botón de cerrar en el panel.
    /// </summary>
    private void AjustarPosicionBotonCerrar()
    {
        if (panelFondoTextoObjeto != null && botonCerrar != null)
        {
            RectTransform botonRect = botonCerrar.GetComponent<RectTransform>();
            botonRect.anchoredPosition = new Vector2(-margenBotonCerrar, -margenBotonCerrar);
        }
    }

    /// <summary>
    /// Inicia o detiene la corrutina de fade para el panel.
    /// </summary>
    /// <param name="from">Alpha inicial.</param>
    /// <param name="to">Alpha final.</param>
    private void StartFade(float from, float to)
    {
        if (canvasGroup == null)
        {
            bool shouldActivate = (to > from);
            gameObject.SetActive(shouldActivate);
            if (shouldActivate) OnEnable();
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadePanel(from, to));
    }

    /// <summary>
    /// Corrutina que maneja la animación de fade-in y fade-out del panel.
    /// </summary>
    /// <param name="startAlpha">Alpha inicial.</param>
    /// <param name="endAlpha">Alpha final.</param>
    /// <returns>IEnumerator para la animación de fade.</returns>
    private IEnumerator FadePanel(float startAlpha, float endAlpha)
    {
        float t = 0f;
        float currentAlpha = canvasGroup.alpha;

        if (endAlpha > startAlpha && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            OnEnable();
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(currentAlpha, endAlpha, t / duracionFade);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (endAlpha > 0)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(false);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// Cambia el color del fondo del panel.
    /// </summary>
    /// <param name="nuevoColor">Nuevo color a aplicar.</param>
    public void SetColorFondo(Color nuevoColor)
    {
        colorFondo = nuevoColor;
        if (gameObject.activeInHierarchy)
        {
            ConfigurarAparienciaPanel();
        }
    }

    /// <summary>
    /// Cambia el margen del botón de cerrar.
    /// </summary>
    /// <param name="nuevoMargen">Nuevo margen a aplicar.</param>
    public void SetMargenBotonCerrar(float nuevoMargen)
    {
        margenBotonCerrar = nuevoMargen;
        if (gameObject.activeInHierarchy)
        {
            AjustarPosicionBotonCerrar();
        }
    }
}
