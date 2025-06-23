using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controlador para cambiar la textura del cursor según el estado del botón y las transiciones del Skybox.
/// Gestiona el cambio entre el cursor normal y el de mano, dependiendo de si el usuario puede interactuar con el carrusel.
/// </summary>
public class CursorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Texturas para el cursor")]
    [SerializeField] private Texture2D normalCursor;     // Cursor por defecto
    [SerializeField] private Texture2D handCursor;       // Cursor de mano 
    [SerializeField] private Vector2 hotSpot = Vector2.zero; // Punto de anclaje del cursor

    [Header("Referencia al objeto Content (puede estar desactivado al principio)")]
    [SerializeField] private GameObject content; // Objeto que contiene SkyBoxButtonSelected 

    private MaterialTransition skyboxFader;               // Script que gestiona transiciones 
    private SkyBoxButtonSelected skyboxButtonHandler;     // Script que gestiona el estado del carrusel

    private bool isPointerOver = false;
    private bool currentCursorIsHand = false;

    /// <summary>
    /// Inicializa el script con una referencia al objeto content.
    /// Permite la inicialización desde otro script si se crea dinámicamente.
    /// </summary>
    /// <param name="contentObject">Objeto que contiene el componente SkyBoxButtonSelected.</param>
    public void Initialize(GameObject contentObject)
    {
        content = contentObject;

        if (content != null)
            skyboxButtonHandler = content.GetComponent<SkyBoxButtonSelected>(); // Intenta obtener el script necesario
    }

    /// <summary>
    /// Inicializa el cursor y busca las referencias necesarias al iniciar el componente.
    /// </summary>
    private void Start()
    {
        // Al iniciar, se pone el cursor por defecto
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);

        // Busca el script de transiciones en la cámara principal
        skyboxFader = GameObject.Find("MainCamera").GetComponent<MaterialTransition>();

        // Si aún no se ha asignado el handler del botón y el content existe, intenta obtenerlo
        if (skyboxButtonHandler == null && content != null)
        {
            skyboxButtonHandler = content.GetComponent<SkyBoxButtonSelected>();
        }
    }

    /// <summary>
    /// Actualiza el estado del cursor cada frame si el puntero está sobre el botón.
    /// Cambia entre el cursor normal y el de mano según el estado de las transiciones y el carrusel.
    /// </summary>
    private void Update()
    {
        // Solo actualiza el cursor si el puntero está sobre este botón
        if (!isPointerOver) return;

        // Evalúa si se debe mostrar el cursor de mano (todas las condiciones deben cumplirse)
        bool shouldShowHand =
            !skyboxFader.isFading &&                          // No debe estar haciendo fade
            skyboxFader.flagBtnCarousel &&                    // El botón del carrusel debe estar habilitado
            skyboxButtonHandler != null &&                    // Asegura que el handler existe
            (skyboxButtonHandler.carruselListo || skyboxButtonHandler.Bandera()); // Al menos una de estas debe ser true

        // Si debería mostrarse la mano y no lo está, la cambia
        if (shouldShowHand && !currentCursorIsHand)
        {
            Cursor.SetCursor(handCursor, hotSpot, CursorMode.Auto);
            currentCursorIsHand = true;
        }
        // Si ya no debería mostrarse la mano pero aún lo está, cambia al cursor normal
        else if (!shouldShowHand && currentCursorIsHand)
        {
            Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
            currentCursorIsHand = false;
        }
    }

    /// <summary>
    /// Evento de Unity que se llama cuando el puntero entra en el área del botón.
    /// Marca que el puntero está sobre el botón y fuerza la reevaluación del cursor.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        currentCursorIsHand = false; // Forzamos que en el próximo Update se evalúe si cambiar el cursor
    }

    /// <summary>
    /// Evento de Unity que se llama cuando el puntero sale del área del botón.
    /// Restaura el cursor al estado normal y marca que el puntero ya no está sobre el botón.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto); // Siempre vuelve al cursor normal al salir
        currentCursorIsHand = false;
    }

    /// <summary>
    /// Evento de Unity que se llama cuando se hace clic en el botón.
    /// Restaura el cursor al estado normal y, si corresponde, desactiva el estado de puntero sobre el botón.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Siempre forzamos el cursor normal
        Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
        currentCursorIsHand = false;

        // Si justo después del clic se dispara un cambio de bandera deja el cursor limpio
        if (skyboxFader != null && (!skyboxFader.flagBtnCarousel || skyboxFader.isFading))
        {
            Cursor.SetCursor(normalCursor, hotSpot, CursorMode.Auto);
            currentCursorIsHand = false;
            isPointerOver = false;
        }
    }
}