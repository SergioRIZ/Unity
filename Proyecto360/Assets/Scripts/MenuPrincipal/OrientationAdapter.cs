using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script para adaptar la interfaz de usuario según la orientación de la pantalla.
/// </summary>
public class OrientationAdapter : MonoBehaviour
{
    [Header("Referencias de la UI")]
    [SerializeField] private RectTransform androidMainContent;
    [SerializeField] private RectTransform[] imageContainers;

    [Header("Configuración de tamaños")]
    [SerializeField] private Vector2 portraitSize = new Vector2(350, 170);       // Reducido de 450, 220
    [SerializeField] private Vector2 landscapeSize = new Vector2(450, 225);      // Reducido de 600, 300
    [SerializeField] private Vector2 portraitSpacing = new Vector2(25, 25);
    [SerializeField] private Vector2 landscapeSpacing = new Vector2(50, 15);

    [Header("Posiciones de elementos UI")]
    [SerializeField] private RectTransform titleText;
    [SerializeField] private RectTransform contentLabel;
    [SerializeField] private float portraitTopMargin = 150f;
    [SerializeField] private float landscapeTopMargin = 80f;

    /// <summary>
    /// Almacena la orientación actual de la pantalla.
    /// </summary>
    private ScreenOrientation currentOrientation;

    /// <summary>
    /// Referencia al componente GridLayoutGroup del contenido principal.
    /// </summary>
    private GridLayoutGroup gridLayout;

    /// <summary>
    /// Referencia al componente ScrollRect asociado al contenido principal.
    /// </summary>
    private ScrollRect scrollRect;

    /// <summary>
    /// Referencia al componente ContentSizeFitter del contenido principal.
    /// </summary>
    private ContentSizeFitter contentSizeFitter;

    /// <summary>
    /// Inicializa referencias y aplica la configuración de orientación al iniciar.
    /// </summary>
    void Start()
    {
        currentOrientation = Screen.orientation;

        if (androidMainContent != null)
        {
            gridLayout = androidMainContent.GetComponent<GridLayoutGroup>();
            scrollRect = androidMainContent.GetComponentInParent<ScrollRect>();
            contentSizeFitter = androidMainContent.GetComponent<ContentSizeFitter>();
        }

        ApplyOrientationSettings();
    }

    /// <summary>
    /// Detecta cambios de orientación y actualiza la UI si es necesario.
    /// </summary>
    void Update()
    {
        if (currentOrientation != Screen.orientation)
        {
            currentOrientation = Screen.orientation;
            ApplyOrientationSettings();
            Invoke("ResetScrollPosition", 0.2f);
        }
    }

    /// <summary>
    /// Aplica configuraciones de UI según la orientación actual.
    /// Ajusta tamaños, posiciones y escalado de los elementos de la interfaz.
    /// </summary>
    void ApplyOrientationSettings()
    {
        bool isLandscape = IsLandscapeOrientation();

        if (gridLayout != null)
        {
            if (isLandscape && Application.platform == RuntimePlatform.Android)
            {
                gridLayout.cellSize = new Vector2(landscapeSize.x * 1.1f, landscapeSize.y * 1.1f); // Reducido de 1.4f, 1.2f
                gridLayout.spacing = new Vector2(landscapeSpacing.x * 1.2f, landscapeSpacing.y * 1.2f);
                gridLayout.constraintCount = 3;
            }
            else
            {
                gridLayout.cellSize = isLandscape ? landscapeSize : portraitSize;
                gridLayout.spacing = isLandscape ? landscapeSpacing : portraitSpacing;
                gridLayout.constraintCount = isLandscape ? 2 : 1;
            }
        }

        foreach (RectTransform container in imageContainers)
        {
            if (container != null)
            {
                container.sizeDelta = isLandscape ?
                    new Vector2(landscapeSize.x * 0.9f, landscapeSize.y * 0.9f) : // Reducido de 1.1f
                    new Vector2(portraitSize.x * 0.9f, portraitSize.y * 0.9f);    // Reducido de 1.1f
            }
        }

        if (titleText != null)
        {
            float yPos = isLandscape ? landscapeTopMargin : portraitTopMargin;
            titleText.anchoredPosition = new Vector2(titleText.anchoredPosition.x, yPos);
        }

        if (contentLabel != null)
        {
            float yPos = isLandscape ? (landscapeTopMargin - 40f) : (portraitTopMargin - 60f);
            contentLabel.anchoredPosition = new Vector2(contentLabel.anchoredPosition.x, yPos);
        }

        if (contentSizeFitter != null)
        {
            Canvas.ForceUpdateCanvases();
            contentSizeFitter.enabled = false;
            contentSizeFitter.enabled = true;
        }

        AdjustCanvasScaler();
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Ajusta el CanvasScaler para la orientación actual.
    /// Cambia la resolución de referencia y el modo de escalado.
    /// </summary>
    void AdjustCanvasScaler()
    {
        bool isLandscape = IsLandscapeOrientation();
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();

        foreach (CanvasScaler scaler in scalers)
        {
            scaler.referenceResolution = isLandscape ?
                new Vector2(1920, 1080) : new Vector2(1080, 1920);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.matchWidthOrHeight = isLandscape ? 0.5f : 1f;
        }
    }

    /// <summary>
    /// Restablece la posición del scroll al inicio.
    /// </summary>
    void ResetScrollPosition()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1.0f;
            scrollRect.horizontalNormalizedPosition = 0.0f;
        }
    }

    /// <summary>
    /// Método para ajustar manualmente la orientación (útil para testing).
    /// </summary>
    public void ForceOrientationUpdate()
    {
        ApplyOrientationSettings();
        ResetScrollPosition();
    }

    /// <summary>
    /// Comprueba si la orientación actual es horizontal.
    /// </summary>
    /// <returns>True si la orientación es Landscape, false si es Portrait.</returns>
    bool IsLandscapeOrientation()
    {
        return (currentOrientation == ScreenOrientation.LandscapeLeft ||
                currentOrientation == ScreenOrientation.LandscapeRight);
    }
}