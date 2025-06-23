using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Clase que controla la visibilidad del handle de un <see cref="Scrollbar"/>
/// cuando el puntero entra o sale del área del <see cref="Scrollbar"/>.
/// </summary>
public class ShowHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Referencia al componente <see cref="Scrollbar"/> asignado desde el inspector.
    /// </summary>
    public Scrollbar scrollbar; // Asigna el scrollbar desde el inspector

    /// <summary>
    /// Referencia al objeto del handle del <see cref="Scrollbar"/>.
    /// </summary>
    private GameObject handle;

    /// <summary>
    /// Método llamado al inicio. Busca el componente <see cref="Scrollbar"/> en el objeto actual
    /// y oculta el handle si está presente.
    /// </summary>
    private void Start()
    {
        if (scrollbar == null)
        {
            scrollbar = GetComponent<Scrollbar>();
        }

        if (scrollbar != null && scrollbar.handleRect != null)
        {
            handle = scrollbar.handleRect.gameObject;
            handle.SetActive(false);
        }
    }

    /// <summary>
    /// Método llamado cuando el puntero entra en el área del <see cref="Scrollbar"/>.
    /// Activa la visibilidad del handle.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (handle != null)
        {
            handle.SetActive(true);
        }
    }

    /// <summary>
    /// Método llamado cuando el puntero sale del área del <see cref="Scrollbar"/>.
    /// Desactiva la visibilidad del handle.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (handle != null)
        {
            handle.SetActive(false);
        }
    }
}
