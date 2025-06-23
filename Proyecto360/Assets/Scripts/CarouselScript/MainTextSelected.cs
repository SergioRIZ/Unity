using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clase que gestiona el comportamiento visual del texto principal de un botón en un carrusel,
/// permitiendo subrayar el texto al interactuar con el puntero y controlar si el subrayado es permanente.
/// Implementa las interfaces <see cref="IPointerEnterHandler"/> y <see cref="IPointerExitHandler"/> para detectar eventos del puntero.
/// </summary>
public class MainTextSelected : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Referencia al componente <see cref="TextMeshProUGUI"/> hijo del botón.
    /// </summary>
    private TextMeshProUGUI tmpText;

    /// <summary>
    /// Indica si el texto del botón debe permanecer subrayado de forma permanente.
    /// Si es <c>true</c>, el subrayado no se modifica al interactuar con el puntero.
    /// </summary>
    public bool isPermanentlyUnderlined = false;

    /// <summary>
    /// Inicializa el script obteniendo la referencia al componente <see cref="TextMeshProUGUI"/> hijo.
    /// </summary>
    private void Awake()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Se ejecuta cuando el puntero entra en el área del botón.
    /// Si el subrayado no es permanente, activa el texto.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        tmpText.enabled = true;
    }

    /// <summary>
    /// Se ejecuta cuando el puntero sale del área del botón.
    /// Si el subrayado no es permanente, desactiva el texto.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        tmpText.enabled = false;
    }
}
