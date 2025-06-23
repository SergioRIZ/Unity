using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


/// <summary>
/// Clase que gestiona la animación y el cambio de color de un botón de menú al interactuar con el puntero.
/// </summary>
[System.Serializable]
public class AnimatedButtonMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Color del texto del botón en estado normal.
    /// </summary>
    [Header("Colores del botón")]
    [SerializeField] private Color colorNormal = Color.white;

    /// <summary>
    /// Color del texto del botón al pasar el puntero por encima.
    /// </summary>
    [SerializeField] private Color colorHover = Color.blue;

    /// <summary>
    /// Referencia al componente TextMeshProUGUI del botón.
    /// </summary>
    private TextMeshProUGUI colorTexto;

    /// <summary>
    /// Inicializa las referencias a los componentes y establece el color inicial del botón.
    /// </summary>
    private void Start()
    {
        colorTexto = GetComponentInChildren<TextMeshProUGUI>();
        if (colorTexto != null)
        {
            colorTexto.color = colorNormal;
        }
    }

    /// <summary>
    /// Evento al entrar el puntero del mouse. Cambia el color y activa la vibración si está configurada.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (colorTexto != null)
        {
            colorTexto.color = colorHover;
        }
    }

    /// <summary>
    /// Evento al salir el puntero del mouse. Restaura el color y detiene la vibración.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (colorTexto != null)
        {
            colorTexto.color = colorNormal;
        }
    }
}
