using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


/// <summary>
/// Controla la visualización y movimiento de un texto UI al pasar el ratón por encima.
/// </summary>
public class HoverTextDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Elemento de texto UI que se mostrará al hacer hover.
    /// </summary>
    [SerializeField] private TextMeshProUGUI textDisplayElement;

    /// <summary>
    /// RectTransform del Canvas que se usará para calcular la posición del texto.
    /// </summary>
    [SerializeField] private RectTransform canvasRectTransform;

    /// <summary>
    /// Desplazamiento del texto respecto a la posición del ratón.
    /// </summary>
    private Vector2 textOffset = new Vector2(5f, -5f);

    /// <summary>
    /// Indica si el puntero está actualmente sobre el objeto.
    /// </summary>
    private bool isHovering = false;

    /// <summary>
    /// Inicializa el componente. Desactiva el script si falta el elemento de texto.
    /// </summary>
    void Awake()
    {
        if (textDisplayElement == null)
        {
            enabled = false; // Deshabilita si falta el elemento de texto
            return;
        }

        textDisplayElement.gameObject.SetActive(false); // Ocultar al inicio
    }

    /// <summary>
    /// Actualiza la posición del texto si el puntero está sobre el objeto.
    /// </summary>
    void Update()
    {
        // Solo actualizar la posición si se está haciendo hover Y se ha asignado un Canvas para seguimiento
        if (isHovering && canvasRectTransform != null)
        {
            UpdateHoverTextPosition();
        }
    }

    /// <summary>
    /// Evento que se dispara al entrar el puntero sobre el objeto.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enabled) return;

        isHovering = true;
        textDisplayElement.gameObject.SetActive(true);

        // Solo actualizar la posición al entrar si se va a seguir al ratón
        if (canvasRectTransform != null)
        {
            UpdateHoverTextPosition(); // Posiciona el texto al entrar si hay seguimiento
        }
        // Si canvasRectTransform es null, el texto se activa en su posición actual (fija)
    }

    /// <summary>
    /// Evento que se dispara al salir el puntero del objeto.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled) return;

        isHovering = false;
        textDisplayElement.gameObject.SetActive(false);
    }

    /// <summary>
    /// Actualiza la posición del texto en función de la posición del ratón.
    /// </summary>
    private void UpdateHoverTextPosition()
    {
        // Esta función solo se llama si canvasRectTransform != null

        Canvas canvas = canvasRectTransform.GetComponent<Canvas>();
        Camera renderingCamera = (canvas != null && canvas.worldCamera != null) ? canvas.worldCamera : Camera.main;

        if (renderingCamera == null)
        {
            // No hay cámara para convertir coordenadas, no podemos seguir al ratón.
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, renderingCamera, out Vector2 localPoint))
        {
            textDisplayElement.rectTransform.anchoredPosition = localPoint + textOffset;
        }
    }
}
