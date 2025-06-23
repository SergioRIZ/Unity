using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Cambia el color del botón cuando el puntero entra o sale del área del botón.
/// </summary>
public class SimpleButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Referencia al componente Button asociado.
    /// </summary>
    private Button button;

    /// <summary>
    /// Almacena los colores originales del botón para restaurarlos al salir el puntero.
    /// </summary>
    private ColorBlock originalColors;

    /// <summary>
    /// Color que se aplica al botón cuando el puntero está sobre él.
    /// </summary>
    private Color hoverColor = new Color(173f / 255f, 216f / 255f, 230f / 255f, 1f); // R:173, G:216, B:230

    /// <summary>
    /// Inicializa la referencia al botón y almacena sus colores originales.
    /// </summary>
    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            originalColors = button.colors;
        }
    }

    /// <summary>
    /// Cambia el color del botón al color de hover cuando el puntero entra en el área del botón.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = hoverColor;
            colors.selectedColor = hoverColor;
            button.colors = colors;
        }
    }

    /// <summary>
    /// Restaura los colores originales del botón cuando el puntero sale del área del botón.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null)
        {
            button.colors = originalColors; // Restaura los colores originales
        }
    }
}