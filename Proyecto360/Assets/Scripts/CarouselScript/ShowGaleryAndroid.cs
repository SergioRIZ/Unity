using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clase que controla la visualización de una galería en Android, incluyendo la animación de un panel y la inicialización de un carrusel.
/// </summary>
public class ShowGaleryAndroid : MonoBehaviour
{
    [Header("Panel que se va a mostrar/ocultar")]
    /// <summary>
    /// Panel que se anima (ScrollView).
    /// </summary>
    [SerializeField] private RectTransform targetPanel;

    [Header("Posiciones (anchoredPosition)")]
    /// <summary>
    /// Posición del panel cuando está oculto.
    /// </summary>
    [SerializeField] private Vector2 hiddenPosition;

    /// <summary>
    /// Posición del panel cuando está visible.
    /// </summary>
    [SerializeField] private Vector2 shownPosition;

    [Header("Tiempo de animación")]
    /// <summary>
    /// Duración de la animación de apertura/cierre del panel.
    /// </summary>
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Texto del botón")]
    /// <summary>
    /// Referencia al texto del botón que alterna la visibilidad del panel.
    /// </summary>
    private TextMeshProUGUI buttonText;

    /// <summary>
    /// Indica si el panel está abierto o cerrado.
    /// </summary>
    private bool isOpen = false;

    /// <summary>
    /// Corrutina activa para la animación del panel.
    /// </summary>
    private Coroutine animationCoroutine;

    /// <summary>
    /// Componente CanvasGroup del panel, utilizado para controlar la visibilidad y la interacción.
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// Referencia al componente encargado de inicializar el carrusel.
    /// </summary>
    private SkyBoxButtonSelected skyBoxButtonSelected;

    /// <summary>
    /// Bandera para determinar si el carrusel ya ha sido renderizado.
    /// </summary>
    private bool carouselRender = true;

    /// <summary>
    /// Método llamado al inicializar el script. Configura el estado inicial del panel y fuerza la inicialización del carrusel.
    /// </summary>
    private void Awake()
    {
        skyBoxButtonSelected = GameObject.Find("Content Android").GetComponent<SkyBoxButtonSelected>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        canvasGroup = targetPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetPanel.gameObject.AddComponent<CanvasGroup>();
        }

        // Inicializamos el estado cerrado
        targetPanel.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        targetPanel.gameObject.SetActive(false);
        buttonText.text = "Abrir";

        StartCoroutine(ForceInitializeGalleryAtStart());
    }

    /// <summary>
    /// Alterna la visibilidad del panel, mostrando u ocultando con animación.
    /// </summary>
    public void Toggle()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        if (!isOpen)
        {
            targetPanel.gameObject.SetActive(true);
            if (carouselRender)
            {
                skyBoxButtonSelected.InitializeCarouselRender();
                carouselRender = false;
            }

            canvasGroup.alpha = 0f;
            targetPanel.anchoredPosition = hiddenPosition;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            animationCoroutine = StartCoroutine(AnimatePanel(hiddenPosition, shownPosition, 0f, 1f, true));
            buttonText.text = "Cerrar";
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            animationCoroutine = StartCoroutine(AnimatePanel(targetPanel.anchoredPosition, hiddenPosition, 1f, 0f, true));
            buttonText.text = "Abrir";
        }

        isOpen = !isOpen;
    }

    /// <summary>
    /// Corrutina que anima el panel desde una posición y opacidad inicial hasta una posición y opacidad final.
    /// </summary>
    /// <param name="fromPos">Posición inicial del panel.</param>
    /// <param name="toPos">Posición final del panel.</param>
    /// <param name="fromAlpha">Opacidad inicial del panel.</param>
    /// <param name="toAlpha">Opacidad final del panel.</param>
    /// <param name="setActiveAtEnd">Indica si el panel debe permanecer activo al finalizar la animación.</param>
    /// <returns>Corrutina de animación.</returns>
    private IEnumerator AnimatePanel(Vector2 fromPos, Vector2 toPos, float fromAlpha, float toAlpha, bool setActiveAtEnd)
    {
        float time = 0f;

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            targetPanel.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            time += Time.deltaTime;
            yield return null;
        }

        targetPanel.anchoredPosition = toPos;
        canvasGroup.alpha = toAlpha;

        if (!setActiveAtEnd)
        {
            targetPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Corrutina que fuerza la inicialización del carrusel al inicio, asegurando que los layouts se actualicen correctamente.
    /// </summary>
    /// <returns>Corrutina de inicialización.</returns>
    private IEnumerator ForceInitializeGalleryAtStart()
    {
        // 1. Activamos el panel completamente
        targetPanel.anchoredPosition = shownPosition;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        //  FORZAMOS el layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(targetPanel);
        Canvas.ForceUpdateCanvases();

        // 2. Esperamos un frame para que se actualicen todos los layouts hijos
        yield return null;

        // 3. Ocultamos normalmente
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        buttonText.text = "Abrir";

        isOpen = false;
    }

    /// <summary>
    /// Cierra el carrusel al hacer clic, ocultando el panel con animación.
    /// </summary>
    public void CloseCorouselOnClick()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        animationCoroutine = StartCoroutine(AnimatePanel(targetPanel.anchoredPosition, hiddenPosition, 1f, 0f, true));
        buttonText.text = "Abrir";
        isOpen = false;
    }
}
