using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Clase que controla el comportamiento del zoom dependiendo de si el puntero del ratón
/// está sobre un ScrollView. Implementa las interfaces <see cref="IPointerEnterHandler"/> y <see cref="IPointerExitHandler"/>
/// para detectar eventos del puntero.
/// </summary>
public class StopZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Indica si el puntero del ratón está actualmente sobre el ScrollView.
    /// Este valor es utilizado por otras clases, como <c>ZoomController</c>, para habilitar o deshabilitar el zoom.
    /// </summary>
    public static bool IsPointerOverScrollView = false;

    /// <summary>
    /// Método llamado cuando el puntero del ratón entra en el área del ScrollView.
    /// Cambia el valor de <see cref="IsPointerOverScrollView"/> a <c>true</c>.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerOverScrollView = true;
        Debug.Log("Dentro de scroll");
    }

    /// <summary>
    /// Método llamado cuando el puntero del ratón sale del área del ScrollView.
    /// Cambia el valor de <see cref="IsPointerOverScrollView"/> a <c>false</c>.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerOverScrollView = false;
        Debug.Log("Fuera de scroll");
    }
}
