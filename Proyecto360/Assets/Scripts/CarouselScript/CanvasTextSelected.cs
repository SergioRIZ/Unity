using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clase que gestiona el comportamiento del texto de un botón en un Canvas cuando el ratón interactúa con él.
/// </summary>
public class CanvasTextSelected : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Referencia al componente TextMeshProUGUI del botón.
    /// </summary>
    private TextMeshProUGUI tmpText;

    /// <summary>
    /// Indica si el texto del botón está subrayado permanentemente.
    /// </summary>
    public bool isPermanentlyUnderlined = false;

    /// <summary>
    /// Obtiene el componente TextMeshProUGUI hijo del botón.
    /// </summary>
    private void Awake()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Evento que se ejecuta al pasar el ratón por encima del botón.
    /// Cambia el estilo del texto a subrayado si no está subrayado permanentemente.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tmpText != null && !isPermanentlyUnderlined)
        {
            // Solo subrayamos si no está subrayado ya
            if ((tmpText.fontStyle & FontStyles.Underline) == 0)
            {
                tmpText.fontStyle |= FontStyles.Underline;
            }
        }
    }

    /// <summary>
    /// Evento que se ejecuta al salir el ratón del botón.
    /// Vuelve el estilo del texto al normal si no está subrayado permanentemente.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tmpText != null && !isPermanentlyUnderlined)
        {
            // Solo quitamos el subrayado si está subrayado
            if ((tmpText.fontStyle & FontStyles.Underline) != 0)
            {
                tmpText.fontStyle &= ~FontStyles.Underline;
            }
        }
    }

    /// <summary>
    /// Establece si el texto del botón debe estar subrayado permanentemente.
    /// </summary>
    /// <param name="isUnderlined">True si debe estar subrayado permanentemente, false en caso contrario.</param>
    public void SetPermanentUnderline(bool isUnderlined)
    {
        if (tmpText != null)
        {
            // Si se debe establecer permanentemente subrayado y la negrita
            if (isUnderlined)
            {
                tmpText.fontStyle |= FontStyles.Underline | FontStyles.Bold;
            }
            else
            {
                tmpText.fontStyle &= ~(FontStyles.Underline | FontStyles.Bold);
            }

            isPermanentlyUnderlined = isUnderlined;
        }
    }

    /// <summary>
    /// Comprueba si el texto del botón está subrayado permanentemente.
    /// </summary>
    /// <returns>True si está subrayado permanentemente, false en caso contrario.</returns>
    public bool IsPermanentlyUnderlined()
    {
        return isPermanentlyUnderlined;
    }
}
