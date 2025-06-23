using UnityEngine;
using UnityEngine.EventSystems;

public class CursorChangerMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Texturas para el cursor")]
    [SerializeField] private Texture2D normalCursor;     // Cursor por defecto
    [SerializeField] private Texture2D handCursor;       // Cursor de mano 
    [SerializeField] private Vector2 hotSpot = Vector2.zero; // Punto de anclaje del cursor

    private bool isMouseOver = false; // Track si el mouse está encima

    /// <summary>
    /// Inicializa el cursor al iniciar el componente, estableciéndolo en el cursor por defecto.
    /// </summary>
    private void Start()
    {
        // Al iniciar, se pone el cursor por defecto
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
    }

    // Evento cuando el puntero entra en el botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        Cursor.SetCursor(handCursor, hotSpot, CursorMode.Auto);
    }

    // Evento cuando el puntero sale del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
    }

    // Evento cuando se hace clic
    public void OnPointerClick(PointerEventData eventData)
    {
        // Si el mouse sigue encima después del clic, mantener la mano
        if (isMouseOver)
        {
            Cursor.SetCursor(handCursor, hotSpot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
        }
    }
}