using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Permite un desplazamiento vertical más rápido en un <see cref="ScrollRect"/> al usar la rueda del ratón.
/// </summary>
public class FastScrollRect : MonoBehaviour, IScrollHandler
{
    /// <summary>
    /// Referencia al componente <see cref="ScrollRect"/> que se va a controlar.
    /// </summary>
    public ScrollRect scrollRect;

    /// <summary>
    /// Velocidad de desplazamiento multiplicada al usar la rueda del ratón.
    /// </summary>
    public float scrollSpeed = 20f; // Aumenta este valor si quieres más velocidad

    /// <summary>
    /// Maneja el evento de desplazamiento del ratón y ajusta la posición vertical del <see cref="ScrollRect"/>.
    /// </summary>
    /// <param name="data">Datos del evento de desplazamiento.</param>
    public void OnScroll(PointerEventData data)
    {
        if (scrollRect == null) return;

        float scrollDelta = data.scrollDelta.y * scrollSpeed * Time.deltaTime;
        scrollRect.verticalNormalizedPosition += scrollDelta;
    }
}
    