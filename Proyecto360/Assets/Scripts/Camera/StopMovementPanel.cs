using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente que detecta si el puntero del ratón está sobre el panel asociado.
/// Utilizado para controlar el comportamiento de otros sistemas, como el zoom de la cámara.
/// </summary>
public class StopMovementPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Indica si el puntero del ratón está actualmente sobre el Panel.
    /// Este valor es utilizado por otras clases, como mouseController, para habilitar o deshabilitar el zoom.
    /// </summary>
    public static bool IsPointerOverPanel = false;

    /// <summary>
    /// Método llamado cuando el puntero del ratón entra en el área del Panel.
    /// Cambia el valor de <see cref="IsPointerOverPanel"/> a <c>true</c>.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverPanel = true;
        Debug.Log("Dentro de scroll");
    }

    /// <summary>
    /// Método llamado cuando el puntero del ratón sale del área del Panel.
    /// Cambia el valor de <see cref="IsPointerOverPanel"/> a <c>false</c>.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverPanel = false;
        Debug.Log("Fuera de scroll");
    }
}
